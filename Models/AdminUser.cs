using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GambianMuslimCommunity.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Password Hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Admin";

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Last Login")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Profile Picture")]
        public string? ProfilePicture { get; set; }

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        // Navigation properties
        public virtual ICollection<AdminSession> Sessions { get; set; } = new List<AdminSession>();
        public virtual ICollection<AdminActivityLog> ActivityLogs { get; set; } = new List<AdminActivityLog>();
    }

    public class AdminSession
    {
        public int Id { get; set; }

        [Required]
        public int AdminUserId { get; set; }

        [Required]
        [StringLength(500)]
        public string SessionToken { get; set; } = string.Empty;

        [Display(Name = "Login Date")]
        public DateTime LoginDate { get; set; } = DateTime.Now;

        [Display(Name = "Last Activity")]
        public DateTime LastActivity { get; set; } = DateTime.Now;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        [Display(Name = "User Agent")]
        public string? UserAgent { get; set; }

        // Navigation property
        [ForeignKey("AdminUserId")]
        public virtual AdminUser AdminUser { get; set; } = null!;
    }

    public class AdminActivityLog
    {
        public int Id { get; set; }

        [Required]
        public int AdminUserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Entity { get; set; }

        public int? EntityId { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; } = DateTime.Now;

        [StringLength(200)]
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        // Navigation property
        [ForeignKey("AdminUserId")]
        public virtual AdminUser AdminUser { get; set; } = null!;
    }

    public class SiteSettings
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string SettingValue { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = "General";

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Last Modified")]
        public DateTime LastModified { get; set; } = DateTime.Now;

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; } = string.Empty;
    }
}