using GambianMuslimCommunity.Models;
using Microsoft.EntityFrameworkCore;

namespace GambianMuslimCommunity.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<CommunityEvent> CommunityEvents { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<PrayerTimeEntity> PrayerTimes { get; set; }
        public DbSet<MasjidProject> MasjidProjects { get; set; }
        public DbSet<MasjidDonation> MasjidDonations { get; set; }
        
        // Admin entities
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<AdminSession> AdminSessions { get; set; }
        public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }
        public DbSet<ContributionTracker> ContributionTrackers { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        
        // Member entities
        public DbSet<Member> Members { get; set; }
        public DbSet<MemberActivityLog> MemberActivityLogs { get; set; }
        public DbSet<MembershipSettings> MembershipSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Service entity
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.IconClass).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure CommunityEvent entity
            modelBuilder.Entity<CommunityEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EventType).HasMaxLength(50);
            });

            // Configure ContactMessage entity
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Subject).HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.DateSent).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.ReadBy).HasMaxLength(100);
            });

            // Configure PrayerTimeEntity
            modelBuilder.Entity<PrayerTimeEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Date).HasColumnType("date");
                entity.Property(e => e.Time).HasColumnType("time");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure MasjidProject entity
            modelBuilder.Entity<MasjidProject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.TargetAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.Updates).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.IsFeatured).HasDefaultValue(true);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");

                // Ignore computed properties
                entity.Ignore(e => e.ProgressPercentage);
                entity.Ignore(e => e.DaysRemaining);
            });

            // Configure MasjidDonation entity
            modelBuilder.Entity<MasjidDonation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DonorName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.PayPalPaymentId).HasMaxLength(100);
                entity.Property(e => e.PayPalPayerId).HasMaxLength(100);
                entity.Property(e => e.PaymentStatus).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.DonationDate).HasDefaultValueSql("GETDATE()");

                // Configure relationship
                entity.HasOne(d => d.MasjidProject)
                      .WithMany()
                      .HasForeignKey(d => d.MasjidProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AdminUser entity
            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("Admin");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ProfilePicture).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                
                // Unique constraints
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure AdminSession entity
            modelBuilder.Entity<AdminSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionToken).IsRequired().HasMaxLength(500);
                entity.Property(e => e.LoginDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.LastActivity).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);

                // Configure relationship
                entity.HasOne(s => s.AdminUser)
                      .WithMany(u => u.Sessions)
                      .HasForeignKey(s => s.AdminUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AdminActivityLog entity
            modelBuilder.Entity<AdminActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Entity).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ActivityDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IpAddress).HasMaxLength(200);

                // Configure relationship
                entity.HasOne(l => l.AdminUser)
                      .WithMany(u => u.ActivityLogs)
                      .HasForeignKey(l => l.AdminUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SiteSettings entity
            modelBuilder.Entity<SiteSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SettingKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SettingValue).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LastModified).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                
                // Unique constraint
                entity.HasIndex(e => new { e.SettingKey, e.Category }).IsUnique();
            });

            // Configure Member entity
            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Profession).HasMaxLength(100);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.MaritalStatus).HasMaxLength(50);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
                entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactRelationship).HasMaxLength(100);
                entity.Property(e => e.MembershipStatus).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.RegistrationDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.ReceiveEmailNotifications).HasDefaultValue(true);
                entity.Property(e => e.ReceiveSmsNotifications).HasDefaultValue(true);
                entity.Property(e => e.PreferredLanguage).HasMaxLength(100).HasDefaultValue("English");
                
                // Unique constraint for email
                entity.HasIndex(e => e.Email).IsUnique();
                
                // Configure relationship with AdminUser
                entity.HasOne(m => m.ApprovedBy)
                      .WithMany()
                      .HasForeignKey(m => m.ApprovedById)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure MemberActivityLog entity
            modelBuilder.Entity<MemberActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Entity).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ActivityDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                
                // Configure relationships
                entity.HasOne(l => l.Member)
                      .WithMany(m => m.ActivityLogs)
                      .HasForeignKey(l => l.MemberId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(l => l.AdminUser)
                      .WithMany()
                      .HasForeignKey(l => l.AdminUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure MembershipSettings entity
            modelBuilder.Entity<MembershipSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SettingKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SettingValue).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LastModified).HasDefaultValueSql("GETDATE()");
                
                // Unique constraint
                entity.HasIndex(e => e.SettingKey).IsUnique();
                
                // Configure relationship
                entity.HasOne(s => s.ModifiedBy)
                      .WithMany()
                      .HasForeignKey(s => s.ModifiedById)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Services
            modelBuilder.Entity<Service>().HasData(
                new Service
                {
                    Id = 1,
                    Title = "Prayer Services",
                    Description = "Daily congregational prayers, Jummah prayers, and special occasion prayers including Eid celebrations.",
                    IconClass = "fas fa-pray",
                    IsActive = true
                },
                new Service
                {
                    Id = 2,
                    Title = "Islamic Education",
                    Description = "Quran classes for children and adults, Islamic studies, Arabic language instruction, and religious guidance.",
                    IconClass = "fas fa-book-quran",
                    IsActive = true
                },
                new Service
                {
                    Id = 3,
                    Title = "Community Support",
                    Description = "Counseling services, family support, assistance for new immigrants, and community welfare programs.",
                    IconClass = "fas fa-heart",
                    IsActive = true
                },
                new Service
                {
                    Id = 4,
                    Title = "Cultural Events",
                    Description = "Gambian cultural celebrations, Islamic holidays, community gatherings, and interfaith dialogue events.",
                    IconClass = "fas fa-calendar-alt",
                    IsActive = true
                },
                new Service
                {
                    Id = 5,
                    Title = "Marriage & Family",
                    Description = "Islamic marriage ceremonies, family counseling, youth programs, and life event celebrations.",
                    IconClass = "fas fa-ring",
                    IsActive = true
                },
                new Service
                {
                    Id = 6,
                    Title = "Zakat & Charity",
                    Description = "Zakat collection and distribution, charitable giving coordination, and community assistance programs.",
                    IconClass = "fas fa-donate",
                    IsActive = true
                }
            );

            // Seed Community Events
            modelBuilder.Entity<CommunityEvent>().HasData(
                new CommunityEvent
                {
                    Id = 1,
                    Title = "Community Iftar",
                    Description = "Join us for a community iftar during Ramadan",
                    EventDate = DateTime.Today.AddDays(10),
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(20, 0, 0),
                    Location = "Community Center",
                    EventType = "Religious"
                },
                new CommunityEvent
                {
                    Id = 2,
                    Title = "Islamic Studies Workshop",
                    Description = "Learn about Islamic history and jurisprudence",
                    EventDate = DateTime.Today.AddDays(25),
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0),
                    Location = "Main Hall",
                    EventType = "Educational"
                },
                new CommunityEvent
                {
                    Id = 3,
                    Title = "Youth Program",
                    Description = "Islamic education and activities for young Muslims",
                    EventDate = DateTime.Today.AddDays(32),
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(12, 0, 0),
                    Location = "Youth Center",
                    EventType = "Youth"
                }
            );

            // Seed Prayer Times for Minneapolis
            var today = DateTime.Today;
            modelBuilder.Entity<PrayerTimeEntity>().HasData(
                new PrayerTimeEntity
                {
                    Id = 1,
                    Name = "Fajr",
                    Time = new TimeSpan(5, 45, 0),
                    Date = today,
                    City = "Minneapolis, MN",
                    IsActive = true
                },
                new PrayerTimeEntity
                {
                    Id = 2,
                    Name = "Dhuhr",
                    Time = new TimeSpan(12, 15, 0),
                    Date = today,
                    City = "Minneapolis, MN",
                    IsActive = true
                },
                new PrayerTimeEntity
                {
                    Id = 3,
                    Name = "Asr",
                    Time = new TimeSpan(14, 30, 0),
                    Date = today,
                    City = "Minneapolis, MN",
                    IsActive = true
                },
                new PrayerTimeEntity
                {
                    Id = 4,
                    Name = "Maghrib",
                    Time = new TimeSpan(16, 45, 0),
                    Date = today,
                    City = "Minneapolis, MN",
                    IsActive = true
                },
                new PrayerTimeEntity
                {
                    Id = 5,
                    Name = "Isha",
                    Time = new TimeSpan(18, 15, 0),
                    Date = today,
                    City = "Minneapolis, MN",
                    IsActive = true
                }
            );

            // Seed Masjid Project
            modelBuilder.Entity<MasjidProject>().HasData(
                new MasjidProject
                {
                    Id = 1,
                    Title = "New Masjid Building Project",
                    Description = "Alhamdulillah, by the grace of Allah (SWT), we are embarking on building a new masjid to serve our growing Gambian Muslim community in Minnesota. This masjid will provide a dedicated space for our five daily prayers, Jummah prayers, Islamic education, and community gatherings. The new facility will include a main prayer hall, separate women's prayer area, classrooms for Islamic studies, community kitchen, and parking facilities.",
                    TargetAmount = 500000m,
                    CurrentAmount = 125000m,
                    StartDate = DateTime.Today.AddDays(-30),
                    TargetCompletionDate = DateTime.Today.AddDays(365),
                    Status = "Fundraising",
                    Location = "Minneapolis, MN - 15 minutes from current location",
                    Updates = "Alhamdulillah! We have secured the land and received initial construction permits. Phase 1 fundraising is 25% complete. May Allah (SWT) bless all our donors and supporters.",
                    IsFeatured = true,
                    IsActive = true,
                    CreatedDate = DateTime.Today.AddDays(-30),
                    ImageUrl = "/images/masjid-project.jpg"
                }
            );

            // Seed default admin user (password: Admin123!)
            modelBuilder.Entity<AdminUser>().HasData(
                new AdminUser
                {
                    Id = 1,
                    FullName = "System Administrator",
                    Email = "admin@gambianmuslimcommunity.org",
                    Username = "admin",
                    PasswordHash = "$2a$11$N.Kt4q5BKkYLGgpF.jKMu.J0WpFbOJGnvf9..jvVt8OAVS5PCIK3y", // Admin123!
                    Role = "SuperAdmin",
                    IsActive = true,
                    CreatedDate = DateTime.Today.AddDays(-30),
                    CreatedBy = "System"
                }
            );
            string passwordHash = BCrypt.Net.BCrypt.HashPassword("Diaa260975");
            modelBuilder.Entity<AdminUser>().HasData(
              new AdminUser
              {
                  Id = 2,
                  FullName = "Abou Diaby",
                  Email = "ab.diaby@gmail.com",
                  Username = "aboudiaby",
                  PasswordHash = passwordHash,
                  Role = "SuperAdmin",
                  IsActive = true,
                  CreatedDate = DateTime.Today.AddDays(-30),
                  CreatedBy = "System"
              }
          );
            // Seed default site settings
            modelBuilder.Entity<SiteSettings>().HasData(
                new SiteSettings { Id = 1, SettingKey = "SiteName", SettingValue = "Gambian Muslim Community in Minnesota", Category = "General", Description = "The name of the website" },
                new SiteSettings { Id = 2, SettingKey = "SiteDescription", SettingValue = "Serving the Gambian Muslim community in Minnesota with Islamic services, education, and support", Category = "General", Description = "Brief description of the website" },
                new SiteSettings { Id = 3, SettingKey = "ContactEmail", SettingValue = "info@gambianmuslimcommunity.org", Category = "Contact", Description = "Primary contact email address" },
                new SiteSettings { Id = 4, SettingKey = "ContactPhone", SettingValue = "(612) 555-0123", Category = "Contact", Description = "Primary contact phone number" },
                new SiteSettings { Id = 5, SettingKey = "Address", SettingValue = "123 Main Street", Category = "Contact", Description = "Physical address" },
                new SiteSettings { Id = 6, SettingKey = "City", SettingValue = "Minneapolis", Category = "Contact", Description = "City" },
                new SiteSettings { Id = 7, SettingKey = "State", SettingValue = "MN", Category = "Contact", Description = "State" },
                new SiteSettings { Id = 8, SettingKey = "ZipCode", SettingValue = "55401", Category = "Contact", Description = "ZIP code" },
                new SiteSettings { Id = 9, SettingKey = "ImamName", SettingValue = "Imam Abdullah Jallow", Category = "General", Description = "Name of the community imam" },
                new SiteSettings { Id = 20, SettingKey = "ImamWelcomeMessage", SettingValue = "Assalamu Alaikum wa Rahmatullahi wa Barakatuh, dear brothers and sisters. Welcome to our vibrant Gambian Muslim Community in Minnesota. May Allah (SWT) bless you and your families as we come together to worship, learn, and support one another in faith. Our community is a place where Islamic values flourish, cultural heritage is preserved, and bonds of brotherhood and sisterhood grow stronger each day.", Category = "General", Description = "Imam's welcome message to the community" },
                new SiteSettings { Id = 21, SettingKey = "ImamImageUrl", SettingValue = "/images/imam-placeholder.jpg", Category = "General", Description = "URL for the Imam's photo" },
                new SiteSettings { Id = 22, SettingKey = "ImamTitle", SettingValue = "Community Imam & Spiritual Leader", Category = "General", Description = "Imam's title or position" },
                new SiteSettings { Id = 10, SettingKey = "FacebookUrl", SettingValue = "https://facebook.com/gambianmuslimcommunity", Category = "Social", Description = "Facebook page URL" },
                new SiteSettings { Id = 11, SettingKey = "InstagramUrl", SettingValue = "https://instagram.com/gambianmuslimmn", Category = "Social", Description = "Instagram page URL" },
                new SiteSettings { Id = 12, SettingKey = "WhatsAppNumber", SettingValue = "+16125550123", Category = "Social", Description = "WhatsApp contact number" },
                new SiteSettings { Id = 13, SettingKey = "LogoUrl", SettingValue = "/images/logo.png", Category = "General", Description = "Website logo URL" },
                new SiteSettings { Id = 14, SettingKey = "SmtpServer", SettingValue = "smtp.gmail.com", Category = "Email", Description = "SMTP server for sending emails" },
                new SiteSettings { Id = 15, SettingKey = "SmtpPort", SettingValue = "587", Category = "Email", Description = "SMTP server port" },
                new SiteSettings { Id = 16, SettingKey = "FromEmail", SettingValue = "noreply@gambianmuslimcommunity.org", Category = "Email", Description = "From email address for system emails" },
                new SiteSettings { Id = 17, SettingKey = "FromName", SettingValue = "Gambian Muslim Community", Category = "Email", Description = "From name for system emails" }
            );
        }
    }
}