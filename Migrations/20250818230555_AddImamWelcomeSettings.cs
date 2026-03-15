using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GambianMuslimCommunity.Migrations
{
    /// <inheritdoc />
    public partial class AddImamWelcomeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9796));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9801));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9805));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9808));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9810));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9813));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9816));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9818));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9821));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9831));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9834));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9837));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9839));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9842));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9844));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9847));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9849));

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Category", "Description", "IsActive", "LastModified", "ModifiedBy", "SettingKey", "SettingValue" },
                values: new object[,]
                {
                    { 20, "General", "Imam's welcome message to the community", true, new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9824), "", "ImamWelcomeMessage", "Assalamu Alaikum wa Rahmatullahi wa Barakatuh, dear brothers and sisters. Welcome to our vibrant Gambian Muslim Community in Minnesota. May Allah (SWT) bless you and your families as we come together to worship, learn, and support one another in faith. Our community is a place where Islamic values flourish, cultural heritage is preserved, and bonds of brotherhood and sisterhood grow stronger each day." },
                    { 21, "General", "URL for the Imam's photo", true, new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9826), "", "ImamImageUrl", "/images/imam-placeholder.jpg" },
                    { 22, "General", "Imam's title or position", true, new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9829), "", "ImamTitle", "Community Imam & Spiritual Leader" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7685));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7689));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7691));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7692));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7694));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7695));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7697));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7699));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7700));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7702));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7703));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7705));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7706));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7708));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7709));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7711));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 9, 31, 13, 139, DateTimeKind.Local).AddTicks(7713));
        }
    }
}
