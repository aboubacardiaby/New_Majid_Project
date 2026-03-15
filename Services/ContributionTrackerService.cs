using System.Text;
using GambianMuslimCommunity.Data;
using GambianMuslimCommunity.Models;
using Microsoft.EntityFrameworkCore;

namespace GambianMuslimCommunity.Services
{
    public class ContributionTrackerService : IContributionTrackerService
    {
        private readonly ApplicationDbContext _context;

        public ContributionTrackerService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Contribution Tracking

        public async Task<bool> AddManualContributionAsync(string contributorName, string email, decimal amount, string projectName, string paymentMethod, DateTime? contributionDate = null, string? notes = null)
        {
            try
            {
                // Create a manual donation record in the MasjidDonations table
                var donation = new MasjidDonation
                {
                    DonorName = contributorName,
                    Email = email,
                    Amount = amount,
                    Message = notes ?? "",
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "Completed",
                    DonationDate = contributionDate ?? DateTime.Now,
                    IsAnonymous = false,
                    TransactionId = $"MANUAL_{DateTime.Now:yyyyMMddHHmmss}",
                    // Try to find matching project or use default
                    MasjidProjectId = await GetProjectIdByNameAsync(projectName) ?? 1
                };

                _context.MasjidDonations.Add(donation);
                await _context.SaveChangesAsync();

                // Update the contribution tracker
                return await UpdateContributionTrackerAsync(contributorName, email, amount, paymentMethod);
            }
            catch (Exception ex)
            {
                // Log the exception if logging is available
                return false;
            }
        }

        private async Task<int?> GetProjectIdByNameAsync(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
                return null;

            var project = await _context.MasjidProjects
                .FirstOrDefaultAsync(p => p.Title.ToLower().Contains(projectName.ToLower()));

            return project?.Id;
        }

        public async Task<bool> UpdateContributionTrackerAsync(string contributorName, string email, decimal amount, string paymentMethod)
        {
            try
            {
                var tracker = await _context.ContributionTrackers
                    .FirstOrDefaultAsync(ct => ct.Email.ToLower() == email.ToLower());

                if (tracker == null)
                {
                    // Create new tracker
                    tracker = new ContributionTracker
                    {
                        ContributorName = contributorName,
                        Email = email,
                        TotalContributions = amount,
                        ContributionCount = 1,
                        FirstContributionDate = DateTime.Now,
                        LastContributionDate = DateTime.Now,
                        AverageContribution = amount,
                        PreferredPaymentMethod = paymentMethod,
                        IsActiveContributor = true,
                        CreatedDate = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };
                    _context.ContributionTrackers.Add(tracker);
                }
                else
                {
                    // Update existing tracker
                    tracker.ContributorName = contributorName; // Update in case name changed
                    tracker.TotalContributions += amount;
                    tracker.ContributionCount += 1;
                    tracker.LastContributionDate = DateTime.Now;
                    tracker.AverageContribution = tracker.TotalContributions / tracker.ContributionCount;
                    tracker.PreferredPaymentMethod = paymentMethod;
                    tracker.IsActiveContributor = true;
                    tracker.LastUpdated = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ContributionSummary?> GetContributorSummaryAsync(string email)
        {
            var tracker = await _context.ContributionTrackers
                .FirstOrDefaultAsync(ct => ct.Email.ToLower() == email.ToLower());

            if (tracker == null) return null;

            var donations = await _context.MasjidDonations
                .Include(d => d.MasjidProject)
                .Where(d => d.Email.ToLower() == email.ToLower() && d.PaymentStatus == "Completed")
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return new ContributionSummary
            {
                ContributorName = tracker.ContributorName,
                Email = tracker.Email,
                TotalAmount = tracker.TotalContributions,
                ContributionCount = tracker.ContributionCount,
                AverageAmount = tracker.AverageContribution,
                FirstContribution = tracker.FirstContributionDate,
                LastContribution = tracker.LastContributionDate,
                Contributions = donations.Select(d => new ContributionDetail
                {
                    Date = d.DonationDate,
                    Amount = d.Amount,
                    ProjectName = d.MasjidProject?.Title ?? "Unknown Project",
                    PaymentMethod = d.PaymentMethod,
                    PaymentStatus = d.PaymentStatus,
                    Message = d.Message
                }).ToList()
            };
        }

        public async Task<List<ContributionSummary>> GetAllContributorsAsync(ContributionFilterOptions? filters = null, int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.ContributionTrackers.AsQueryable();

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    query = query.Where(ct => ct.ContributorName.Contains(filters.SearchTerm) || 
                                            ct.Email.Contains(filters.SearchTerm));
                }

                if (filters.MinAmount.HasValue)
                    query = query.Where(ct => ct.TotalContributions >= filters.MinAmount);

                if (filters.MaxAmount.HasValue)
                    query = query.Where(ct => ct.TotalContributions <= filters.MaxAmount);

                if (filters.StartDate.HasValue)
                    query = query.Where(ct => ct.LastContributionDate >= filters.StartDate);

                if (filters.EndDate.HasValue)
                    query = query.Where(ct => ct.LastContributionDate <= filters.EndDate);

                // Apply sorting
                query = filters.SortBy?.ToLower() switch
                {
                    "name" => filters.SortOrder == "asc" ? query.OrderBy(ct => ct.ContributorName) : query.OrderByDescending(ct => ct.ContributorName),
                    "count" => filters.SortOrder == "asc" ? query.OrderBy(ct => ct.ContributionCount) : query.OrderByDescending(ct => ct.ContributionCount),
                    "date" => filters.SortOrder == "asc" ? query.OrderBy(ct => ct.LastContributionDate) : query.OrderByDescending(ct => ct.LastContributionDate),
                    _ => filters.SortOrder == "asc" ? query.OrderBy(ct => ct.TotalContributions) : query.OrderByDescending(ct => ct.TotalContributions)
                };
            }
            else
            {
                query = query.OrderByDescending(ct => ct.TotalContributions);
            }

            var trackers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return trackers.Select(ct => new ContributionSummary
            {
                ContributorName = ct.ContributorName,
                Email = ct.Email,
                TotalAmount = ct.TotalContributions,
                ContributionCount = ct.ContributionCount,
                AverageAmount = ct.AverageContribution,
                FirstContribution = ct.FirstContributionDate,
                LastContribution = ct.LastContributionDate
            }).ToList();
        }

        public async Task<int> GetContributorsCountAsync(ContributionFilterOptions? filters = null)
        {
            var query = _context.ContributionTrackers.AsQueryable();

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    query = query.Where(ct => ct.ContributorName.Contains(filters.SearchTerm) || 
                                            ct.Email.Contains(filters.SearchTerm));
                }

                if (filters.MinAmount.HasValue)
                    query = query.Where(ct => ct.TotalContributions >= filters.MinAmount);

                if (filters.MaxAmount.HasValue)
                    query = query.Where(ct => ct.TotalContributions <= filters.MaxAmount);

                if (filters.StartDate.HasValue)
                    query = query.Where(ct => ct.LastContributionDate >= filters.StartDate);

                if (filters.EndDate.HasValue)
                    query = query.Where(ct => ct.LastContributionDate <= filters.EndDate);
            }

            return await query.CountAsync();
        }

        #endregion

        #region Analytics

        public async Task<ContributionAnalytics> GetWeeklyAnalyticsAsync(DateTime? weekStart = null)
        {
            var startDate = weekStart ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endDate = startDate.AddDays(7);

            return await GenerateAnalyticsAsync("Weekly", startDate, endDate);
        }

        public async Task<ContributionAnalytics> GetMonthlyAnalyticsAsync(DateTime? monthStart = null)
        {
            var startDate = monthStart ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endDate = startDate.AddMonths(1);

            return await GenerateAnalyticsAsync("Monthly", startDate, endDate);
        }

        public async Task<ContributionAnalytics> GetYearlyAnalyticsAsync(int? year = null)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var startDate = new DateTime(targetYear, 1, 1);
            var endDate = startDate.AddYears(1);

            return await GenerateAnalyticsAsync($"Yearly ({targetYear})", startDate, endDate);
        }

        public async Task<ContributionAnalytics> GetCustomAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            return await GenerateAnalyticsAsync("Custom", startDate, endDate);
        }

        public async Task<ContributionAnalytics> GetQuarterlyAnalyticsAsync(int year, int quarter)
        {
            var startDate = new DateTime(year, (quarter - 1) * 3 + 1, 1);
            var endDate = startDate.AddMonths(3);

            return await GenerateAnalyticsAsync($"Q{quarter} {year}", startDate, endDate);
        }

        private async Task<ContributionAnalytics> GenerateAnalyticsAsync(string period, DateTime startDate, DateTime endDate)
        {
            var donations = await _context.MasjidDonations
                .Where(d => d.DonationDate >= startDate && d.DonationDate < endDate && d.PaymentStatus == "Completed")
                .ToListAsync();

            var topContributors = donations
                .Where(d => !d.IsAnonymous)
                .GroupBy(d => new { d.Email, d.DonorName })
                .Select(g => new TopContributor
                {
                    Email = g.Key.Email,
                    Name = g.Key.DonorName,
                    Amount = g.Sum(d => d.Amount),
                    ContributionCount = g.Count()
                })
                .OrderByDescending(tc => tc.Amount)
                .Take(5)
                .ToList();

            var dailyBreakdown = donations
                .GroupBy(d => d.DonationDate.Date)
                .Select(g => new DailyContribution
                {
                    Date = g.Key,
                    Amount = g.Sum(d => d.Amount),
                    Count = g.Count()
                })
                .OrderBy(db => db.Date)
                .ToList();

            return new ContributionAnalytics
            {
                Period = period,
                TotalAmount = donations.Sum(d => d.Amount),
                ContributionCount = donations.Count,
                UniqueContributors = donations.Select(d => d.Email).Distinct().Count(),
                AverageContribution = donations.Any() ? donations.Average(d => d.Amount) : 0,
                StartDate = startDate,
                EndDate = endDate,
                TopContributors = topContributors,
                DailyBreakdown = dailyBreakdown
            };
        }

        #endregion

        #region Individual Reports

        public async Task<IndividualContributorReport> GetContributorReportAsync(string email)
        {
            var summary = await GetContributorSummaryAsync(email);
            if (summary == null)
            {
                return new IndividualContributorReport();
            }

            var monthlyTrends = await GetContributorMonthlyTrendsAsync(email, 12);
            var projectBreakdown = await GetContributorProjectBreakdownAsync(email);

            var currentYear = DateTime.Today.Year;
            var yearStart = new DateTime(currentYear, 1, 1);
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

            var yearToDate = await GetContributorPeriodTotalAsync(email, yearStart, DateTime.Now);
            var monthToDate = await GetContributorPeriodTotalAsync(email, monthStart, DateTime.Now);
            var weekToDate = await GetContributorPeriodTotalAsync(email, weekStart, DateTime.Now);

            var rank = await GetContributorRankAsync(email, "year");
            var recognition = await GetContributorRecognitionLevelAsync(email);

            return new IndividualContributorReport
            {
                Summary = summary,
                MonthlyTrends = monthlyTrends,
                ProjectBreakdown = projectBreakdown,
                YearToDateTotal = yearToDate,
                MonthToDateTotal = monthToDate,
                WeekToDateTotal = weekToDate,
                ContributorRank = rank
            };
        }

        public async Task<List<ContributionAnalytics>> GetContributorMonthlyTrendsAsync(string email, int months = 12)
        {
            var trends = new List<ContributionAnalytics>();
            var startDate = DateTime.Today.AddMonths(-months + 1);

            for (int i = 0; i < months; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var monthlyDonations = await _context.MasjidDonations
                    .Where(d => d.Email.ToLower() == email.ToLower() && 
                               d.DonationDate >= monthStart && 
                               d.DonationDate < monthEnd && 
                               d.PaymentStatus == "Completed")
                    .ToListAsync();

                trends.Add(new ContributionAnalytics
                {
                    Period = monthStart.ToString("MMM yyyy"),
                    TotalAmount = monthlyDonations.Sum(d => d.Amount),
                    ContributionCount = monthlyDonations.Count,
                    StartDate = monthStart,
                    EndDate = monthEnd
                });
            }

            return trends;
        }

        public async Task<List<ProjectContribution>> GetContributorProjectBreakdownAsync(string email)
        {
            var donations = await _context.MasjidDonations
                .Include(d => d.MasjidProject)
                .Where(d => d.Email.ToLower() == email.ToLower() && d.PaymentStatus == "Completed")
                .ToListAsync();

            return donations
                .GroupBy(d => d.MasjidProject?.Title ?? "Unknown Project")
                .Select(g => new ProjectContribution
                {
                    ProjectName = g.Key,
                    TotalAmount = g.Sum(d => d.Amount),
                    ContributionCount = g.Count(),
                    FirstContribution = g.Min(d => d.DonationDate),
                    LastContribution = g.Max(d => d.DonationDate)
                })
                .OrderByDescending(pc => pc.TotalAmount)
                .ToList();
        }

        private async Task<decimal> GetContributorPeriodTotalAsync(string email, DateTime startDate, DateTime endDate)
        {
            return await _context.MasjidDonations
                .Where(d => d.Email.ToLower() == email.ToLower() && 
                           d.DonationDate >= startDate && 
                           d.DonationDate <= endDate && 
                           d.PaymentStatus == "Completed")
                .SumAsync(d => d.Amount);
        }

        #endregion

        #region Recognition and Ranking

        public async Task<List<TopContributor>> GetTopContributorsAsync(string period = "month", int count = 10, bool includeAnonymous = false)
        {
            DateTime startDate;
            switch (period.ToLower())
            {
                case "week":
                    startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    break;
                case "year":
                    startDate = new DateTime(DateTime.Today.Year, 1, 1);
                    break;
                default: // month
                    startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;
            }

            var donations = await _context.MasjidDonations
                .Where(d => d.DonationDate >= startDate && d.PaymentStatus == "Completed")
                .Where(d => includeAnonymous || !d.IsAnonymous)
                .ToListAsync();

            return donations
                .GroupBy(d => new { d.Email, d.DonorName, d.IsAnonymous })
                .Select(g => new TopContributor
                {
                    Email = g.Key.Email,
                    Name = g.Key.IsAnonymous ? "Anonymous" : g.Key.DonorName,
                    Amount = g.Sum(d => d.Amount),
                    ContributionCount = g.Count(),
                    IsAnonymous = g.Key.IsAnonymous
                })
                .OrderByDescending(tc => tc.Amount)
                .Take(count)
                .ToList();
        }

        public async Task<ContributionRecognition> GetContributorRecognitionLevelAsync(string email)
        {
            var totalContributions = await _context.MasjidDonations
                .Where(d => d.Email.ToLower() == email.ToLower() && d.PaymentStatus == "Completed")
                .SumAsync(d => d.Amount);

            if (totalContributions >= 10000)
                return new ContributionRecognition { Level = "Platinum", Title = "Platinum Patron", MinimumAmount = 10000, BadgeIcon = "fas fa-gem", Description = "Exceptional community supporter" };
            else if (totalContributions >= 5000)
                return new ContributionRecognition { Level = "Gold", Title = "Gold Supporter", MinimumAmount = 5000, BadgeIcon = "fas fa-medal", Description = "Outstanding community contributor" };
            else if (totalContributions >= 2000)
                return new ContributionRecognition { Level = "Silver", Title = "Silver Member", MinimumAmount = 2000, BadgeIcon = "fas fa-award", Description = "Valued community member" };
            else if (totalContributions >= 500)
                return new ContributionRecognition { Level = "Bronze", Title = "Bronze Contributor", MinimumAmount = 500, BadgeIcon = "fas fa-trophy", Description = "Dedicated supporter" };
            else
                return new ContributionRecognition { Level = "Supporter", Title = "Community Supporter", MinimumAmount = 0, BadgeIcon = "fas fa-heart", Description = "Thank you for your support" };
        }

        public async Task<string> GetContributorRankAsync(string email, string period = "year")
        {
            var topContributors = await GetTopContributorsAsync(period, 1000, false);
            var contributorIndex = topContributors.FindIndex(tc => tc.Email.ToLower() == email.ToLower());
            
            if (contributorIndex == -1) return "Unranked";
            
            var rank = contributorIndex + 1;
            var suffix = rank switch
            {
                1 => "st",
                2 => "nd", 
                3 => "rd",
                _ => "th"
            };
            
            return $"{rank}{suffix} of {topContributors.Count}";
        }

        #endregion

        #region Maintenance

        public async Task<bool> RecalculateContributionTrackersAsync()
        {
            try
            {
                // Get all completed donations grouped by email
                var donationGroups = await _context.MasjidDonations
                    .Where(d => d.PaymentStatus == "Completed" && !string.IsNullOrEmpty(d.Email))
                    .GroupBy(d => d.Email.ToLower())
                    .ToListAsync();

                foreach (var group in donationGroups)
                {
                    var donations = group.OrderBy(d => d.DonationDate).ToList();
                    var email = group.Key;
                    var latestDonation = donations.Last();

                    var tracker = await _context.ContributionTrackers
                        .FirstOrDefaultAsync(ct => ct.Email.ToLower() == email);

                    if (tracker == null)
                    {
                        tracker = new ContributionTracker
                        {
                            Email = email,
                            CreatedDate = DateTime.Now
                        };
                        _context.ContributionTrackers.Add(tracker);
                    }

                    tracker.ContributorName = latestDonation.DonorName;
                    tracker.TotalContributions = donations.Sum(d => d.Amount);
                    tracker.ContributionCount = donations.Count;
                    tracker.FirstContributionDate = donations.First().DonationDate;
                    tracker.LastContributionDate = donations.Last().DonationDate;
                    tracker.AverageContribution = tracker.TotalContributions / tracker.ContributionCount;
                    tracker.PreferredPaymentMethod = donations.GroupBy(d => d.PaymentMethod)
                        .OrderByDescending(g => g.Count()).First().Key;
                    tracker.IsActiveContributor = tracker.LastContributionDate >= DateTime.Today.AddMonths(-6);
                    tracker.LastUpdated = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CleanupInactiveContributorsAsync(int monthsInactive = 24)
        {
            try
            {
                var cutoffDate = DateTime.Today.AddMonths(-monthsInactive);
                var inactiveTrackers = await _context.ContributionTrackers
                    .Where(ct => ct.LastContributionDate < cutoffDate)
                    .ToListAsync();

                foreach (var tracker in inactiveTrackers)
                {
                    tracker.IsActiveContributor = false;
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

        #region Export

        public async Task<byte[]> ExportContributionReportAsync(ContributionFilterOptions? filters = null)
        {
            var contributors = await GetAllContributorsAsync(filters, 1, int.MaxValue);
            
            var csv = new StringBuilder();
            csv.AppendLine("Contributor Name,Email,Total Amount,Contribution Count,Average Amount,First Contribution,Last Contribution");
            
            foreach (var contributor in contributors)
            {
                csv.AppendLine($"\"{contributor.ContributorName}\"," +
                             $"\"{contributor.Email}\"," +
                             $"{contributor.TotalAmount:C}," +
                             $"{contributor.ContributionCount}," +
                             $"{contributor.AverageAmount:C}," +
                             $"{contributor.FirstContribution:yyyy-MM-dd}," +
                             $"{contributor.LastContribution:yyyy-MM-dd}");
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportIndividualReportAsync(string email)
        {
            var report = await GetContributorReportAsync(email);
            
            var csv = new StringBuilder();
            csv.AppendLine("Individual Contributor Report");
            csv.AppendLine($"Name: {report.Summary.ContributorName}");
            csv.AppendLine($"Email: {report.Summary.Email}");
            csv.AppendLine($"Total Contributions: {report.Summary.TotalAmount:C}");
            csv.AppendLine($"Contribution Count: {report.Summary.ContributionCount}");
            csv.AppendLine($"Average Amount: {report.Summary.AverageAmount:C}");
            csv.AppendLine($"Year to Date: {report.YearToDateTotal:C}");
            csv.AppendLine($"Month to Date: {report.MonthToDateTotal:C}");
            csv.AppendLine($"Rank: {report.ContributorRank}");
            csv.AppendLine("");
            csv.AppendLine("Contribution History:");
            csv.AppendLine("Date,Amount,Project,Payment Method,Status");
            
            foreach (var contribution in report.Summary.Contributions)
            {
                csv.AppendLine($"{contribution.Date:yyyy-MM-dd}," +
                             $"{contribution.Amount:C}," +
                             $"\"{contribution.ProjectName}\"," +
                             $"\"{contribution.PaymentMethod}\"," +
                             $"\"{contribution.PaymentStatus}\"");
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        #endregion
    }
}