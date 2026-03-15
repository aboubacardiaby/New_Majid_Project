using GambianMuslimCommunity.Data;
using GambianMuslimCommunity.Models;
using Microsoft.EntityFrameworkCore;

namespace GambianMuslimCommunity.Services
{
    public class CommunityService : ICommunityService
    {
        private readonly ApplicationDbContext _context;

        public CommunityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetActiveServicesAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<List<CommunityEvent>> GetUpcomingEventsAsync(int count = 0)
        {
            var query = _context.CommunityEvents
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate);

            if (count > 0)
            {
                return await query.Take(count).ToListAsync();
            }

            return await query.ToListAsync();
        }

        public async Task<PrayerSchedule> GetPrayerScheduleAsync(string city = "Minneapolis, MN")
        {
            var today = DateTime.Today;
            var prayerEntities = await _context.PrayerTimes
                .Where(p => p.Date == today && p.IsActive && p.City == city)
                .OrderBy(p => p.Time)
                .ToListAsync();

            // If no prayer times for today, get the default ones
            if (!prayerEntities.Any())
            {
                prayerEntities = await _context.PrayerTimes
                    .Where(p => p.IsActive && p.City == city)
                    .OrderBy(p => p.Time)
                    .ToListAsync();
            }

            var now = DateTime.Now.TimeOfDay;
            var prayers = prayerEntities.Select(p => new PrayerTime
            {
                Name = p.Name,
                Time = p.Time,
                IsNext = false
            }).ToList();

            // Mark the next prayer
            var nextPrayer = prayers.FirstOrDefault(p => p.Time > now);
            if (nextPrayer != null)
            {
                nextPrayer.IsNext = true;
            }
            else if (prayers.Any())
            {
                // If no prayer left today, mark Fajr as next
                prayers.First().IsNext = true;
            }

            return new PrayerSchedule
            {
                Date = today,
                Prayers = prayers,
                City = city
            };
        }

        public async Task<bool> SaveContactMessageAsync(ContactMessage message)
        {
            try
            {
                message.DateSent = DateTime.Now;
                _context.ContactMessages.Add(message);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ImamWelcomeMessage> GetImamWelcomeMessageAsync()
        {
            var settings = await _context.SiteSettings
                .Where(s => s.IsActive && s.Category == "General" && 
                       (s.SettingKey == "ImamName" || s.SettingKey == "ImamWelcomeMessage" || 
                        s.SettingKey == "ImamImageUrl" || s.SettingKey == "ImamTitle"))
                .ToListAsync();

            var settingsDict = settings.ToDictionary(s => s.SettingKey, s => s.SettingValue);

            return new ImamWelcomeMessage
            {
                ImamName = settingsDict.GetValueOrDefault("ImamName", "Imam Abdullah Jallow"),
                WelcomeMessage = settingsDict.GetValueOrDefault("ImamWelcomeMessage", 
                    "Assalamu Alaikum wa Rahmatullahi wa Barakatuh, dear brothers and sisters. Welcome to our vibrant Gambian Muslim Community in Minnesota."),
                ImamImageUrl = settingsDict.GetValueOrDefault("ImamImageUrl", "/images/imam-placeholder.jpg"),
                ImamTitle = settingsDict.GetValueOrDefault("ImamTitle", "Community Imam & Spiritual Leader")
            };
        }

        public async Task<MasjidProject?> GetFeaturedMasjidProjectAsync()
        {
            try
            {
                return await _context.MasjidProjects
                    .Where(p => p.IsActive && p.IsFeatured)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                // Return null if MasjidProjects table doesn't exist yet
                return null;
            }
        }

        public async Task<List<MasjidProject>> GetActiveMasjidProjectsAsync()
        {
            try
            {
                return await _context.MasjidProjects
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                return new List<MasjidProject>();
            }
        }

        public async Task<MasjidProject?> GetMasjidProjectByIdAsync(int id)
        {
            try
            {
                return await _context.MasjidProjects
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SaveDonationAsync(MasjidDonation donation)
        {
            try
            {
                donation.DonationDate = DateTime.Now;
                _context.MasjidDonations.Add(donation);
                
                // Only update project amount for completed payments
                if (donation.PaymentStatus == "Completed")
                {
                    var project = await _context.MasjidProjects.FindAsync(donation.MasjidProjectId);
                    if (project != null)
                    {
                        project.CurrentAmount += donation.Amount;
                    }
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDonationAsync(MasjidDonation donation)
        {
            try
            {
                _context.MasjidDonations.Update(donation);
                
                // Update project amount when payment is completed
                if (donation.PaymentStatus == "Completed")
                {
                    var project = await _context.MasjidProjects.FindAsync(donation.MasjidProjectId);
                    if (project != null)
                    {
                        project.CurrentAmount += donation.Amount;
                    }
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<MasjidDonation?> GetDonationByIdAsync(int id)
        {
            try
            {
                return await _context.MasjidDonations.FindAsync(id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<MasjidDonation?> GetDonationByPayPalPaymentIdAsync(string payPalPaymentId)
        {
            try
            {
                return await _context.MasjidDonations
                    .FirstOrDefaultAsync(d => d.PayPalPaymentId == payPalPaymentId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<MasjidDonation>> GetDonationsByProjectIdAsync(int projectId, bool includeAnonymous = false)
        {
            var query = _context.MasjidDonations
                .Where(d => d.MasjidProjectId == projectId);

            if (!includeAnonymous)
            {
                query = query.Where(d => !d.IsAnonymous);
            }

            return await query
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }
    }
}