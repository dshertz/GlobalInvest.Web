using GlobalInvest.Helpers;

namespace GlobalInvest.ViewModels
{
    public class FundDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public List<int> AvailableYears { get; set; } = new();
        public NavSummaryViewModel NavSummary { get; set; } = new(); // 🟢 Säkerställer att det aldrig blir null
        public PaginatedList<NavCourseViewModel> NavCourses { get; set; } // 🔄 Byt till NavCourseViewModel!
    }
}
