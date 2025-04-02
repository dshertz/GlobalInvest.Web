using System.Diagnostics;
using GlobalInvest.Data;
using GlobalInvest.ViewModels.Calculations;
using Microsoft.EntityFrameworkCore;

namespace GlobalInvest.Services
{
    public class CalculationService
    {
        private readonly AppDbContext _context;

        public CalculationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CalculationViewModel> CalculateInvestments(
            int incomeReportId,
            int fundId,
            decimal investmentPercentage,
            List<int> selectedYears,
            int investmentDay
        )
        {
            var stopwatch = Stopwatch.StartNew();

            var report = await _context
                .IncomeReports.Include(r => r.TaxSummaries)
                .FirstOrDefaultAsync(r => r.Id == incomeReportId);

            if (report == null)
                return new CalculationViewModel();

            var investmentResults = new List<InvestmentResultViewModel>();
            decimal totalShares = 0;
            decimal totalInvested = 0;

            // 🟢 Hämta alla NAV-kurser för fonden
            var allNavCourses = await _context
                .NavCourses.Where(n => n.FundId == fundId)
                .OrderBy(n => n.Date)
                .ToListAsync();

            var navLookup = allNavCourses
                .GroupBy(n => (Year: n.Date.Year, Month: n.Date.Month))
                .ToDictionary(
                    g => g.Key,
                    g =>
                        g.OrderBy(n => n.Date).FirstOrDefault(n => n.Date.Day >= investmentDay)
                        ?? g.FirstOrDefault()
                );

            foreach (
                var tax in report.TaxSummaries.Where(t => selectedYears.Contains(t.IncomeYear))
            )
            {
                decimal monthlyDisposableIncome = (tax.EmploymentIncome - tax.FinalTax) / 12;
                decimal monthlyInvestment = Math.Round(
                    monthlyDisposableIncome * (investmentPercentage / 100),
                    MidpointRounding.AwayFromZero
                );

                for (int month = 1; month <= 12; month++)
                {
                    if (
                        navLookup.TryGetValue((tax.IncomeYear, month), out var navCourse)
                        && navCourse != null
                    )
                    {
                        var sharesBought = monthlyInvestment / navCourse.NavValue;
                        totalShares += sharesBought;
                        totalInvested += monthlyInvestment;
                        var totalPortfolioValue = totalShares * navCourse.NavValue;
                        var profit = totalPortfolioValue - totalInvested;
                        var profitPercentage =
                            totalInvested > 0 ? (profit / totalInvested) * 100 : 0;

                        investmentResults.Add(
                            new InvestmentResultViewModel
                            {
                                IncomeYear = tax.IncomeYear,
                                MonthlyDisposableIncome = monthlyDisposableIncome,
                                MonthlyInvestment = monthlyInvestment,
                                InvestmentDate = navCourse.Date.ToString("yyyy-MM-dd"),
                                NavValue = navCourse.NavValue,
                                SharesBought = sharesBought,
                                TotalShares = totalShares,
                                TotalPortfolioValue = totalPortfolioValue,
                                TotalInvested = totalInvested,
                                Profit = profit,
                                ProfitPercentage = profitPercentage,
                            }
                        );
                    }
                }
            }

            // 🔥 **Beräkna FIRE-målet**
            var fireTarget = CalculateFireTarget(investmentResults, investmentPercentage);

            stopwatch.Stop();
            Console.WriteLine($"CalculateInvestments tog {stopwatch.ElapsedMilliseconds} ms");

            return new CalculationViewModel
            {
                InvestmentResults = investmentResults,
                FireTargetDate = fireTarget?.FireDate,
                FireTargetValue = fireTarget?.FireValue,
            };
        }

        // 🔥 **Hitta första gången FIRE-målet uppnås**
        private (string FireDate, decimal FireValue)? CalculateFireTarget(
            List<InvestmentResultViewModel> investmentResults,
            decimal investmentPercentage
        )
        {
            if (!investmentResults.Any())
            {
                Console.WriteLine("🔥 Inga investeringsresultat att analysera.");
                return null;
            }

            var lastResult = investmentResults.Last();
            decimal lastFireGoal =
                (1 - (investmentPercentage / 100)) * lastResult.MonthlyDisposableIncome * 12 * 25;

            Console.WriteLine($"🔥 Sista punktens datum: {lastResult.InvestmentDate}");
            Console.WriteLine($"🔥 Sista portföljvärde: {lastResult.TotalPortfolioValue:N0}");
            Console.WriteLine($"🔥 FIRE-mål vid sista punkten: {lastFireGoal:N0}");

            if (lastResult.TotalPortfolioValue < lastFireGoal)
            {
                Console.WriteLine(
                    $"🔥 FIRE ej uppnått. Returnerar sista punkten: {lastResult.InvestmentDate}, FireGoal: {lastFireGoal:N0}"
                );
                return (lastResult.InvestmentDate, lastFireGoal);
            }

            for (int i = investmentResults.Count - 2; i >= 0; i--)
            {
                decimal fireGoal =
                    (1 - (investmentPercentage / 100))
                    * investmentResults[i].MonthlyDisposableIncome
                    * 12
                    * 25;
                decimal portfolioValue = investmentResults[i].TotalPortfolioValue;

                Console.WriteLine(
                    $"🔥 Kontroll vid {investmentResults[i].InvestmentDate}: FireGoal={fireGoal:N0}, Portfolio={portfolioValue:N0}"
                );

                if (portfolioValue < fireGoal)
                {
                    string fireDate = investmentResults[i + 1].InvestmentDate;
                    // decimal fireValue = investmentResults[i + 1].TotalPortfolioValue;
                    decimal fireValue =
                        (1 - (investmentPercentage / 100))
                        * investmentResults[i + 1].MonthlyDisposableIncome
                        * 12
                        * 25;

                    Console.WriteLine(
                        $"🔥 Första FIRE-målet uppnått vid: {fireDate}, Portföljvärde: {fireValue:N0}"
                    );
                    return (fireDate, fireValue);
                }
            }
            return (lastResult.InvestmentDate, lastResult.TotalPortfolioValue);
        }
    }
}