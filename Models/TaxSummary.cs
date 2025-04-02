namespace GlobalInvest.Models
{
    public class TaxSummary
    {
        public int Id { get; set; } // Primärnyckel
        public int IncomeYear { get; set; } // Inkomstår
        public decimal EmploymentIncome { get; set; } // Överskott av tjänst
        public decimal FinalTax { get; set; } // Summa slutlig skatt
        public int IncomeReportId { get; set; }  // 🔗 Utländsk nyckel
        public IncomeReport IncomeReport { get; set; }  // Navigationsproperty
    }
}