using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GambianMuslimCommunity.Migrations
{
    /// <inheritdoc />
    public partial class AddContributionTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContributionTrackers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContributorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalContributions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContributionCount = table.Column<int>(type: "int", nullable: false),
                    FirstContributionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastContributionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AverageContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreferredPaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActiveContributor = table.Column<bool>(type: "bit", nullable: false),
                    ContributionNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContributionTrackers", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(8995));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9001));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9004));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9007));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9010));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9013));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9016));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9018));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9021));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9032));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9035));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9037));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9040));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9043));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9045));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9048));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9051));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9024));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 21,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9027));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 22,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 17, 22, 326, DateTimeKind.Local).AddTicks(9029));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContributionTrackers");

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

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9824));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 21,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9826));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 22,
                column: "LastModified",
                value: new DateTime(2025, 8, 18, 18, 5, 54, 650, DateTimeKind.Local).AddTicks(9829));
        }
    }
}
