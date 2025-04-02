using System.ComponentModel.DataAnnotations;

namespace GlobalInvest.ViewModels
{
    public class NavSummaryViewModel
    {
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? EndDate { get; set; }
        public int TotalCourses { get; set; }
    }
}
