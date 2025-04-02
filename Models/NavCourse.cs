namespace GlobalInvest.Models
{
    public class NavCourse
    {
        public int Id { get; set; }
        public required string FundName { get; set; }
        public required string Currency { get; set; }
        public decimal? Value { get; set; }
        public DateTime? Date { get; set; }
        public int FundId { get; set; }
        public required Fund Fund { get; set; }
    }
    /* public class NavCourse
    {
        public int Id { get; set; }
        public decimal NavValue { get; set; }
        public DateTime Date { get; set; }
        public int FundId { get; set; }
        public Fund Fund { get; set; }
    } */
}
