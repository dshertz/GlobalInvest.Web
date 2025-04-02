namespace GlobalInvest.Models
{
    public class Fund
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Currency { get; set; }
        public ICollection<NavCourse> NavCourses { get; set; } = new List<NavCourse>();
    }
}
