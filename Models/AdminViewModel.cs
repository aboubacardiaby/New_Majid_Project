using System.ComponentModel.DataAnnotations;

namespace GambianMuslimCommunity.Models
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public DashboardStats Stats { get; set; } = new DashboardStats();
        public List<RecentDonation> RecentDonations { get; set; } = new List<RecentDonation>();
        public List<ContactMessage> RecentMessages { get; set; } = new List<ContactMessage>();
        public List<AdminActivityLog> RecentActivities { get; set; } = new List<AdminActivityLog>();
        public List<MasjidProject> ActiveProjects { get; set; } = new List<MasjidProject>();
    }

    public class DashboardStats
    {
        public decimal TotalDonations { get; set; }
        public int TotalDonators { get; set; }
        public int PendingMessages { get; set; }
        public int ActiveProjects { get; set; }
        public decimal MonthlyDonations { get; set; }
        public int WeeklyDonations { get; set; }
        public int TotalMembers { get; set; }
        public decimal AverageDonation { get; set; }
    }

    public class RecentDonation
    {
        public int Id { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime DonationDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class AdminSettingsViewModel
    {
        public List<SiteSettingsGroup> SettingsGroups { get; set; } = new List<SiteSettingsGroup>();
        public SiteInfo SiteInfo { get; set; } = new SiteInfo();
        public EmailSettings EmailSettings { get; set; } = new EmailSettings();
        public PayPalSettings PayPalSettings { get; set; } = new PayPalSettings();
        public SocialMediaSettings SocialMedia { get; set; } = new SocialMediaSettings();
    }

    public class SiteSettingsGroup
    {
        public string Category { get; set; } = string.Empty;
        public List<SiteSettings> Settings { get; set; } = new List<SiteSettings>();
    }

    public class SiteInfo
    {
        [Required]
        [Display(Name = "Site Name")]
        public string SiteName { get; set; } = string.Empty;

        [Display(Name = "Site Description")]
        public string SiteDescription { get; set; } = string.Empty;

        [Display(Name = "Contact Email")]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "State")]
        public string State { get; set; } = string.Empty;

        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Display(Name = "Logo URL")]
        public string LogoUrl { get; set; } = string.Empty;

        [Display(Name = "Imam Name")]
        public string ImamName { get; set; } = string.Empty;

        [Display(Name = "Imam Welcome Message")]
        public string ImamWelcomeMessage { get; set; } = string.Empty;

        [Display(Name = "Imam Image URL")]
        public string ImamImageUrl { get; set; } = string.Empty;

        [Display(Name = "Imam Title")]
        public string ImamTitle { get; set; } = string.Empty;
    }

    public class EmailSettings
    {
        [Display(Name = "SMTP Server")]
        public string SmtpServer { get; set; } = string.Empty;

        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "From Email")]
        [EmailAddress]
        public string FromEmail { get; set; } = string.Empty;

        [Display(Name = "From Name")]
        public string FromName { get; set; } = string.Empty;

        [Display(Name = "Use SSL")]
        public bool UseSSL { get; set; } = true;

        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class SocialMediaSettings
    {
        [Display(Name = "Facebook URL")]
        public string FacebookUrl { get; set; } = string.Empty;

        [Display(Name = "Instagram URL")]
        public string InstagramUrl { get; set; } = string.Empty;

        [Display(Name = "YouTube URL")]
        public string YouTubeUrl { get; set; } = string.Empty;

        [Display(Name = "WhatsApp Number")]
        public string WhatsAppNumber { get; set; } = string.Empty;

        [Display(Name = "Twitter Handle")]
        public string TwitterHandle { get; set; } = string.Empty;
    }

    public class AdminProjectManagementViewModel
    {
        public List<MasjidProject> Projects { get; set; } = new List<MasjidProject>();
        public MasjidProject? EditingProject { get; set; }
        public List<MasjidDonation> ProjectDonations { get; set; } = new List<MasjidDonation>();
        public decimal TotalRaised { get; set; }
        public int TotalDonators { get; set; }
    }

    public class AdminDonationManagementViewModel
    {
        public List<MasjidDonation> Donations { get; set; } = new List<MasjidDonation>();
        public DonationFilterOptions FilterOptions { get; set; } = new DonationFilterOptions();
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
    }

    public class DonationFilterOptions
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ProjectId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class AdminMessageManagementViewModel
    {
        public List<ContactMessage> Messages { get; set; } = new List<ContactMessage>();
        public ContactMessage? SelectedMessage { get; set; }
        public MessageFilterOptions FilterOptions { get; set; } = new MessageFilterOptions();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
    }

    public class MessageFilterOptions
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsRead { get; set; }
        public string? SearchTerm { get; set; }
        public string? Subject { get; set; }
    }

    public class PaymentStatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class ReplyMessageRequest
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool SendCopy { get; set; }
    }

    public class BulkMessageRequest
    {
        public List<int> MessageIds { get; set; } = new List<int>();
    }

    public class MassMessageRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RecipientType { get; set; } = "all"; // all, donors, contacts
        public bool TestMode { get; set; } = false;
    }
}