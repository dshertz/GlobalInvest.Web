namespace GlobalInvest.ViewModels
{
    public class FundIndexViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Currency { get; set; }
        public NavSummaryViewModel NavSummary { get; set; } = new();
    }
}
