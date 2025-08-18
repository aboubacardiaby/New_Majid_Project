using GambianMuslimCommunity.Models;

namespace GambianMuslimCommunity.Services
{
    public interface IAdminService
    {
        // Authentication methods
        Task<AdminUser?> ValidateAdminAsync(string username, string password);
        Task<AdminUser?> GetAdminByIdAsync(int id);
        Task<AdminUser?> GetAdminByUsernameAsync(string username);
        Task<string> CreateSessionAsync(AdminUser admin, string ipAddress, string userAgent);
        Task<AdminUser?> ValidateSessionAsync(string sessionToken);
        Task<bool> LogoutAsync(string sessionToken);
        Task<bool> UpdateLastActivityAsync(string sessionToken);
        
        // Admin management
        Task<List<AdminUser>> GetActiveAdminsAsync();
        Task<bool> CreateAdminAsync(AdminUser admin, string password);
        Task<bool> UpdateAdminAsync(AdminUser admin);
        Task<bool> DeactivateAdminAsync(int id, string deactivatedBy);
        Task<bool> ChangePasswordAsync(int adminId, string newPassword, string changedBy);
        
        // Activity logging
        Task LogActivityAsync(int adminId, string action, string? entity = null, int? entityId = null, 
            string? description = null, string? ipAddress = null);
        Task<List<AdminActivityLog>> GetRecentActivitiesAsync(int count = 50);
        Task<List<AdminActivityLog>> GetAdminActivitiesAsync(int adminId, int count = 50);
        
        // Dashboard statistics
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<List<RecentDonation>> GetRecentDonationsAsync(int count = 10);
        Task<List<ContactMessage>> GetRecentMessagesAsync(int count = 10);
        
        // Site settings management
        Task<List<SiteSettings>> GetAllSettingsAsync();
        Task<List<SiteSettingsGroup>> GetSettingsGroupedAsync();
        Task<SiteSettings?> GetSettingAsync(string key, string category = "General");
        Task<bool> UpdateSettingAsync(string key, string value, string category = "General", string modifiedBy = "");
        Task<bool> CreateSettingAsync(SiteSettings setting);
        Task<bool> DeleteSettingAsync(int id);
        
        // Content management
        Task<List<MasjidProject>> GetAllProjectsAsync();
        Task<MasjidProject?> GetProjectByIdAsync(int id);
        Task<bool> CreateProjectAsync(MasjidProject project, string createdBy);
        Task<bool> UpdateProjectAsync(MasjidProject project, string modifiedBy);
        Task<bool> DeleteProjectAsync(int id, string deletedBy);
        Task<bool> UnsetAllFeaturedProjectsAsync();
        
        // Donation management
        Task<List<MasjidDonation>> GetDonationsAsync(DonationFilterOptions? filters = null, int pageNumber = 1, int pageSize = 20);
        Task<List<MasjidDonation>> GetAllDonationsAsync(DonationFilterOptions? filters = null);
        Task<MasjidDonation?> GetDonationByIdAsync(int id);
        Task<int> GetDonationsCountAsync(DonationFilterOptions? filters = null);
        Task<decimal> GetDonationsTotalAsync(DonationFilterOptions? filters = null);
        Task<bool> UpdateDonationStatusAsync(int donationId, string status, string updatedBy);
        Task<bool> UpdatePaymentStatusAsync(int donationId, string status);
        Task<bool> SendThankYouEmailAsync(int donationId);
        Task<bool> RefundDonationAsync(int donationId, string reason, string processedBy);
        
        // Message management
        Task<List<ContactMessage>> GetMessagesAsync(MessageFilterOptions? filters = null, int pageNumber = 1, int pageSize = 20);
        Task<List<ContactMessage>> GetAllMessagesAsync(MessageFilterOptions? filters = null);
        Task<int> GetMessagesCountAsync(MessageFilterOptions? filters = null);
        Task<ContactMessage?> GetMessageByIdAsync(int id);
        Task<bool> MarkMessageAsReadAsync(int id, string readBy);
        Task<bool> MarkAllMessagesAsReadAsync(string readBy);
        Task<bool> DeleteMessageAsync(int id, string deletedBy);
        Task<bool> ReplyToMessageAsync(int messageId, string subject, string reply, string repliedBy);
        Task<bool> SendMassMessageAsync(string subject, string message, string recipientType, bool testMode, string sentBy);
        Task<int> GetRecipientCountAsync(string recipientType);
        
        // Reports
        Task<byte[]> ExportDonationsAsync(DonationFilterOptions? filters = null);
        Task<byte[]> ExportMessagesAsync(MessageFilterOptions? filters = null);
    }
}