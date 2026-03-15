using GambianMuslimCommunity.Models;

namespace GambianMuslimCommunity.Services
{
    public interface IContributionTrackerService
    {
        // Contribution tracking methods
        Task<bool> UpdateContributionTrackerAsync(string contributorName, string email, decimal amount, string paymentMethod);
        Task<bool> AddManualContributionAsync(string contributorName, string email, decimal amount, string projectName, string paymentMethod, DateTime? contributionDate = null, string? notes = null);
        Task<ContributionSummary?> GetContributorSummaryAsync(string email);
        Task<List<ContributionSummary>> GetAllContributorsAsync(ContributionFilterOptions? filters = null, int pageNumber = 1, int pageSize = 20);
        Task<int> GetContributorsCountAsync(ContributionFilterOptions? filters = null);
        
        // Analytics methods
        Task<ContributionAnalytics> GetWeeklyAnalyticsAsync(DateTime? weekStart = null);
        Task<ContributionAnalytics> GetMonthlyAnalyticsAsync(DateTime? monthStart = null);
        Task<ContributionAnalytics> GetYearlyAnalyticsAsync(int? year = null);
        Task<ContributionAnalytics> GetCustomAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<ContributionAnalytics> GetQuarterlyAnalyticsAsync(int year, int quarter);
        
        // Individual contributor reports
        Task<IndividualContributorReport> GetContributorReportAsync(string email);
        Task<List<ContributionAnalytics>> GetContributorMonthlyTrendsAsync(string email, int months = 12);
        Task<List<ProjectContribution>> GetContributorProjectBreakdownAsync(string email);
        
        // Recognition and ranking
        Task<List<TopContributor>> GetTopContributorsAsync(string period = "month", int count = 10, bool includeAnonymous = false);
        Task<ContributionRecognition> GetContributorRecognitionLevelAsync(string email);
        Task<string> GetContributorRankAsync(string email, string period = "year");
        
        // Maintenance and updates
        Task<bool> RecalculateContributionTrackersAsync();
        Task<bool> CleanupInactiveContributorsAsync(int monthsInactive = 24);
        
        // Export functionality
        Task<byte[]> ExportContributionReportAsync(ContributionFilterOptions? filters = null);
        Task<byte[]> ExportIndividualReportAsync(string email);
    }
}