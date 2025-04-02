using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalInvest.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToIncomeReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "IncomeReports",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "IncomeReports");
        }
    }
}
