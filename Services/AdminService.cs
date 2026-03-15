using System.Text;
using GambianMuslimCommunity.Data;
using System.Security.Cryptography;
using GambianMuslimCommunity.Models;
using Microsoft.EntityFrameworkCore;

namespace GambianMuslimCommunity.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Authentication Methods

        public async Task<AdminUser?> ValidateAdminAsync(string username, string password)
        {
            try
            {
                var admin = await _context.AdminUsers
                    .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);

                admin.LastLoginDate = DateTime.Now;
              //  string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                //return admin;
                if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash)) //--temporary removal of password check for ease of testing
                {
                    admin.LastLoginDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return admin;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AdminUser?> GetAdminByIdAsync(int id)
        {
            return await _context.AdminUsers
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        }

        public async Task<AdminUser?> GetAdminByUsernameAsync(string username)
        {
            return await _context.AdminUsers
                .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);
        }

        public async Task<string> CreateSessionAsync(AdminUser admin, string ipAddress, string userAgent)
        {
            try
            {
                var sessionToken = GenerateSessionToken();
                
                var session = new AdminSession
                {
                    AdminUserId = admin.Id,
                    SessionToken = sessionToken,
                    LoginDate = DateTime.Now,
                    LastActivity = DateTime.Now,
                    IsActive = true,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _context.AdminSessions.Add(session);
                await _context.SaveChangesAsync();
                
                return sessionToken;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<AdminUser?> ValidateSessionAsync(string sessionToken)
        {
            try
            {
                var session = await _context.AdminSessions
                    .Include(s => s.AdminUser)
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);

                if (session != null && session.AdminUser.IsActive)
                {
                    // Update last activity
                    session.LastActivity = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    return session.AdminUser;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> LogoutAsync(string sessionToken)
        {
            try
            {
                var session = await _context.AdminSessions
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);
                
                if (session != null)
                {
                    session.IsActive = false;
                    await _context.SaveChangesAsync();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateLastActivityAsync(string sessionToken)
        {
            try
            {
                var session = await _context.AdminSessions
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);
                
                if (session != null)
                {
                    session.LastActivity = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                
                return true;
            }
            catch(Exception exec)
            {
                return false;
            }
        }

        #endregion

        #region Admin Management

        public async Task<List<AdminUser>> GetActiveAdminsAsync()
        {
            return await _context.AdminUsers
                .Where(a => a.IsActive)
                .OrderBy(a => a.FullName)
                .ToListAsync();
        }

        public async Task<bool> CreateAdminAsync(AdminUser admin, string password)
        {
            try
            {
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                admin.CreatedDate = DateTime.Now;
                
                _context.AdminUsers.Add(admin);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAdminAsync(AdminUser admin)
        {
            try
            {
                _context.AdminUsers.Update(admin);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateAdminAsync(int id, string deactivatedBy)
        {
            try
            {
                var admin = await _context.AdminUsers.FindAsync(id);
                if (admin != null)
                {
                    admin.IsActive = false;
                    await _context.SaveChangesAsync();
                    
                    await LogActivityAsync(0, "Deactivate Admin", "AdminUser", id, 
                        $"Admin {admin.Username} deactivated", null);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int adminId, string newPassword, string changedBy)
        {
            try
            {
                var admin = await _context.AdminUsers.FindAsync(adminId);
                if (admin != null)
                {
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    await _context.SaveChangesAsync();
                    
                    await LogActivityAsync(adminId, "Password Change", "AdminUser", adminId, 
                        "Password changed", null);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Activity Logging

        public async Task LogActivityAsync(int adminId, string action, string? entity = null, 
            int? entityId = null, string? description = null, string? ipAddress = null)
        {
            try
            {
                var activity = new AdminActivityLog
                {
                    AdminUserId = adminId,
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    Description = description,
                    ActivityDate = DateTime.Now,
                    IpAddress = ipAddress
                };

                _context.AdminActivityLogs.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Log errors if needed, but don't throw
            }
        }

        public async Task<List<AdminActivityLog>> GetRecentActivitiesAsync(int count = 50)
        {
            return await _context.AdminActivityLogs
                .Include(a => a.AdminUser)
                .OrderByDescending(a => a.ActivityDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<AdminActivityLog>> GetAdminActivitiesAsync(int adminId, int count = 50)
        {
            return await _context.AdminActivityLogs
                .Include(a => a.AdminUser)
                .Where(a => a.AdminUserId == adminId)
                .OrderByDescending(a => a.ActivityDate)
                .Take(count)
                .ToListAsync();
        }

        #endregion

        #region Dashboard Statistics

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);

            var stats = new DashboardStats
            {
                TotalDonations = await _context.MasjidDonations
                    .Where(d => d.PaymentStatus == "Completed")
                    .SumAsync(d => d.Amount),
                
                TotalDonators = await _context.MasjidDonations
                    .Where(d => d.PaymentStatus == "Completed")
                    .Select(d => d.Email)
                    .Distinct()
                    .CountAsync(),
                
                PendingMessages = await _context.ContactMessages
                    .Where(m => !m.IsRead)
                    .CountAsync(),
                
                ActiveProjects = await _context.MasjidProjects
                    .Where(p => p.IsActive)
                    .CountAsync(),
                
                MonthlyDonations = await _context.MasjidDonations
                    .Where(d => d.PaymentStatus == "Completed" && d.DonationDate >= startOfMonth)
                    .SumAsync(d => d.Amount),
                
                WeeklyDonations = await _context.MasjidDonations
                    .Where(d => d.PaymentStatus == "Completed" && d.DonationDate >= startOfWeek)
                    .CountAsync()
            };

            stats.AverageDonation = stats.TotalDonators > 0 ? stats.TotalDonations / stats.TotalDonators : 0;

            return stats;
        }

        public async Task<List<RecentDonation>> GetRecentDonationsAsync(int count = 10)
        {
            return await _context.MasjidDonations
                .Include(d => d.MasjidProject)
                .OrderByDescending(d => d.DonationDate)
                .Take(count)
                .Select(d => new RecentDonation
                {
                    Id = d.Id,
                    DonorName = d.IsAnonymous ? "Anonymous" : d.DonorName,
                    Amount = d.Amount,
                    ProjectName = d.MasjidProject!.Title,
                    DonationDate = d.DonationDate,
                    PaymentMethod = d.PaymentMethod,
                    PaymentStatus = d.PaymentStatus
                })
                .ToListAsync();
        }

        public async Task<List<ContactMessage>> GetRecentMessagesAsync(int count = 10)
        {
            return await _context.ContactMessages
                .OrderByDescending(m => m.DateSent)
                .Take(count)
                .ToListAsync();
        }

        #endregion

        #region Site Settings

        public async Task<List<SiteSettings>> GetAllSettingsAsync()
        {
            return await _context.SiteSettings
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.SettingKey)
                .ToListAsync();
        }

        public async Task<List<SiteSettingsGroup>> GetSettingsGroupedAsync()
        {
            var settings = await GetAllSettingsAsync();
            return settings.GroupBy(s => s.Category)
                .Select(g => new SiteSettingsGroup
                {
                    Category = g.Key,
                    Settings = g.ToList()
                })
                .ToList();
        }

        public async Task<SiteSettings?> GetSettingAsync(string key, string category = "General")
        {
            return await _context.SiteSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key && s.Category == category && s.IsActive);
        }

        public async Task<bool> UpdateSettingAsync(string key, string value, string category = "General", string modifiedBy = "")
        {
            try
            {
                var setting = await GetSettingAsync(key, category);
                if (setting != null)
                {
                    setting.SettingValue = value;
                    setting.LastModified = DateTime.Now;
                    setting.ModifiedBy = modifiedBy;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var newSetting = new SiteSettings
                    {
                        SettingKey = key,
                        SettingValue = value,
                        Category = category,
                        ModifiedBy = modifiedBy
                    };
                    _context.SiteSettings.Add(newSetting);
                    await _context.SaveChangesAsync();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateSettingAsync(SiteSettings setting)
        {
            try
            {
                _context.SiteSettings.Add(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSettingAsync(int id)
        {
            try
            {
                var setting = await _context.SiteSettings.FindAsync(id);
                if (setting != null)
                {
                    setting.IsActive = false;
                    await _context.SaveChangesAsync();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Content Management

        public async Task<List<MasjidProject>> GetAllProjectsAsync()
        {
            return await _context.MasjidProjects
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<MasjidProject?> GetProjectByIdAsync(int id)
        {
            return await _context.MasjidProjects
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> CreateProjectAsync(MasjidProject project, string createdBy)
        {
            try
            {
                project.CreatedDate = DateTime.Now;
                _context.MasjidProjects.Add(project);
                await _context.SaveChangesAsync();
                
                await LogActivityAsync(0, "Create Project", "MasjidProject", project.Id, 
                    $"Created project: {project.Title}", null);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProjectAsync(MasjidProject project, string modifiedBy)
        {
            try
            {
                _context.MasjidProjects.Update(project);
                await _context.SaveChangesAsync();
                
                await LogActivityAsync(0, "Update Project", "MasjidProject", project.Id, 
                    $"Updated project: {project.Title}", null);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProjectAsync(int id, string deletedBy)
        {
            try
            {
                var project = await _context.MasjidProjects.FindAsync(id);
                if (project != null)
                {
                    project.IsActive = false;
                    await _context.SaveChangesAsync();
                    
                    await LogActivityAsync(0, "Delete Project", "MasjidProject", id, 
                        $"Deleted project: {project.Title}", null);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnsetAllFeaturedProjectsAsync()
        {
            try
            {
                var featuredProjects = await _context.MasjidProjects
                    .Where(p => p.IsFeatured)
                    .ToListAsync();

                foreach (var project in featuredProjects)
                {
                    project.IsFeatured = false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Donation Management

        public async Task<List<MasjidDonation>> GetDonationsAsync(DonationFilterOptions? filters = null, int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.MasjidDonations.Include(d => d.MasjidProject).AsQueryable();

            if (filters != null)
            {
                if (filters.StartDate.HasValue)
                    query = query.Where(d => d.DonationDate >= filters.StartDate);

                if (filters.EndDate.HasValue)
                    query = query.Where(d => d.DonationDate <= filters.EndDate);

                if (filters.ProjectId.HasValue)
                    query = query.Where(d => d.MasjidProjectId == filters.ProjectId);

                if (!string.IsNullOrEmpty(filters.PaymentMethod))
                    query = query.Where(d => d.PaymentMethod == filters.PaymentMethod);

                if (!string.IsNullOrEmpty(filters.PaymentStatus))
                    query = query.Where(d => d.PaymentStatus == filters.PaymentStatus);

                if (filters.MinAmount.HasValue)
                    query = query.Where(d => d.Amount >= filters.MinAmount);

                if (filters.MaxAmount.HasValue)
                    query = query.Where(d => d.Amount <= filters.MaxAmount);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                    query = query.Where(d => d.DonorName.Contains(filters.SearchTerm) || 
                                           d.Email.Contains(filters.SearchTerm));
            }

            return await query
                .OrderByDescending(d => d.DonationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetDonationsCountAsync(DonationFilterOptions? filters = null)
        {
            var query = _context.MasjidDonations.AsQueryable();

            if (filters != null)
            {
                if (filters.StartDate.HasValue)
                    query = query.Where(d => d.DonationDate >= filters.StartDate);

                if (filters.EndDate.HasValue)
                    query = query.Where(d => d.DonationDate <= filters.EndDate);

                if (filters.ProjectId.HasValue)
                    query = query.Where(d => d.MasjidProjectId == filters.ProjectId);

                if (!string.IsNullOrEmpty(filters.PaymentMethod))
                    query = query.Where(d => d.PaymentMethod == filters.PaymentMethod);

                if (!string.IsNullOrEmpty(filters.PaymentStatus))
                    query = query.Where(d => d.PaymentStatus == filters.PaymentStatus);

                if (filters.MinAmount.HasValue)
                    query = query.Where(d => d.Amount >= filters.MinAmount);

                if (filters.MaxAmount.HasValue)
                    query = query.Where(d => d.Amount <= filters.MaxAmount);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                    query = query.Where(d => d.DonorName.Contains(filters.SearchTerm) || 
                                           d.Email.Contains(filters.SearchTerm));
            }

            return await query.CountAsync();
        }

        public async Task<decimal> GetDonationsTotalAsync(DonationFilterOptions? filters = null)
        {
            var query = _context.MasjidDonations.AsQueryable();

            if (filters != null)
            {
                if (filters.StartDate.HasValue)
                    query = query.Where(d => d.DonationDate >= filters.StartDate);

                if (filters.EndDate.HasValue)
                    query = query.Where(d => d.DonationDate <= filters.EndDate);

                if (filters.ProjectId.HasValue)
                    query = query.Where(d => d.MasjidProjectId == filters.ProjectId);

                if (!string.IsNullOrEmpty(filters.PaymentMethod))
                    query = query.Where(d => d.PaymentMethod == filters.PaymentMethod);

                if (!string.IsNullOrEmpty(filters.PaymentStatus))
                    query = query.Where(d => d.PaymentStatus == filters.PaymentStatus);

                if (filters.MinAmount.HasValue)
                    query = query.Where(d => d.Amount >= filters.MinAmount);

                if (filters.MaxAmount.HasValue)
                    query = query.Where(d => d.Amount <= filters.MaxAmount);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                    query = query.Where(d => d.DonorName.Contains(filters.SearchTerm) || 
                                           d.Email.Contains(filters.SearchTerm));
            }

            return await query.Where(d => d.PaymentStatus == "Completed").SumAsync(d => d.Amount);
        }

        public async Task<bool> UpdateDonationStatusAsync(int donationId, string status, string updatedBy)
        {
            try
            {
                var donation = await _context.MasjidDonations.FindAsync(donationId);
                if (donation != null)
                {
                    donation.PaymentStatus = status;
                    await _context.SaveChangesAsync();
                    
                    await LogActivityAsync(0, "Update Donation Status", "MasjidDonation", donationId, 
                        $"Status updated to: {status}", null);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RefundDonationAsync(int donationId, string reason, string processedBy)
        {
            try
            {
                var donation = await _context.MasjidDonations.FindAsync(donationId);
                if (donation != null)
                {
                    donation.PaymentStatus = "Refunded";
                    await _context.SaveChangesAsync();
                    
                    await LogActivityAsync(0, "Refund Donation", "MasjidDonation", donationId, 
                        $"Refunded: {reason}", null);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<MasjidDonation>> GetAllDonationsAsync(DonationFilterOptions? filters = null)
        {
            var query = _context.MasjidDonations.Include(d => d.MasjidProject).AsQueryable();
            
            if (filters != null)
            {
                if (filters.StartDate.HasValue)
                    query = query.Where(d => d.DonationDate >= filters.StartDate);
                if (filters.EndDate.HasValue)
                    query = query.Where(d => d.DonationDate <= filters.EndDate);
                if (filters.ProjectId.HasValue)
                    query = query.Where(d => d.MasjidProjectId == filters.ProjectId);
                if (!string.IsNullOrEmpty(filters.PaymentMethod))
                    query = query.Where(d => d.PaymentMethod == filters.PaymentMethod);
                if (!string.IsNullOrEmpty(filters.PaymentStatus))
                    query = query.Where(d => d.PaymentStatus == filters.PaymentStatus);
                if (filters.MinAmount.HasValue)
                    query = query.Where(d => d.Amount >= filters.MinAmount);
                if (filters.MaxAmount.HasValue)
                    query = query.Where(d => d.Amount <= filters.MaxAmount);
                if (!string.IsNullOrEmpty(filters.SearchTerm))
                    query = query.Where(d => d.DonorName.Contains(filters.SearchTerm) || 
                                           d.Email.Contains(filters.SearchTerm));
            }

            return await query.OrderByDescending(d => d.DonationDate).ToListAsync();
        }

        public async Task<MasjidDonation?> GetDonationByIdAsync(int id)
        {
            return await _context.MasjidDonations
                .Include(d => d.MasjidProject)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int donationId, string status)
        {
            try
            {
                var donation = await _context.MasjidDonations.FindAsync(donationId);
                if (donation != null)
                {
                    donation.PaymentStatus = status;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendThankYouEmailAsync(int donationId)
        {
            try
            {
                var donation = await _context.MasjidDonations
                    .Include(d => d.MasjidProject)
                    .FirstOrDefaultAsync(d => d.Id == donationId);

                if (donation == null || string.IsNullOrEmpty(donation.Email))
                    return false;

                // Here you would implement email sending logic
                // For now, we'll just return true as a placeholder
                // In a real implementation, you would use an email service like SendGrid, MailKit, etc.
                
                await LogActivityAsync(0, "Send Thank You Email", "MasjidDonation", donationId, 
                    $"Thank you email sent to {donation.Email}", null);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Message Management

        public async Task<List<ContactMessage>> GetMessagesAsync(MessageFilterOptions? filters = null, int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.ContactMessages.AsQueryable();

            if (filters != null)
            {
                if (filters.StartDate.HasValue)
                    query = query.Where(m => m.DateSent >= filters.StartDate);

                if (filters.EndDate.HasValue)
                    query = query.Where(m => m.DateSent <= filters.EndDate);

                if (filters.IsRead.HasValue)
                    query = query.Where(m => m.IsRead == filters.IsRead);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                    query = query.Where(m => m.Name.Contains(filters.SearchTerm) || 
                                           m.Email.Contains(filters.SearchTerm) || 
                                           m.Message.Contains(filters.SearchTerm));

                if (!string.IsNullOrEmpty(filters.Subject))
                    query = query.Where(m => m.Subject.Contains(filters.Subject));
            }

            return await query
                .OrderByDescending(m => m.DateSent)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetMessagesCountAsync(MessageFilterOptions? filters = null)
        {
            var query = _context.ContactMessages.AsQueryable();

            if (filters != null)
            {
                if (filters.StartDate.HasValue)
                    query = query.Where(m => m.DateSent >= filters.StartDate);

                if (filters.EndDate.HasValue)
                    query = query.Where(m => m.DateSent <= filters.EndDate);

                if (filters.IsRead.HasValue)
                    query = query.Where(m => m.IsRead == filters.IsRead);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                    query = query.Where(m => m.Name.Contains(filters.SearchTerm) || 
                                           m.Email.Contains(filters.SearchTerm) || 
                                           m.Message.Contains(filters.SearchTerm));

                if (!string.IsNullOrEmpty(filters.Subject))
                    query = query.Where(m => m.Subject.Contains(filters.Subject));
            }

            return await query.CountAsync();
        }

        public async Task<ContactMessage?> GetMessageByIdAsync(int id)
        {
            return await _context.ContactMessages.FindAsync(id);
        }

        public async Task<bool> MarkMessageAsReadAsync(int id, string readBy)
        {
            try
            {
                var message = await _context.ContactMessages.FindAsync(id);
                if (message != null)
                {
                    message.IsRead = true;
                    await _context.SaveChangesAsync();
                    
                    await LogActivityAsync(0, "Mark Message Read", "ContactMessage", id, 
                        $"Message from {message.Name} marked as read", null);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(int id, string deletedBy)
        {
            try
            {
                var message = await _context.ContactMessages.FindAsync(id);
                if (message != null)
                {
                    _context.ContactMessages.Remove(message);
                    await _context.SaveChangesAsync();
                    
                    await LogActivityAsync(0, "Delete Message", "ContactMessage", id, 
                        $"Message from {message.Name} deleted", null);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReplyToMessageAsync(int messageId, string subject, string reply, string repliedBy)
        {
            try
            {
                var message = await _context.ContactMessages.FindAsync(messageId);
                if (message == null) return false;

                // Here you would implement email sending logic
                // For now, we'll just log the activity
                await LogActivityAsync(0, "Reply To Message", "ContactMessage", messageId, 
                    $"Replied to message from {message.Name}: {subject}", null);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ContactMessage>> GetAllMessagesAsync(MessageFilterOptions? filters = null)
        {
            var query = _context.ContactMessages.AsQueryable();

            if (filters != null)
            {
                if (filters.StartDate.HasValue)
                    query = query.Where(m => m.DateSent >= filters.StartDate);

                if (filters.EndDate.HasValue)
                    query = query.Where(m => m.DateSent <= filters.EndDate);

                if (filters.IsRead.HasValue)
                    query = query.Where(m => m.IsRead == filters.IsRead);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                    query = query.Where(m => m.Name.Contains(filters.SearchTerm) || 
                                           m.Email.Contains(filters.SearchTerm) || 
                                           m.Message.Contains(filters.SearchTerm) ||
                                           m.Subject.Contains(filters.SearchTerm));

                if (!string.IsNullOrEmpty(filters.Subject))
                    query = query.Where(m => m.Subject.Contains(filters.Subject));
            }

            return await query.OrderByDescending(m => m.DateSent).ToListAsync();
        }

        public async Task<bool> MarkAllMessagesAsReadAsync(string readBy)
        {
            try
            {
                var unreadMessages = await _context.ContactMessages
                    .Where(m => !m.IsRead)
                    .ToListAsync();

                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.ReadDate = DateTime.Now;
                    message.ReadBy = readBy;
                }

                await _context.SaveChangesAsync();

                await LogActivityAsync(0, "Mark All Messages Read", "ContactMessage", null, 
                    $"Marked {unreadMessages.Count} messages as read", null);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendMassMessageAsync(string subject, string messageContent, string recipientType, bool testMode, string sentBy)
        {
            try
            {
                var recipients = new List<string>();

                if (testMode)
                {
                    // In test mode, send only to admin
                    var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Username == sentBy);
                    if (admin != null && !string.IsNullOrEmpty(admin.Email))
                    {
                        recipients.Add(admin.Email);
                    }
                }
                else
                {
                    // Get recipients based on type
                    switch (recipientType.ToLower())
                    {
                        case "donors":
                            recipients = await _context.MasjidDonations
                                .Where(d => !string.IsNullOrEmpty(d.Email) && !d.IsAnonymous)
                                .Select(d => d.Email)
                                .Distinct()
                                .ToListAsync();
                            break;
                        case "contacts":
                            recipients = await _context.ContactMessages
                                .Where(c => !string.IsNullOrEmpty(c.Email))
                                .Select(c => c.Email)
                                .Distinct()
                                .ToListAsync();
                            break;
                        default: // "all"
                            var donorEmails = await _context.MasjidDonations
                                .Where(d => !string.IsNullOrEmpty(d.Email) && !d.IsAnonymous)
                                .Select(d => d.Email)
                                .ToListAsync();
                            var contactEmails = await _context.ContactMessages
                                .Where(c => !string.IsNullOrEmpty(c.Email))
                                .Select(c => c.Email)
                                .ToListAsync();
                            recipients = donorEmails.Union(contactEmails).Distinct().ToList();
                            break;
                    }
                }

                // Here you would implement the actual email sending logic
                // For now, we'll just log the activity
                await LogActivityAsync(0, "Send Mass Message", "System", null, 
                    $"Mass message sent to {recipients.Count} recipients: {subject}", null);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetRecipientCountAsync(string recipientType)
        {
            try
            {
                return recipientType.ToLower() switch
                {
                    "donors" => await _context.MasjidDonations
                        .Where(d => !string.IsNullOrEmpty(d.Email) && !d.IsAnonymous)
                        .Select(d => d.Email)
                        .Distinct()
                        .CountAsync(),
                    "contacts" => await _context.ContactMessages
                        .Where(c => !string.IsNullOrEmpty(c.Email))
                        .Select(c => c.Email)
                        .Distinct()
                        .CountAsync(),
                    _ => await _context.MasjidDonations
                        .Where(d => !string.IsNullOrEmpty(d.Email) && !d.IsAnonymous)
                        .Select(d => d.Email)
                        .Union(_context.ContactMessages
                            .Where(c => !string.IsNullOrEmpty(c.Email))
                            .Select(c => c.Email))
                        .Distinct()
                        .CountAsync()
                };
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Reports

        public async Task<byte[]> ExportDonationsAsync(DonationFilterOptions? filters = null)
        {
            // Basic implementation - would typically use a library like EPPlus for Excel export
            var donations = await GetDonationsAsync(filters, 1, int.MaxValue);
            
            var csv = new StringBuilder();
            csv.AppendLine("Date,Donor,Email,Amount,Project,Payment Method,Status");
            
            foreach (var donation in donations)
            {
                csv.AppendLine($"{donation.DonationDate:yyyy-MM-dd},{donation.DonorName},{donation.Email},{donation.Amount},{donation.MasjidProject?.Title},{donation.PaymentMethod},{donation.PaymentStatus}");
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportMessagesAsync(MessageFilterOptions? filters = null)
        {
            var messages = await GetMessagesAsync(filters, 1, int.MaxValue);
            
            var csv = new StringBuilder();
            csv.AppendLine("Date,Name,Email,Subject,Message,Read");
            
            foreach (var message in messages)
            {
                csv.AppendLine($"{message.DateSent:yyyy-MM-dd},{message.Name},{message.Email},{message.Subject},{message.Message.Replace(',', ';')},{message.IsRead}");
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        #endregion

        #region Helper Methods

        private string GenerateSessionToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        #endregion
    }
}