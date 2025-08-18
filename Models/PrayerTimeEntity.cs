namespace GambianMuslimCommunity.Models
{
    public class PrayerTimeEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan Time { get; set; }
        public DateTime Date { get; set; }
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}