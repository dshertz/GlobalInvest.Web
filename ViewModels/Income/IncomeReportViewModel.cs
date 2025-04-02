namespace GlobalInvest.ViewModels
{
    public class IncomeReportViewModel
    {
        public int? Id { get; set; } // Kan vara null vid skapande
        public string Name { get; set; }
        public string Currency { get; set; } // Standardval
        public List<string> AvailableCurrencies { get; } = new() { "SEK", "EUR", "USD" };
        public List<TaxSummaryViewModel> TaxSummaries { get; set; } = new();
        // 🔹 Räknar antal unika år i TaxSummaries
        public int TaxYearsCount => TaxSummaries.Select(t => t.IncomeYear).Distinct().Count();
    }
}