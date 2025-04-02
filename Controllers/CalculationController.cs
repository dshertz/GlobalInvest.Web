using GlobalInvest.Data;
using GlobalInvest.Services;
using GlobalInvest.ViewModels;
using GlobalInvest.ViewModels.Calculations;
using GlobalInvest.ViewModels.Fund;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalInvest.Controllers
{
    [Route("calculations")]
    public class CalculationController : Controller
    {
        private readonly CalculationService _calculationService;
        private readonly AppDbContext _context;
        private readonly BreadcrumbService _breadcrumbService;

        public CalculationController(
            CalculationService calculationService,
            AppDbContext context,
            BreadcrumbService breadcrumbService
        )
        {
            _breadcrumbService = breadcrumbService;
            _calculationService = calculationService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? selectedIncomeReportId,
            int? selectedFundId,
            decimal? investmentPercentage,
            int? investmentDay
        )
        {
            var reports = await _context.IncomeReports.Include(r => r.TaxSummaries).ToListAsync();

            var funds = await _context.Funds.ToListAsync();

            var navYears =
                selectedFundId != null
                    ? await _context
                        .NavCourses.Where(n => n.FundId == selectedFundId)
                        .Select(n => n.Date.Year)
                        .Distinct()
                        .ToListAsync()
                    : new List<int>();

            var selectedReport = reports.FirstOrDefault(r => r.Id == selectedIncomeReportId);

            var model = new CalculationViewModel
            {
                IncomeReports = reports
                    .Select(r => new IncomeReportViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Currency = r.Currency,
                        TaxSummaries = r
                            .TaxSummaries.Select(t => new TaxSummaryViewModel
                            {
                                IncomeYear = t.IncomeYear,
                                EmploymentIncome = t.EmploymentIncome,
                                FinalTax = t.FinalTax,
                                IsSelected = true,
                                HasNavCourses = navYears.Contains(t.IncomeYear),
                            })
                            .ToList(),
                    })
                    .ToList(),
                Funds = funds
                    .Select(f => new FundSelectionViewModel { Id = f.Id, Name = f.Name })
                    .ToList(),
                NavYears = navYears,
                SelectedIncomeReportId = selectedIncomeReportId,
                SelectedFundId = selectedFundId,
                InvestmentPercentage = investmentPercentage ?? 10,
                InvestmentDay = investmentDay ?? 25,
            };

            if (selectedIncomeReportId != null && selectedFundId != null)
            {
                var selectedYearsList = model
                    .IncomeReports.Where(r => r.Id == selectedIncomeReportId)
                    .SelectMany(r => r.TaxSummaries)
                    .Where(t => t.IsSelected)
                    .Select(t => t.IncomeYear)
                    .ToList();

                var investmentResults = await _calculationService.CalculateInvestments(
                    selectedIncomeReportId.Value,
                    selectedFundId.Value,
                    model.InvestmentPercentage,
                    selectedYearsList,
                    model.InvestmentDay
                );

                model.InvestmentResults = investmentResults.InvestmentResults;
                model.FireTargetDate = investmentResults.FireTargetDate;
                model.FireTargetValue = investmentResults.FireTargetValue;
            }

            ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(customControllerTitle: "Beräkningar");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInvestmentResults(
            [FromBody] InvestmentUpdateRequest request
        )
        {
            if (request.SelectedIncomeReportId == null || request.SelectedFundId == null)
                return BadRequest("Inkomstrapport och fond måste vara valda.");

            var selectedYears =
                request.SelectedYears?.Select(int.Parse).ToList() ?? new List<int>();

            var updatedResults = await _calculationService.CalculateInvestments(
                request.SelectedIncomeReportId.Value,
                request.SelectedFundId.Value,
                request.InvestmentPercentage,
                selectedYears,
                request.InvestmentDay
            );

            return Json(updatedResults);
        }

        public class InvestmentUpdateRequest
        {
            public int? SelectedIncomeReportId { get; set; }
            public int? SelectedFundId { get; set; }
            public decimal InvestmentPercentage { get; set; }
            public List<string> SelectedYears { get; set; }
            public int InvestmentDay { get; set; }
        }
    }
}
