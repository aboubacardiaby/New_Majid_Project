using GambianMuslimCommunity.Models;

namespace GambianMuslimCommunity.Services
{
    public interface ICommunityService
    {
        Task<List<Service>> GetActiveServicesAsync();
        Task<List<CommunityEvent>> GetUpcomingEventsAsync(int count = 0);
        Task<PrayerSchedule> GetPrayerScheduleAsync(string city = "Minneapolis, MN");
        Task<bool> SaveContactMessageAsync(ContactMessage message);
        Task<ImamWelcomeMessage> GetImamWelcomeMessageAsync();
        
        // Masjid Project methods
        Task<MasjidProject?> GetFeaturedMasjidProjectAsync();
        Task<List<MasjidProject>> GetActiveMasjidProjectsAsync();
        Task<MasjidProject?> GetMasjidProjectByIdAsync(int id);
        Task<bool> SaveDonationAsync(MasjidDonation donation);
        Task<bool> UpdateDonationAsync(MasjidDonation donation);
        Task<MasjidDonation?> GetDonationByIdAsync(int id);
        Task<MasjidDonation?> GetDonationByPayPalPaymentIdAsync(string payPalPaymentId);
        Task<List<MasjidDonation>> GetDonationsByProjectIdAsync(int projectId, bool includeAnonymous = false);
    }
}