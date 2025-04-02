using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalInvest.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDisposableIncomeAndIsSelectedFromTaxSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisposableIncome",
                table: "TaxSummaries");

            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "TaxSummaries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DisposableIncome",
                table: "TaxSummaries",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "TaxSummaries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
