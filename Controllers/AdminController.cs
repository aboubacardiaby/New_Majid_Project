using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using GambianMuslimCommunity.Models;
using GambianMuslimCommunity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace GambianMuslimCommunity.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        private readonly IContributionTrackerService _contributionService;
        private readonly ICommunityService _communityService;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger, IContributionTrackerService contributionService, ICommunityService communityService)
        {
            _adminService = adminService;
            _logger = logger;
            _contributionService = contributionService;
            _communityService = communityService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            var model = new AdminLoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var admin = await _adminService.ValidateAdminAsync(model.Username, model.Password);
            if (admin == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim("FullName", admin.FullName),
                new Claim(ClaimTypes.Role, admin.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "AdminCookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in
            await HttpContext.SignInAsync("AdminCookies", claimsPrincipal);

            // Create session
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();
            var sessionToken = await _adminService.CreateSessionAsync(admin, ipAddress, userAgent);

            // Store session token in cookie
            Response.Cookies.Append("AdminSession", sessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(8)
            });

            // Log activity
            await _adminService.LogActivityAsync(admin.Id, "Login", "Admin", admin.Id, 
                "Admin login successful", ipAddress);

            _logger.LogInformation("Admin {Username} logged in successfully", admin.Username);

            // Redirect to return URL or dashboard
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var adminId = GetCurrentAdminId();
            var sessionToken = Request.Cookies["AdminSession"];

            if (!string.IsNullOrEmpty(sessionToken))
            {
                await _adminService.LogoutAsync(sessionToken);
            }

            if (adminId > 0)
            {
                await _adminService.LogActivityAsync(adminId, "Logout", "Admin", adminId, 
                    "Admin logout", HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            await HttpContext.SignOutAsync("AdminCookies");
            Response.Cookies.Delete("AdminSession");

            _logger.LogInformation("Admin logged out");

            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                Stats = await _adminService.GetDashboardStatsAsync(),
                RecentDonations = await _adminService.GetRecentDonationsAsync(10),
                RecentMessages = await _adminService.GetRecentMessagesAsync(10),
                RecentActivities = await _adminService.GetRecentActivitiesAsync(20),
                ActiveProjects = await _adminService.GetAllProjectsAsync()
            };

            await UpdateSessionActivity();

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var model = await LoadSettingsViewModel();
            await UpdateSessionActivity();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSiteInfo(AdminSettingsViewModel model)
        {
            // Only validate SiteInfo properties
            var siteInfoValidationErrors = ModelState
                .Where(x => x.Key.StartsWith("SiteInfo."))
                .Where(x => x.Value.Errors.Count > 0)
                .ToList();

            if (siteInfoValidationErrors.Any())
            {
                // Reload settings to repopulate the view model
                var viewModel = await LoadSettingsViewModel();
                viewModel.SiteInfo = model.SiteInfo; // Keep the user's input
                
                // Extract SiteInfo validation errors
                var siteInfoErrors = siteInfoValidationErrors
                    .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();
                
                if (siteInfoErrors.Any())
                {
                    TempData["ErrorMessage"] = "Site Information errors: " + string.Join(", ", siteInfoErrors);
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
                
                return View("Settings", viewModel);
            }

            var siteInfoModel = model.SiteInfo;

            var adminUsername = User.Identity?.Name ?? "Unknown";
            var adminId = GetCurrentAdminId();

            var updates = new Dictionary<string, (string Value, string Category)>
            {
                ["SiteName"] = (siteInfoModel.SiteName, "General"),
                ["SiteDescription"] = (siteInfoModel.SiteDescription, "General"),
                ["ContactEmail"] = (siteInfoModel.ContactEmail, "Contact"),
                ["ContactPhone"] = (siteInfoModel.ContactPhone, "Contact"),
                ["Address"] = (siteInfoModel.Address, "Contact"),
                ["City"] = (siteInfoModel.City, "Contact"),
                ["State"] = (siteInfoModel.State, "Contact"),
                ["ZipCode"] = (siteInfoModel.ZipCode, "Contact"),
                ["LogoUrl"] = (siteInfoModel.LogoUrl, "General"),
                ["ImamName"] = (siteInfoModel.ImamName, "General"),
                ["ImamWelcomeMessage"] = (siteInfoModel.ImamWelcomeMessage, "General"),
                ["ImamImageUrl"] = (siteInfoModel.ImamImageUrl, "General"),
                ["ImamTitle"] = (siteInfoModel.ImamTitle, "General")
            };

            var success = true;
            foreach (var update in updates)
            {
                if (!await _adminService.UpdateSettingAsync(update.Key, update.Value.Value, update.Value.Category, adminUsername))
                {
                    success = false;
                }
            }

            if (success)
            {
                await _adminService.LogActivityAsync(adminId, "Update Settings", "SiteSettings", null, 
                    "Updated site information settings", HttpContext.Connection.RemoteIpAddress?.ToString());
                
                TempData["SuccessMessage"] = "Site information updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "There was an error updating the settings. Please try again.";
            }

            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSocialMedia(AdminSettingsViewModel model)
        {
            // Only validate SocialMedia properties
            var socialMediaValidationErrors = ModelState
                .Where(x => x.Key.StartsWith("SocialMedia."))
                .Where(x => x.Value.Errors.Count > 0)
                .ToList();

            if (socialMediaValidationErrors.Any())
            {
                // Reload settings to repopulate the view model
                var viewModel = await LoadSettingsViewModel();
                viewModel.SocialMedia = model.SocialMedia; // Keep the user's input
                
                // Extract social media validation errors
                var socialMediaErrors = socialMediaValidationErrors
                    .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();
                
                if (socialMediaErrors.Any())
                {
                    TempData["ErrorMessage"] = "Social Media errors: " + string.Join(", ", socialMediaErrors);
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
                
                return View("Settings", viewModel);
            }

            var adminUsername = User.Identity?.Name ?? "Unknown";
            var adminId = GetCurrentAdminId();

            var socialMediaModel = model.SocialMedia;
            var updates = new Dictionary<string, string>
            {
                ["FacebookUrl"] = socialMediaModel.FacebookUrl,
                ["InstagramUrl"] = socialMediaModel.InstagramUrl,
                ["WhatsAppNumber"] = socialMediaModel.WhatsAppNumber,
                ["TwitterHandle"] = socialMediaModel.TwitterHandle
            };

            var success = true;
            foreach (var update in updates)
            {
                if (!await _adminService.UpdateSettingAsync(update.Key, update.Value, "Social", adminUsername))
                {
                    success = false;
                }
            }

            if (success)
            {
                await _adminService.LogActivityAsync(adminId, "Update Settings", "SiteSettings", null, 
                    "Updated social media settings", HttpContext.Connection.RemoteIpAddress?.ToString());
                
                TempData["SuccessMessage"] = "Social media settings updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "There was an error updating the settings. Please try again.";
            }

            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmailSettings(AdminSettingsViewModel model)
        {
            // Only validate EmailSettings properties
            var emailValidationErrors = ModelState
                .Where(x => x.Key.StartsWith("EmailSettings."))
                .Where(x => x.Value.Errors.Count > 0)
                .ToList();

            if (emailValidationErrors.Any())
            {
                // Reload settings to repopulate the view model
                var viewModel = await LoadSettingsViewModel();
                viewModel.EmailSettings = model.EmailSettings; // Keep the user's input
                
                // Extract email settings validation errors
                var emailErrors = emailValidationErrors
                    .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();
                
                if (emailErrors.Any())
                {
                    TempData["ErrorMessage"] = "Email Settings errors: " + string.Join(", ", emailErrors);
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
                
                return View("Settings", viewModel);
            }

            var adminUsername = User.Identity?.Name ?? "Unknown";
            var adminId = GetCurrentAdminId();
            var emailModel = model.EmailSettings;
            var updates = new Dictionary<string, (string Value, string Category)>
            {
                ["SmtpServer"] = (emailModel.SmtpServer, "Email"),
                ["SmtpPort"] = (emailModel.SmtpPort.ToString(), "Email"),
                ["FromEmail"] = (emailModel.FromEmail, "Email"),
                ["FromName"] = (emailModel.FromName, "Email"),
                ["UseSSL"] = (emailModel.UseSSL.ToString(), "Email"),
                ["Username"] = (emailModel.Username, "Email")
            };

            // Only update password if provided
            if (!string.IsNullOrEmpty(emailModel.Password))
            {
                updates["Password"] = (emailModel.Password, "Email");
            }

            var success = true;
            foreach (var update in updates)
            {
                if (!await _adminService.UpdateSettingAsync(update.Key, update.Value.Value, update.Value.Category, adminUsername))
                {
                    success = false;
                }
            }

            if (success)
            {
                await _adminService.LogActivityAsync(adminId, "Update Settings", "EmailSettings", null, 
                    "Updated email settings", HttpContext.Connection.RemoteIpAddress?.ToString());
                
                TempData["SuccessMessage"] = "Email settings updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "There was an error updating the settings. Please try again.";
            }

            return RedirectToAction(nameof(Settings));
        }

        [Authorize]
        public async Task<IActionResult> Donations(DonationFilterOptions? filters = null, int page = 1)
        {
            const int pageSize = 20;
            
            var donations = await _adminService.GetDonationsAsync(filters, page, pageSize);
            var totalCount = await _adminService.GetDonationsCountAsync(filters);
            var totalAmount = await _adminService.GetDonationsTotalAsync(filters);

            var model = new AdminDonationManagementViewModel
            {
                Donations = donations,
                FilterOptions = filters ?? new DonationFilterOptions(),
                TotalAmount = totalAmount,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            await UpdateSessionActivity();
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetDonationDetails(int id)
        {
            var donation = await _adminService.GetDonationByIdAsync(id);
            if (donation == null)
            {
                return NotFound();
            }

            return PartialView("_DonationDetailsPartial", donation);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendThankYouEmail(int id)
        {
            var success = await _adminService.SendThankYouEmailAsync(id);
            return Json(new { success, message = success ? "Thank you email sent successfully!" : "Failed to send email" });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] PaymentStatusUpdateRequest request)
        {
            var success = await _adminService.UpdatePaymentStatusAsync(id, request.Status);
            return Json(new { success, message = success ? "Payment status updated successfully!" : "Failed to update status" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportDonations([FromQuery] DonationFilterOptions? filters = null)
        {
            var donations = await _adminService.GetAllDonationsAsync(filters);
            
            // Create CSV content
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,Donor Name,Email,Project,Amount,Payment Method,Payment Status,Transaction ID,Message");
            
            foreach (var donation in donations)
            {
                csv.AppendLine($"{donation.DonationDate:yyyy-MM-dd HH:mm:ss}," +
                             $"\"{(donation.IsAnonymous ? "Anonymous" : donation.DonorName)}\"," +
                             $"\"{donation.Email}\"," +
                             $"\"{donation.MasjidProject.Title}\"," +
                             $"{donation.Amount}," +
                             $"\"{donation.PaymentMethod}\"," +
                             $"\"{donation.PaymentStatus}\"," +
                             $"\"{donation.TransactionId}\"," +
                             $"\"{donation.Message?.Replace("\"", "\"\"")}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"Donations_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(bytes, "text/csv", fileName);
        }

        [Authorize]
        public async Task<IActionResult> Messages(MessageFilterOptions? filters = null, int page = 1)
        {
            const int pageSize = 20;
            
            var messages = await _adminService.GetMessagesAsync(filters, page, pageSize);
            var totalCount = await _adminService.GetMessagesCountAsync(filters);

            var model = new AdminMessageManagementViewModel
            {
                Messages = messages,
                FilterOptions = filters ?? new MessageFilterOptions(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                UnreadCount = await _adminService.GetMessagesCountAsync(new MessageFilterOptions { IsRead = false })
            };

            await UpdateSessionActivity();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkMessageRead(int id)
        {
            var adminUsername = User.Identity?.Name ?? "Unknown";
            var success = await _adminService.MarkMessageAsReadAsync(id, adminUsername);
            
            return Json(new { success });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var adminUsername = User.Identity?.Name ?? "Unknown";
            var success = await _adminService.DeleteMessageAsync(id, adminUsername);
            
            return Json(new { success });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMessageDetails(int id)
        {
            var message = await _adminService.GetMessageByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            // Mark as read if not already read
            if (!message.IsRead)
            {
                var adminUsername = User.Identity?.Name ?? "Unknown";
                await _adminService.MarkMessageAsReadAsync(id, adminUsername);
            }

            return PartialView("_MessageDetailsPartial", message);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMessageForReply(int id)
        {
            var message = await _adminService.GetMessageByIdAsync(id);
            if (message == null)
            {
                return Json(new { success = false, message = "Message not found" });
            }

            return Json(new { 
                success = true, 
                name = message.Name,
                email = message.Email, 
                subject = message.Subject,
                message = message.Message,
                date = message.DateSent.ToString("MMM dd, yyyy 'at' h:mm tt")
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyToMessage([FromBody] ReplyMessageRequest request)
        {
            var success = await _adminService.ReplyToMessageAsync(request.MessageId, request.Subject, request.Message, User.Identity?.Name ?? "Unknown");
            return Json(new { success, message = success ? "Reply sent successfully!" : "Failed to send reply" });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] BulkMessageRequest request)
        {
            var adminUsername = User.Identity?.Name ?? "Unknown";
            var successCount = 0;
            
            foreach (var messageId in request.MessageIds)
            {
                if (await _adminService.MarkMessageAsReadAsync(messageId, adminUsername))
                {
                    successCount++;
                }
            }

            return Json(new { 
                success = successCount > 0, 
                message = $"{successCount} of {request.MessageIds.Count} messages marked as read" 
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessages([FromBody] BulkMessageRequest request)
        {
            var adminUsername = User.Identity?.Name ?? "Unknown";
            var successCount = 0;
            
            foreach (var messageId in request.MessageIds)
            {
                if (await _adminService.DeleteMessageAsync(messageId, adminUsername))
                {
                    successCount++;
                }
            }

            return Json(new { 
                success = successCount > 0, 
                message = $"{successCount} of {request.MessageIds.Count} messages deleted" 
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllMessagesAsRead()
        {
            var success = await _adminService.MarkAllMessagesAsReadAsync(User.Identity?.Name ?? "Unknown");
            return Json(new { success, message = success ? "All messages marked as read" : "Failed to mark all messages as read" });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMassMessage([FromBody] MassMessageRequest request)
        {
            var adminUsername = User.Identity?.Name ?? "Unknown";
            var success = await _adminService.SendMassMessageAsync(request.Subject, request.Message, request.RecipientType, request.TestMode, adminUsername);
            
            string message = success ? 
                (request.TestMode ? "Test message sent successfully to your email" : "Mass message sent successfully to the community") : 
                "Failed to send mass message";
            
            return Json(new { success, message });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRecipientCount(string type = "all")
        {
            var count = await _adminService.GetRecipientCountAsync(type);
            return Json(new { success = true, count });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportMessages([FromQuery] MessageFilterOptions? filters = null)
        {
            var messages = await _adminService.GetAllMessagesAsync(filters);
            
            // Create CSV content
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,Name,Email,Subject,Message,Status,Read Date,Read By");
            
            foreach (var message in messages)
            {
                csv.AppendLine($"{message.DateSent:yyyy-MM-dd HH:mm:ss}," +
                             $"\"{message.Name}\"," +
                             $"\"{message.Email}\"," +
                             $"\"{message.Subject}\"," +
                             $"\"{message.Message?.Replace("\"", "\"\"")}\"," +
                             $"{(message.IsRead ? "Read" : "Unread")}," +
                             $"{message.ReadDate:yyyy-MM-dd HH:mm:ss}," +
                             $"\"{message.ReadBy}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"Messages_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(bytes, "text/csv", fileName);
        }

        [Authorize]
        public async Task<IActionResult> Projects()
        {
            var projects = await _adminService.GetAllProjectsAsync();
            
            var model = new AdminProjectManagementViewModel
            {
                Projects = projects,
                TotalRaised = projects.Sum(p => p.CurrentAmount),
                TotalDonators = await _adminService.GetDonationsCountAsync()
            };

            await UpdateSessionActivity();
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult CreateProject()
        {
            return View(new MasjidProject());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(MasjidProject project)
        {
            if (ModelState.IsValid)
            {
                var adminUsername = User.Identity?.Name ?? "Unknown";
                var success = await _adminService.CreateProjectAsync(project, adminUsername);
                
                if (success)
                {
                    var adminId = GetCurrentAdminId();
                    await _adminService.LogActivityAsync(adminId, "Create Project", "MasjidProject", null, 
                        $"Created project: {project.Title}", HttpContext.Connection.RemoteIpAddress?.ToString());
                    
                    TempData["SuccessMessage"] = "Project created successfully!";
                    return RedirectToAction(nameof(Projects));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create project. Please try again.";
                }
            }

            return View(project);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProject(int id)
        {
            var project = await _adminService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(MasjidProject project)
        {
            if (ModelState.IsValid)
            {
                var adminUsername = User.Identity?.Name ?? "Unknown";
                var success = await _adminService.UpdateProjectAsync(project, adminUsername);
                
                if (success)
                {
                    var adminId = GetCurrentAdminId();
                    await _adminService.LogActivityAsync(adminId, "Update Project", "MasjidProject", project.Id, 
                        $"Updated project: {project.Title}", HttpContext.Connection.RemoteIpAddress?.ToString());
                    
                    TempData["SuccessMessage"] = "Project updated successfully!";
                    return RedirectToAction(nameof(Projects));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update project. Please try again.";
                }
            }

            return View(project);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var adminUsername = User.Identity?.Name ?? "Unknown";
            var success = await _adminService.DeleteProjectAsync(id, adminUsername);
            
            if (success)
            {
                var adminId = GetCurrentAdminId();
                await _adminService.LogActivityAsync(adminId, "Delete Project", "MasjidProject", id, 
                    "Project deleted", HttpContext.Connection.RemoteIpAddress?.ToString());
                
                return Json(new { success = true, message = "Project deleted successfully!" });
            }
            
            return Json(new { success = false, message = "Failed to delete project" });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProjectStatus(int id)
        {
            var project = await _adminService.GetProjectByIdAsync(id);
            if (project != null)
            {
                project.IsActive = !project.IsActive;
                var adminUsername = User.Identity?.Name ?? "Unknown";
                var success = await _adminService.UpdateProjectAsync(project, adminUsername);
                
                if (success)
                {
                    var adminId = GetCurrentAdminId();
                    await _adminService.LogActivityAsync(adminId, "Toggle Project Status", "MasjidProject", id, 
                        $"Project status changed to: {(project.IsActive ? "Active" : "Inactive")}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    
                    return Json(new { 
                        success = true, 
                        message = $"Project {(project.IsActive ? "activated" : "deactivated")} successfully!",
                        isActive = project.IsActive
                    });
                }
            }
            
            return Json(new { success = false, message = "Failed to update project status" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ProjectDetails(int id)
        {
            var project = await _adminService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var projectDonations = await _adminService.GetDonationsAsync(
                new DonationFilterOptions { ProjectId = id }, 1, 100);

            return PartialView("_ProjectDetailsPartial", new AdminProjectManagementViewModel
            {
                EditingProject = project,
                ProjectDonations = projectDonations,
                TotalRaised = project.CurrentAmount,
                TotalDonators = projectDonations.Count
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeaturedProject(int id)
        {
            var project = await _adminService.GetProjectByIdAsync(id);
            if (project != null)
            {
                // If setting this project as featured, unset others first
                if (!project.IsFeatured)
                {
                    await _adminService.UnsetAllFeaturedProjectsAsync();
                }
                
                project.IsFeatured = !project.IsFeatured;
                var adminUsername = User.Identity?.Name ?? "Unknown";
                var success = await _adminService.UpdateProjectAsync(project, adminUsername);
                
                if (success)
                {
                    var adminId = GetCurrentAdminId();
                    await _adminService.LogActivityAsync(adminId, "Toggle Featured Project", "MasjidProject", id, 
                        $"Project featured status changed to: {(project.IsFeatured ? "Featured" : "Not Featured")}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    
                    return Json(new { 
                        success = true, 
                        message = $"Project {(project.IsFeatured ? "set as featured" : "removed from featured")} successfully!",
                        isFeatured = project.IsFeatured
                    });
                }
            }
            
            return Json(new { success = false, message = "Failed to update featured status" });
        }

        [Authorize]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Contribution Management

        [Authorize]
        public async Task<IActionResult> Contributions(ContributionFilterOptions? filters = null, int page = 1)
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

            await UpdateSessionActivity();
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ContributionAnalytics(string period = "month")
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecalculateContributionTrackers()
        {
            var success = await _contributionService.RecalculateContributionTrackersAsync();
            
            if (success)
            {
                var adminId = GetCurrentAdminId();
                await _adminService.LogActivityAsync(adminId, "Recalculate Trackers", "ContributionTracker", null,
                    "Recalculated all contribution trackers", HttpContext.Connection.RemoteIpAddress?.ToString());
                    
                TempData["SuccessMessage"] = "Contribution trackers have been recalculated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to recalculate contribution trackers.";
            }

            return RedirectToAction("Contributions");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ExportContributions([FromQuery] ContributionFilterOptions? filters = null)
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
                    var adminId = GetCurrentAdminId();
                    await _adminService.LogActivityAsync(adminId, "Add Manual Contribution", "MasjidDonation", null,
                        $"Added manual contribution: ${model.Amount} from {model.ContributorName}", 
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                        
                    _logger.LogInformation("Manual contribution added: {Amount} from {Name} ({Email})", 
                        model.Amount, model.ContributorName, model.Email);
                    
                    TempData["SuccessMessage"] = $"Manual contribution of ${model.Amount:F2} for {model.ContributorName} has been added successfully.";
                    return RedirectToAction("Contributions");
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

        #endregion

        #region Helper Methods

        private async Task<AdminSettingsViewModel> LoadSettingsViewModel()
        {
            var model = new AdminSettingsViewModel
            {
                SettingsGroups = await _adminService.GetSettingsGroupedAsync()
            };

            // Populate typed settings
            var settings = await _adminService.GetAllSettingsAsync();
            var settingsDict = settings.ToDictionary(s => $"{s.Category}:{s.SettingKey}", s => s.SettingValue);

            model.SiteInfo = new SiteInfo
            {
                SiteName = settingsDict.GetValueOrDefault("General:SiteName", ""),
                SiteDescription = settingsDict.GetValueOrDefault("General:SiteDescription", ""),
                ContactEmail = settingsDict.GetValueOrDefault("Contact:ContactEmail", ""),
                ContactPhone = settingsDict.GetValueOrDefault("Contact:ContactPhone", ""),
                Address = settingsDict.GetValueOrDefault("Contact:Address", ""),
                City = settingsDict.GetValueOrDefault("Contact:City", ""),
                State = settingsDict.GetValueOrDefault("Contact:State", ""),
                ZipCode = settingsDict.GetValueOrDefault("Contact:ZipCode", ""),
                LogoUrl = settingsDict.GetValueOrDefault("General:LogoUrl", ""),
                ImamName = settingsDict.GetValueOrDefault("General:ImamName", ""),
                ImamWelcomeMessage = settingsDict.GetValueOrDefault("General:ImamWelcomeMessage", ""),
                ImamImageUrl = settingsDict.GetValueOrDefault("General:ImamImageUrl", ""),
                ImamTitle = settingsDict.GetValueOrDefault("General:ImamTitle", "")
            };

            model.SocialMedia = new SocialMediaSettings
            {
                FacebookUrl = settingsDict.GetValueOrDefault("Social:FacebookUrl", ""),
                InstagramUrl = settingsDict.GetValueOrDefault("Social:InstagramUrl", ""),
                WhatsAppNumber = settingsDict.GetValueOrDefault("Social:WhatsAppNumber", ""),
                TwitterHandle = settingsDict.GetValueOrDefault("Social:TwitterHandle", "")
            };

            model.EmailSettings = new EmailSettings
            {
                SmtpServer = settingsDict.GetValueOrDefault("Email:SmtpServer", ""),
                SmtpPort = int.TryParse(settingsDict.GetValueOrDefault("Email:SmtpPort", "587"), out var port) ? port : 587,
                FromEmail = settingsDict.GetValueOrDefault("Email:FromEmail", ""),
                FromName = settingsDict.GetValueOrDefault("Email:FromName", ""),
                UseSSL = bool.TryParse(settingsDict.GetValueOrDefault("Email:UseSSL", "true"), out var ssl) && ssl,
                Username = settingsDict.GetValueOrDefault("Email:Username", "")
                // Note: Password is intentionally not loaded for security
            };

            return model;
        }

        private int GetCurrentAdminId()
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return adminIdClaim != null ? int.Parse(adminIdClaim.Value) : 0;
        }

        private async Task UpdateSessionActivity()
        {
            var sessionToken = Request.Cookies["AdminSession"];
            if (!string.IsNullOrEmpty(sessionToken))
            {
                await _adminService.UpdateLastActivityAsync(sessionToken);
            }
        }

        #endregion
    }
}