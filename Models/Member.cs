using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GambianMuslimCommunity.Models
{
    public class Member
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [StringLength(255)]
        [Display(Name = "Street Address")]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string State { get; set; } = string.Empty;
        
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Profession { get; set; } = string.Empty;
        
        [StringLength(100)]
        [Display(Name = "Nationality")]
        public string Nationality { get; set; } = string.Empty;
        
        [StringLength(50)]
        [Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; } = string.Empty;
        
        [StringLength(100)]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; } = string.Empty;
        
        [StringLength(20)]
        [Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; } = string.Empty;
        
        [StringLength(100)]
        [Display(Name = "Emergency Contact Relationship")]
        public string EmergencyContactRelationship { get; set; } = string.Empty;
        
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(20)]
        [Display(Name = "Membership Status")]
        public string MembershipStatus { get; set; } = "Pending"; // Pending, Active, Inactive, Suspended, Rejected
        
        [Display(Name = "Approved Date")]
        public DateTime? ApprovedDate { get; set; }
        
        [Display(Name = "Approved By")]
        public int? ApprovedById { get; set; }
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Full Name")]
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        
        [StringLength(1000)]
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; } = string.Empty;
        
        [Display(Name = "Receive Email Notifications")]
        public bool ReceiveEmailNotifications { get; set; } = true;
        
        [Display(Name = "Receive SMS Notifications")]
        public bool ReceiveSmsNotifications { get; set; } = true;
        
        [StringLength(100)]
        [Display(Name = "Preferred Language")]
        public string PreferredLanguage { get; set; } = "English";
        
        [Display(Name = "Member Since")]
        [NotMapped]
        public string MemberSince => ApprovedDate?.ToString("MMMM yyyy") ?? "Not yet approved";
        
        // Navigation properties
        [ForeignKey("ApprovedById")]
        public virtual AdminUser? ApprovedBy { get; set; }
        
        public virtual ICollection<MemberActivityLog> ActivityLogs { get; set; } = new List<MemberActivityLog>();
    }

    public class MemberActivityLog
    {
        public int Id { get; set; }
        
        [Required]
        public int MemberId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Entity { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(200)]
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }
        
        public int? AdminUserId { get; set; }
        
        // Navigation properties
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; } = null!;
        
        [ForeignKey("AdminUserId")]
        public virtual AdminUser? AdminUser { get; set; }
    }

    public class MembershipSettings
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
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Last Modified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Modified By")]
        public int? ModifiedById { get; set; }
        
        // Navigation property
        [ForeignKey("ModifiedById")]
        public virtual AdminUser? ModifiedBy { get; set; }
    }
}