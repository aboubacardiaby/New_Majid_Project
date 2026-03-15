namespace GambianMuslimCommunity.Models
{
    public class DonationSuccessViewModel
    {
        public string DonorName    { get; set; } = string.Empty;
        public string Email        { get; set; } = string.Empty;
        public decimal Amount      { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime DonationDate { get; set; }
        public bool IsAnonymous    { get; set; }
        public bool EmailSent      { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public int    ProjectId    { get; set; }
        public decimal ProjectProgress { get; set; }
        public decimal TargetAmount    { get; set; }
        public decimal CurrentAmount   { get; set; }
    }

    public class HomeViewModel
    {
        public List<Service> Services { get; set; } = new();
        public List<CommunityEvent> UpcomingEvents { get; set; } = new();
        public PrayerSchedule PrayerSchedule { get; set; } = new();
        public MasjidProject? FeaturedMasjidProject { get; set; }
        public ImamWelcomeMessage ImamWelcome { get; set; } = new();
    }

    public class ImamWelcomeMessage
    {
        public string ImamName { get; set; } = string.Empty;
        public string WelcomeMessage { get; set; } = string.Empty;
        public string ImamImageUrl { get; set; } = string.Empty;
        public string ImamTitle { get; set; } = string.Empty;
    }
}