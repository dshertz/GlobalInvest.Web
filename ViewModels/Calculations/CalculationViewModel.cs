using GlobalInvest.ViewModels.Fund;

namespace GlobalInvest.ViewModels.Calculations
{
    public class CalculationViewModel
    {
        public List<IncomeReportViewModel> IncomeReports { get; set; } = new();
        public int? SelectedIncomeReportId { get; set; }
        public List<FundSelectionViewModel> Funds { get; set; } = new();
        public int? SelectedFundId { get; set; }
        public List<int> NavYears { get; set; } = new();
        public decimal InvestmentPercentage { get; set; } = 10;
        public int InvestmentDay { get; set; } = 25; // Default to 25
        public List<InvestmentResultViewModel> InvestmentResults { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new List<int>();
        public string? FireTargetDate { get; set; }
        public decimal? FireTargetValue { get; set; }
    }
}