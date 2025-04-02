namespace GlobalInvest.ViewModels
{
    public class TaxSummaryViewModel
    {
        public int Id { get; set; }
        public int IncomeYear { get; set; }
        public decimal EmploymentIncome { get; set; }
        public decimal FinalTax { get; set; }
        public decimal DisposableIncome => EmploymentIncome - FinalTax;
        public decimal MonthlyDisposableIncome => DisposableIncome / 12;
        public bool IsSelected { get; set; }
        public bool HasNavCourses { get; set; }
        public decimal MonthlyInvestment(decimal percentage)
        {
            return Math.Round(MonthlyDisposableIncome * (percentage / 100), MidpointRounding.AwayFromZero);
        }
    }
}