using System.ComponentModel.DataAnnotations;

namespace GambianMuslimCommunity.Models
{
    public class MasjidProject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "New Masjid Building Project";

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Target Amount")]
        public decimal TargetAmount { get; set; }

        [Display(Name = "Current Amount")]
        public decimal CurrentAmount { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Target Completion Date")]
        public DateTime? TargetCompletionDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Planning"; // Planning, Fundraising, Construction, Completed

        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Project Updates")]
        public string Updates { get; set; } = string.Empty;

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; } = true;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        // Calculated property
        public decimal ProgressPercentage => TargetAmount > 0 ? Math.Round((CurrentAmount / TargetAmount) * 100, 1) : 0;

        public int DaysRemaining => TargetCompletionDate.HasValue 
            ? Math.Max(0, (int)(TargetCompletionDate.Value - DateTime.Today).TotalDays)
            : 0;
    }

    public class MasjidDonation
    {
        public int Id { get; set; }

        [Required]
        public int MasjidProjectId { get; set; }
        public MasjidProject MasjidProject { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [Display(Name = "Donor Name")]
        public string DonorName { get; set; } = string.Empty;

        [StringLength(200)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Donation Amount")]
        [Range(1, 1000000, ErrorMessage = "Donation amount must be greater than $0")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        [Display(Name = "Message/Note")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Anonymous Donation")]
        public bool IsAnonymous { get; set; }

        [Display(Name = "Donation Date")]
        public DateTime DonationDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "PayPal Payment ID")]
        public string PayPalPaymentId { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "PayPal Payer ID")]
        public string PayPalPayerId { get; set; } = string.Empty;

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Cancelled
    }

    public class PaymentRequest
    {
        public int MasjidProjectId { get; set; }
        public decimal Amount { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string ApprovalUrl { get; set; } = string.Empty;
        public MasjidDonation? Donation { get; set; }
    }
}