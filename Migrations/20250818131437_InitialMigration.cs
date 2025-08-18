using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GambianMuslimCommunity.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DateSent = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasjidProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Updates = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasjidProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrayerTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrayerTimes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasjidDonations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasjidProjectId = table.Column<int>(type: "int", nullable: false),
                    DonorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    DonationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasjidDonations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasjidDonations_MasjidProjects_MasjidProjectId",
                        column: x => x.MasjidProjectId,
                        principalTable: "MasjidProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CommunityEvents",
                columns: new[] { "Id", "Description", "EndTime", "EventDate", "EventType", "Location", "StartTime", "Title" },
                values: new object[,]
                {
                    { 1, "Join us for a community iftar during Ramadan", new TimeSpan(0, 20, 0, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Local), "Religious", "Community Center", new TimeSpan(0, 18, 0, 0, 0), "Community Iftar" },
                    { 2, "Learn about Islamic history and jurisprudence", new TimeSpan(0, 16, 0, 0, 0), new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Local), "Educational", "Main Hall", new TimeSpan(0, 14, 0, 0, 0), "Islamic Studies Workshop" },
                    { 3, "Islamic education and activities for young Muslims", new TimeSpan(0, 12, 0, 0, 0), new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Local), "Youth", "Youth Center", new TimeSpan(0, 10, 0, 0, 0), "Youth Program" }
                });

            migrationBuilder.InsertData(
                table: "MasjidProjects",
                columns: new[] { "Id", "CreatedDate", "CurrentAmount", "Description", "ImageUrl", "IsActive", "IsFeatured", "Location", "StartDate", "Status", "TargetAmount", "TargetCompletionDate", "Title", "Updates" },
                values: new object[] { 1, new DateTime(2025, 7, 19, 0, 0, 0, 0, DateTimeKind.Local), 125000m, "Alhamdulillah, by the grace of Allah (SWT), we are embarking on building a new masjid to serve our growing Gambian Muslim community in Minnesota. This masjid will provide a dedicated space for our five daily prayers, Jummah prayers, Islamic education, and community gatherings. The new facility will include a main prayer hall, separate women's prayer area, classrooms for Islamic studies, community kitchen, and parking facilities.", "/images/masjid-project.jpg", true, true, "Minneapolis, MN - 15 minutes from current location", new DateTime(2025, 7, 19, 0, 0, 0, 0, DateTimeKind.Local), "Fundraising", 500000m, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), "New Masjid Building Project", "Alhamdulillah! We have secured the land and received initial construction permits. Phase 1 fundraising is 25% complete. May Allah (SWT) bless all our donors and supporters." });

            migrationBuilder.InsertData(
                table: "PrayerTimes",
                columns: new[] { "Id", "City", "Date", "IsActive", "Name", "Time" },
                values: new object[,]
                {
                    { 1, "Minneapolis, MN", new DateTime(2025, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), true, "Fajr", new TimeSpan(0, 5, 45, 0, 0) },
                    { 2, "Minneapolis, MN", new DateTime(2025, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), true, "Dhuhr", new TimeSpan(0, 12, 15, 0, 0) },
                    { 3, "Minneapolis, MN", new DateTime(2025, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), true, "Asr", new TimeSpan(0, 14, 30, 0, 0) },
                    { 4, "Minneapolis, MN", new DateTime(2025, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), true, "Maghrib", new TimeSpan(0, 16, 45, 0, 0) },
                    { 5, "Minneapolis, MN", new DateTime(2025, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), true, "Isha", new TimeSpan(0, 18, 15, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Description", "IconClass", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Daily congregational prayers, Jummah prayers, and special occasion prayers including Eid celebrations.", "fas fa-pray", true, "Prayer Services" },
                    { 2, "Quran classes for children and adults, Islamic studies, Arabic language instruction, and religious guidance.", "fas fa-book-quran", true, "Islamic Education" },
                    { 3, "Counseling services, family support, assistance for new immigrants, and community welfare programs.", "fas fa-heart", true, "Community Support" },
                    { 4, "Gambian cultural celebrations, Islamic holidays, community gatherings, and interfaith dialogue events.", "fas fa-calendar-alt", true, "Cultural Events" },
                    { 5, "Islamic marriage ceremonies, family counseling, youth programs, and life event celebrations.", "fas fa-ring", true, "Marriage & Family" },
                    { 6, "Zakat collection and distribution, charitable giving coordination, and community assistance programs.", "fas fa-donate", true, "Zakat & Charity" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MasjidDonations_MasjidProjectId",
                table: "MasjidDonations",
                column: "MasjidProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityEvents");

            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "MasjidDonations");

            migrationBuilder.DropTable(
                name: "PrayerTimes");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "MasjidProjects");
        }
    }
}
