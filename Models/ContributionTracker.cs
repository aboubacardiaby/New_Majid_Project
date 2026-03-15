using System.ComponentModel.DataAnnotations;

namespace GambianMuslimCommunity.Models
{
    public class ContributionTracker
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Contributor Name")]
        public string ContributorName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Total Contributions")]
        public decimal TotalContributions { get; set; }

        [Display(Name = "Contribution Count")]
        public int ContributionCount { get; set; }

        [Display(Name = "First Contribution Date")]
        public DateTime FirstContributionDate { get; set; }

        [Display(Name = "Last Contribution Date")]
        public DateTime LastContributionDate { get; set; }

        [Display(Name = "Average Contribution")]
        public decimal AverageContribution { get; set; }

        [Display(Name = "Preferred Payment Method")]
        public string PreferredPaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Is Active Contributor")]
        public bool IsActiveContributor { get; set; } = true;

        [Display(Name = "Contribution Notes")]
        public string ContributionNotes { get; set; } = string.Empty;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class ContributionSummary
    {
        public string ContributorName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ContributionCount { get; set; }
        public decimal AverageAmount { get; set; }
        public DateTime FirstContribution { get; set; }
        public DateTime LastContribution { get; set; }
        public List<ContributionDetail> Contributions { get; set; } = new();
    }

    public class ContributionDetail
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ContributionAnalytics
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ContributionCount { get; set; }
        public int UniqueContributors { get; set; }
        public decimal AverageContribution { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<TopContributor> TopContributors { get; set; } = new();
        public List<DailyContribution> DailyBreakdown { get; set; } = new();
    }

    public class TopContributor
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int ContributionCount { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class DailyContribution
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class ContributionTrackerViewModel
    {
        public List<ContributionSummary> Contributors { get; set; } = new();
        public ContributionAnalytics WeeklyAnalytics { get; set; } = new();
        public ContributionAnalytics MonthlyAnalytics { get; set; } = new();
        public ContributionAnalytics YearlyAnalytics { get; set; } = new();
        public ContributionAnalytics CustomAnalytics { get; set; } = new();
        public ContributionFilterOptions FilterOptions { get; set; } = new();
        public int TotalContributors { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
    }

    public class ContributionFilterOptions
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TimePeriod { get; set; } = "month"; // week, month, quarter, year, custom
        public string? SortBy { get; set; } = "amount"; // amount, count, date, name
        public string? SortOrder { get; set; } = "desc"; // asc, desc
        public bool IncludeAnonymous { get; set; } = true;
    }

    public class IndividualContributorReport
    {
        public ContributionSummary Summary { get; set; } = new();
        public List<ContributionAnalytics> MonthlyTrends { get; set; } = new();
        public List<ProjectContribution> ProjectBreakdown { get; set; } = new();
        public decimal YearToDateTotal { get; set; }
        public decimal MonthToDateTotal { get; set; }
        public decimal WeekToDateTotal { get; set; }
        public int ConsecutiveMonths { get; set; }
        public string ContributorRank { get; set; } = string.Empty;
        public decimal AnnualTarget { get; set; }
        public decimal TargetProgress { get; set; }
    }

    public class ProjectContribution
    {
        public string ProjectName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ContributionCount { get; set; }
        public DateTime FirstContribution { get; set; }
        public DateTime LastContribution { get; set; }
    }

    public class ContributionRecognition
    {
        public string Level { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
        public string Title { get; set; } = string.Empty;
        public decimal MinimumAmount { get; set; }
        public string BadgeIcon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ManualContribution
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Contributor Name")]
        public string ContributorName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be greater than $0")]
        [Display(Name = "Contribution Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Contribution Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ContributionDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Helper property for dropdown lists
        public List<string>? AvailableProjects { get; set; }
        public List<string>? PaymentMethods { get; set; }
    }
}