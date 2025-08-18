namespace GambianMuslimCommunity.Models
{
    public class PrayerTime
    {
        public string Name { get; set; } = string.Empty;
        public TimeSpan Time { get; set; }
        public bool IsNext { get; set; }
    }

    public class PrayerSchedule
    {
        public DateTime Date { get; set; }
        public List<PrayerTime> Prayers { get; set; } = new();
        public string City { get; set; } = "Minneapolis, MN";
    }
}