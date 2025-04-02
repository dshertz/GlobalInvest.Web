namespace GlobalInvest.Models
{
    public class IncomeReport
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Currency { get; set; }
        public ICollection<TaxSummary> TaxSummaries { get; set; } = new List<TaxSummary>();
    }
}
