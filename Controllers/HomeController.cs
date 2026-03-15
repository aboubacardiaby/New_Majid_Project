using GambianMuslimCommunity.Models;
using GambianMuslimCommunity.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GambianMuslimCommunity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICommunityService _communityService;
        private readonly IPayPalService _payPalService;
        private readonly IContributionTrackerService _contributionTrackerService;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, ICommunityService communityService, IPayPalService payPalService, IContributionTrackerService contributionTrackerService, IEmailService emailService)
        {
            _logger = logger;
            _communityService = communityService;
            _payPalService = payPalService;
            _contributionTrackerService = contributionTrackerService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                Services = await _communityService.GetActiveServicesAsync(),
                UpcomingEvents = await _communityService.GetUpcomingEventsAsync(3),
                PrayerSchedule = await _communityService.GetPrayerScheduleAsync(),
                FeaturedMasjidProject = await _communityService.GetFeaturedMasjidProjectAsync(),
                ImamWelcome = await _communityService.GetImamWelcomeMessageAsync()
            };
            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> Services()
        {
            var services = await _communityService.GetActiveServicesAsync();
            return View(services);
        }

        public async Task<IActionResult> Events()
        {
            var events = await _communityService.GetUpcomingEventsAsync();
            return View(events);
        }

        public async Task<IActionResult> PrayerTimes()
        {
            var prayerSchedule = await _communityService.GetPrayerScheduleAsync();
            return View(prayerSchedule);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactMessage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                var saved = await _communityService.SaveContactMessageAsync(model);
                
                if (saved)
                {
                    _logger.LogInformation("Contact message received from {Name} ({Email}): {Subject}", 
                        model.Name, model.Email, model.Subject);
                    
                    TempData["SuccessMessage"] = "Thank you for your message! We will get back to you soon, Insha'Allah.";
                    return RedirectToAction(nameof(Contact));
                }
                else
                {
                    ModelState.AddModelError("", "There was an error saving your message. Please try again.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> MasjidProject(int? id)
        {
            MasjidProject? project;
            
            if (id.HasValue)
            {
                project = await _communityService.GetMasjidProjectByIdAsync(id.Value);
            }
            else
            {
                project = await _communityService.GetFeaturedMasjidProjectAsync();
            }

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpGet]
        public async Task<IActionResult> Donate(int id)
        {
            var project = await _communityService.GetMasjidProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var donation = new MasjidDonation
            {
                MasjidProjectId = id,
                MasjidProject = project
            };

            return View(donation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(MasjidDonation donation)
        {
            if (ModelState.IsValid)
            {
                var saved = await _communityService.SaveDonationAsync(donation);
                
                if (saved)
                {
                    // Update contribution tracker for non-PayPal donations
                    await _contributionTrackerService.UpdateContributionTrackerAsync(
                        donation.DonorName ?? "Anonymous", 
                        donation.Email ?? "", 
                        donation.Amount, 
                        "Direct");

                    _logger.LogInformation("Donation received: {Amount} from {DonorName} for project {ProjectId}", 
                        donation.Amount, donation.DonorName, donation.MasjidProjectId);
                    
                    TempData["SuccessMessage"] = $"Jazakallahu Khayran! Your donation of ${donation.Amount:F2} has been received. May Allah (SWT) reward you abundantly.";
                    return RedirectToAction(nameof(MasjidProject), new { id = donation.MasjidProjectId });
                }
                else
                {
                    ModelState.AddModelError("", "There was an error processing your donation. Please try again.");
                }
            }

            // Reload project data if validation fails
            donation.MasjidProject = await _communityService.GetMasjidProjectByIdAsync(donation.MasjidProjectId);
            return View(donation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayPalPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var project = await _communityService.GetMasjidProjectByIdAsync(request.MasjidProjectId);
                if (project == null)
                {
                    return Json(new PaymentResult { Success = false, Message = "Project not found" });
                }

                // Save pending donation FIRST so we have its DB Id for the cancel URL
                var donation = new MasjidDonation
                {
                    MasjidProjectId = request.MasjidProjectId,
                    DonorName = request.DonorName,
                    Email = request.Email,
                    Amount = request.Amount,
                    Message = request.Message,
                    IsAnonymous = request.IsAnonymous,
                    PaymentMethod = "PayPal",
                    PaymentStatus = "Pending",
                    DonationDate = DateTime.Now
                };

                var saved = await _communityService.SaveDonationAsync(donation);
                if (!saved)
                {
                    return Json(new PaymentResult { Success = false, Message = "Failed to save donation" });
                }

                // Create PayPal payment — embed donationId in cancel URL so we can mark it Cancelled
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var returnUrl = $"{baseUrl}/Home/PayPalSuccess";
                var cancelUrl = $"{baseUrl}/Home/PayPalCancel?donationId={donation.Id}";

                var payment = _payPalService.CreatePayment(
                    request.Amount,
                    "USD",
                    $"Donation to {project.Title} by {request.DonorName}",
                    returnUrl,
                    cancelUrl
                );

                // Update donation with the PayPal payment Id
                donation.PayPalPaymentId = payment.id;
                await _communityService.UpdateDonationAsync(donation);

                // Get approval URL
                var approvalUrl = payment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

                return Json(new PaymentResult
                {
                    Success = true,
                    PaymentId = payment.id,
                    ApprovalUrl = approvalUrl,
                    Message = "Payment created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment");
                return Json(new PaymentResult { Success = false, Message = "Payment creation failed" });
            }
        }

        public async Task<IActionResult> PayPalSuccess(string paymentId, string PayerID)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(PayerID))
                {
                    TempData["ErrorMessage"] = "Invalid payment parameters";
                    return RedirectToAction(nameof(MasjidProject));
                }

                // Execute PayPal payment
                var executedPayment = _payPalService.ExecutePayment(paymentId, PayerID);

                // Find the donation in database
                var donation = await _communityService.GetDonationByPayPalPaymentIdAsync(paymentId);
                if (donation == null)
                {
                    TempData["ErrorMessage"] = "Donation record not found";
                    return RedirectToAction(nameof(MasjidProject));
                }

                // Idempotency guard: if already processed (e.g. user refreshed success page), skip re-execution
                if (donation.PaymentStatus == "Completed")
                {
                    TempData["SuccessMessage"] = $"Jazakallahu Khayran! Your PayPal donation of ${donation.Amount:F2} has already been recorded.";
                    return RedirectToAction(nameof(MasjidProject), new { id = donation.MasjidProjectId });
                }

                // Validate payment
                if (_payPalService.ValidatePayment(executedPayment, donation.Amount))
                {
                    // Update donation status
                    donation.PayPalPayerId = PayerID;
                    donation.PaymentStatus = "Completed";
                    donation.TransactionId = executedPayment.transactions?.FirstOrDefault()?.related_resources?.FirstOrDefault()?.sale?.id ?? "";

                    var updated = await _communityService.UpdateDonationAsync(donation);
                    if (updated)
                    {
                        // Update contribution tracker
                        await _contributionTrackerService.UpdateContributionTrackerAsync(
                            donation.DonorName ?? "Anonymous",
                            donation.Email ?? "",
                            donation.Amount,
                            donation.PaymentMethod ?? "PayPal");

                        // Send receipt email
                        var project = await _communityService.GetMasjidProjectByIdAsync(donation.MasjidProjectId);
                        var emailSent = await _emailService.SendDonationReceiptAsync(donation, project!);

                        _logger.LogInformation("PayPal donation completed: {PaymentId} for ${Amount}", paymentId, donation.Amount);
                        return RedirectToAction(nameof(DonationSuccess), new { id = donation.Id, emailSent });
                    }
                }

                TempData["ErrorMessage"] = "Payment validation failed";
                return RedirectToAction(nameof(MasjidProject));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal success: {PaymentId}", paymentId);
                TempData["ErrorMessage"] = "Payment processing failed";
                return RedirectToAction(nameof(MasjidProject));
            }
        }

        public async Task<IActionResult> PayPalCancel(int? donationId, string? token)
        {
            _logger.LogInformation("PayPal payment cancelled. DonationId: {DonationId}, Token: {Token}", donationId, token);

            if (donationId.HasValue)
            {
                var donation = await _communityService.GetDonationByIdAsync(donationId.Value);
                if (donation != null && donation.PaymentStatus == "Pending")
                {
                    donation.PaymentStatus = "Cancelled";
                    await _communityService.UpdateDonationAsync(donation);
                }
            }

            TempData["ErrorMessage"] = "PayPal payment was cancelled";
            return RedirectToAction(nameof(MasjidProject));
        }

        public async Task<IActionResult> DonationSuccess(int id, bool emailSent = false)
        {
            var donation = await _communityService.GetDonationByIdAsync(id);
            if (donation == null)
                return NotFound();

            var project = await _communityService.GetMasjidProjectByIdAsync(donation.MasjidProjectId);

            var receiptNumber = $"GMC-{donation.Id:D6}-{donation.DonationDate:yyyyMMdd}";
            var progress = project != null && project.TargetAmount > 0
                ? Math.Round((project.CurrentAmount / project.TargetAmount) * 100, 1)
                : 0;

            var model = new DonationSuccessViewModel
            {
                DonorName    = donation.IsAnonymous ? "Anonymous Donor" : (donation.DonorName ?? "Valued Donor"),
                Email        = donation.Email ?? "",
                Amount       = donation.Amount,
                PaymentMethod = donation.PaymentMethod ?? "PayPal",
                TransactionId = donation.TransactionId ?? "",
                ReceiptNumber = receiptNumber,
                DonationDate  = donation.DonationDate,
                IsAnonymous   = donation.IsAnonymous,
                EmailSent     = emailSent,
                ProjectTitle  = project?.Title ?? "Masjid Building Project",
                ProjectId     = donation.MasjidProjectId,
                ProjectProgress = progress,
                TargetAmount    = project?.TargetAmount ?? 0,
                CurrentAmount   = project?.CurrentAmount ?? 0
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}