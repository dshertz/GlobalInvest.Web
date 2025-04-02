using GlobalInvest.ViewModels;

namespace GlobalInvest.Services
{
    public class TaxSummaryImportService
    {
        public List<TaxSummaryImportViewModel> ParseTaxText(string taxText)
        {
            var models = new List<TaxSummaryImportViewModel>();

            var lines = taxText.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var columns = line.Split("\t");
                if (columns.Length < 3) continue; // Hoppa över felaktiga rader

                if (int.TryParse(columns[0], out var incomeYear) &&
                    decimal.TryParse(columns[1], out var employmentIncome) &&
                    decimal.TryParse(columns[2], out var finalTax))
                {
                    models.Add(new TaxSummaryImportViewModel
                    {
                        IncomeYear = incomeYear,
                        EmploymentIncome = employmentIncome,
                        FinalTax = finalTax
                    });
                }
            }

            return models;
        }
    }
}