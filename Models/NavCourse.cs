namespace GlobalInvest.Models
{
    public class NavCourse
    {
        public int Id { get; set; }
        public decimal NavValue { get; set; }
        public DateTime Date { get; set; }
        public int FundId { get; set; }
        public Fund Fund { get; set; } = null!;
    }
}
