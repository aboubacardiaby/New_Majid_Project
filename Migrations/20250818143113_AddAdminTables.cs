using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GambianMuslimCommunity.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "ContactMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReadBy",
                table: "ContactMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadDate",
                table: "ContactMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Admin"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProfilePicture = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminUserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IpAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminActivityLogs_AdminUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AdminUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminUserId = table.Column<int>(type: "int", nullable: false),
                    SessionToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LoginDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IpAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminSessions_AdminUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AdminUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AdminUsers",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Email", "FullName", "IsActive", "LastLoginDate", "PasswordHash", "PhoneNumber", "ProfilePicture", "Role", "Username" },
                values: new object[] { 1, "System", new DateTime(2025, 7, 19, 0, 0, 0, 0, DateTimeKind.Local), "admin@gambianmuslimcommunity.org", "System Administrator", true, null, "$2a$11$N.Kt4q5BKkYLGgpF.jKMu.J0WpFbOJGnvf9..jvVt8OAVS5PCIK3y", null, null, "SuperAdmin", "admin" });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Category", "Description", "IsActive", "LastModified", "ModifiedBy", "SettingKey", "SettingValue" },
                values: new object[,]
                {
                    { 1, "General", "The name of the website", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7685), "", "SiteName", "Gambian Muslim Community in Minnesota" },
                    { 2, "General", "Brief description of the website", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7689), "", "SiteDescription", "Serving the Gambian Muslim community in Minnesota with Islamic services, education, and support" },
                    { 3, "Contact", "Primary contact email address", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7691), "", "ContactEmail", "info@gambianmuslimcommunity.org" },
                    { 4, "Contact", "Primary contact phone number", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7692), "", "ContactPhone", "(612) 555-0123" },
                    { 5, "Contact", "Physical address", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7694), "", "Address", "123 Main Street" },
                    { 6, "Contact", "City", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7695), "", "City", "Minneapolis" },
                    { 7, "Contact", "State", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7697), "", "State", "MN" },
                    { 8, "Contact", "ZIP code", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7699), "", "ZipCode", "55401" },
                    { 9, "General", "Name of the community imam", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7700), "", "ImamName", "Imam Abdullah Jallow" },
                    { 10, "Social", "Facebook page URL", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7702), "", "FacebookUrl", "https://facebook.com/gambianmuslimcommunity" },
                    { 11, "Social", "Instagram page URL", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7703), "", "InstagramUrl", "https://instagram.com/gambianmuslimmn" },
                    { 12, "Social", "WhatsApp contact number", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7705), "", "WhatsAppNumber", "+16125550123" },
                    { 13, "General", "Website logo URL", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7706), "", "LogoUrl", "/images/logo.png" },
                    { 14, "Email", "SMTP server for sending emails", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7708), "", "SmtpServer", "smtp.gmail.com" },
                    { 15, "Email", "SMTP server port", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7709), "", "SmtpPort", "587" },
                    { 16, "Email", "From email address for system emails", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7711), "", "FromEmail", "noreply@gambianmuslimcommunity.org" },
                    { 17, "Email", "From name for system emails", true, new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7713), "", "FromName", "Gambian Muslim Community" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminActivityLogs_AdminUserId",
                table: "AdminActivityLogs",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminSessions_AdminUserId",
                table: "AdminSessions",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_SettingKey_Category",
                table: "SiteSettings",
                columns: new[] { "SettingKey", "Category" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminActivityLogs");

            migrationBuilder.DropTable(
                name: "AdminSessions");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "ReadBy",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "ReadDate",
                table: "ContactMessages");
        }
    }
}
