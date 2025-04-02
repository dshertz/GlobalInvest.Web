using GlobalInvest.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInvest.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<Fund> Funds { get; set; }
        public DbSet<NavCourse> NavCourses { get; set; }
        public DbSet<TaxSummary> TaxSummaries { get; set; }
        public DbSet<IncomeReport> IncomeReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Index för Date-kolumnen
            modelBuilder
                .Entity<NavCourse>()
                .HasIndex(n => n.Date)
                .HasDatabaseName("IX_NavCourse_Date");

            // Index för FundId-kolumnen
            modelBuilder
                .Entity<NavCourse>()
                .HasIndex(n => n.FundId)
                .HasDatabaseName("IX_NavCourse_FundId");

            // Gör att NAV-kurser raderas när fonden tas bort
            modelBuilder
                .Entity<NavCourse>()
                .HasOne(n => n.Fund)
                .WithMany(f => f.NavCourses)
                .HasForeignKey(n => n.FundId)
                .OnDelete(DeleteBehavior.Cascade);

            // Raderar alla TaxSummaries om IncomeReport raderas
            modelBuilder
                .Entity<TaxSummary>()
                .HasOne(ts => ts.IncomeReport)
                .WithMany(ir => ir.TaxSummaries)
                .HasForeignKey(ts => ts.IncomeReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
