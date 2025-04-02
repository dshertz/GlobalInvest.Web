using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalInvest.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncomeReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NavCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NavValue = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavCourses_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomeYear = table.Column<int>(type: "INTEGER", nullable: false),
                    EmploymentIncome = table.Column<decimal>(type: "TEXT", nullable: false),
                    FinalTax = table.Column<decimal>(type: "TEXT", nullable: false),
                    DisposableIncome = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsSelected = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncomeReportId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxSummaries_IncomeReports_IncomeReportId",
                        column: x => x.IncomeReportId,
                        principalTable: "IncomeReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NavCourse_Date",
                table: "NavCourses",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_NavCourse_FundId",
                table: "NavCourses",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxSummaries_IncomeReportId",
                table: "TaxSummaries",
                column: "IncomeReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NavCourses");

            migrationBuilder.DropTable(
                name: "TaxSummaries");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "IncomeReports");
        }
    }
}
