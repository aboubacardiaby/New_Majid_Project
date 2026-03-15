using GambianMuslimCommunity.Models;
using GambianMuslimCommunity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GambianMuslimCommunity.Controllers
{
    public class ContributionController : Controller
    {
        private readonly IContributionTrackerService _contributionService;
        private readonly ILogger<ContributionController> _logger;
        private readonly ICommunityService _communityService;

        public ContributionController(IContributionTrackerService contributionService, ILogger<ContributionController> logger, ICommunityService communityService)
        {
            _contributionService = contributionService;
            _logger = logger;
            _communityService = communityService;
        }

        // Public contributor lookup
        [HttpGet]
        public IActionResult Lookup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Lookup(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Please enter an email address.";
                return View();
            }

            var summary = await _contributionService.GetContributorSummaryAsync(email);
            if (summary == null)
            {
                TempData["ErrorMessage"] = "No contribution records found for this email address.";
                return View();
            }

            return RedirectToAction("Report", new { email = email });
        }

        [HttpGet]
        public async Task<IActionResult> Report(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Lookup");
            }

            var report = await _contributionService.GetContributorReportAsync(email);
            if (report.Summary == null || string.IsNullOrEmpty(report.Summary.Email))
            {
                TempData["ErrorMessage"] = "No contribution records found for this email address.";
                return RedirectToAction("Lookup");
            }

            return View(report);
        }

        [HttpGet]
        public async Task<IActionResult> Recognition(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }

            var recognition = await _contributionService.GetContributorRecognitionLevelAsync(email);
            return Json(recognition);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }

            var reportData = await _contributionService.ExportIndividualReportAsync(email);
            var fileName = $"Contribution_Report_{email.Replace("@", "_at_")}_{DateTime.Now:yyyyMMdd}.csv";
            
            return File(reportData, "text/csv", fileName);
        }

        // Admin-only routes
        [Authorize]
        public async Task<IActionResult> Admin(ContributionFilterOptions? filters = null, int page = 1)
        {
            const int pageSize = 20;
            
            var contributors = await _contributionService.GetAllContributorsAsync(filters, page, pageSize);
            var totalCount = await _contributionService.GetContributorsCountAsync(filters);
            
            var weeklyAnalytics = await _contributionService.GetWeeklyAnalyticsAsync();
            var monthlyAnalytics = await _contributionService.GetMonthlyAnalyticsAsync();
            var yearlyAnalytics = await _contributionService.GetYearlyAnalyticsAsync();

            var model = new ContributionTrackerViewModel
            {
                Contributors = contributors,
                WeeklyAnalytics = weeklyAnalytics,
                MonthlyAnalytics = monthlyAnalytics,
                YearlyAnalytics = yearlyAnalytics,
                FilterOptions = filters ?? new ContributionFilterOptions(),
                TotalContributors = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Analytics(string period = "month")
        {
            ContributionAnalytics analytics = period.ToLower() switch
            {
                "week" => await _contributionService.GetWeeklyAnalyticsAsync(),
                "year" => await _contributionService.GetYearlyAnalyticsAsync(),
                _ => await _contributionService.GetMonthlyAnalyticsAsync()
            };

            return Json(analytics);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> TopContributors(string period = "month", int count = 10)
        {
            var topContributors = await _contributionService.GetTopContributorsAsync(period, count, false);
            return Json(topContributors);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RecalculateTrackers()
        {
            var success = await _contributionService.RecalculateContributionTrackersAsync();
            
            if (success)
            {
                TempData["SuccessMessage"] = "Contribution trackers have been recalculated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to recalculate contribution trackers.";
            }

            return RedirectToAction("Admin");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ExportAll([FromQuery] ContributionFilterOptions? filters = null)
        {
            var reportData = await _contributionService.ExportContributionReportAsync(filters);
            var fileName = $"All_Contributors_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(reportData, "text/csv", fileName);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ContributorDetails(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return NotFound();
            }

            var report = await _contributionService.GetContributorReportAsync(email);
            if (report.Summary == null || string.IsNullOrEmpty(report.Summary.Email))
            {
                return NotFound();
            }

            return PartialView("_ContributorDetailsPartial", report);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddContribution()
        {
            var model = new ManualContribution
            {
                ContributionDate = DateTime.Now,
                AvailableProjects = await GetAvailableProjectsAsync(),
                PaymentMethods = GetPaymentMethods()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContribution(ManualContribution model)
        {
            if (ModelState.IsValid)
            {
                var success = await _contributionService.AddManualContributionAsync(
                    model.ContributorName,
                    model.Email,
                    model.Amount,
                    model.ProjectName,
                    model.PaymentMethod,
                    model.ContributionDate,
                    model.Notes);

                if (success)
                {
                    _logger.LogInformation("Manual contribution added: {Amount} from {Name} ({Email})", 
                        model.Amount, model.ContributorName, model.Email);
                    
                    TempData["SuccessMessage"] = $"Manual contribution of ${model.Amount:F2} for {model.ContributorName} has been added successfully.";
                    return RedirectToAction("Admin");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to add the contribution. Please try again.");
                }
            }

            // Reload dropdown data if validation fails
            model.AvailableProjects = await GetAvailableProjectsAsync();
            model.PaymentMethods = GetPaymentMethods();
            return View(model);
        }

        private async Task<List<string>> GetAvailableProjectsAsync()
        {
            try
            {
                var projects = await _communityService.GetActiveMasjidProjectsAsync();
                var projectNames = projects?.Select(p => p.Title).ToList() ?? new List<string>();
                
                // Add common project names if no projects found or to supplement existing ones
                var commonProjects = new List<string>
                {
                    "General Fund",
                    "Masjid Construction",
                    "Ramadan Support",
                    "Education Fund",
                    "Community Events",
                    "Iftar Program",
                    "Zakat Distribution",
                    "Emergency Relief"
                };

                // Combine and remove duplicates
                projectNames.AddRange(commonProjects);
                return projectNames.Distinct().ToList();
            }
            catch
            {
                // Fallback to common project names if service call fails
                return new List<string>
                {
                    "General Fund",
                    "Masjid Construction",
                    "Ramadan Support",
                    "Education Fund",
                    "Community Events",
                    "Iftar Program",
                    "Zakat Distribution",
                    "Emergency Relief"
                };
            }
        }

        private List<string> GetPaymentMethods()
        {
            return new List<string>
            {
                "Cash",
                "Check",
                "Bank Transfer",
                "Credit Card",
                "PayPal",
                "Zelle",
                "Venmo",
                "Other"
            };
        }
    }
}