namespace GambianMuslimCommunity.Models
{
    public class HomeViewModel
    {
        public List<Service> Services { get; set; } = new();
        public List<CommunityEvent> UpcomingEvents { get; set; } = new();
        public PrayerSchedule PrayerSchedule { get; set; } = new();
        public MasjidProject? FeaturedMasjidProject { get; set; }
    }
}