using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GambianMuslimCommunity.Migrations
{
    /// <inheritdoc />
    public partial class AddPayPalFieldsToMasjidDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayPalPayerId",
                table: "MasjidDonations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PayPalPaymentId",
                table: "MasjidDonations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "MasjidDonations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayPalPayerId",
                table: "MasjidDonations");

            migrationBuilder.DropColumn(
                name: "PayPalPaymentId",
                table: "MasjidDonations");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "MasjidDonations");
        }
    }
}
