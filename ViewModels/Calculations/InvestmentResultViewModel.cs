namespace GlobalInvest.ViewModels.Calculations
{
    public class InvestmentResultViewModel
    {
        public int IncomeYear { get; set; }
        public decimal MonthlyDisposableIncome { get; set; }
        public decimal MonthlyInvestment { get; set; }
        public string InvestmentDate { get; set; }
        public decimal NavValue { get; set; }
        public decimal SharesBought { get; set; }
        public decimal TotalShares { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitPercentage { get; set; }
        public decimal TotalInvested { get; set; }
    }
}