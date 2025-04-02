using GlobalInvest.Data;
using GlobalInvest.Models;
using GlobalInvest.Services;
using GlobalInvest.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("income-reports")] // 🔹 Plural enligt REST-konventionen
public class IncomeReportController : Controller
{
    private readonly AppDbContext _context;
    private readonly BreadcrumbService _breadcrumbService;
    private readonly ModalService _modalService;
    private readonly TaxSummaryImportService _taxSummaryImportService;

    public IncomeReportController(
        AppDbContext context,
        BreadcrumbService breadcrumbService,
        ModalService modalService,
        TaxSummaryImportService taxSummaryImportService
    )
    {
        _taxSummaryImportService = taxSummaryImportService;
        _context = context;
        _breadcrumbService = breadcrumbService;
        _modalService = modalService;
    }

    // 🟢 LISTA ALLA INKOMSTUPPGIFTER (GET /income-reports)
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var reports = await _context
            .IncomeReports.Include(r => r.TaxSummaries)
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
                    })
                    .ToList(),
            })
            .ToListAsync();

        ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
            customControllerTitle: "Inkomster"
        );
        return View(reports);
    }

    // 🔍 HÄMTA EN INKOMSTUPPGIFT (GET /income-reports/{id})
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var report = await _context
            .IncomeReports.Include(ir => ir.TaxSummaries)
            .FirstOrDefaultAsync(ir => ir.Id == id);

        if (report == null)
            return NotFound();

        var reportViewModel = new IncomeReportViewModel
        {
            Id = report.Id,
            Name = report.Name,
            Currency = report.Currency,
            TaxSummaries = report
                .TaxSummaries.Select(t => new TaxSummaryViewModel
                {
                    Id = t.Id,
                    IncomeYear = t.IncomeYear,
                    EmploymentIncome = t.EmploymentIncome,
                    FinalTax = t.FinalTax,
                    IsSelected = false,
                })
                .ToList(),
        };

        ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
            customControllerTitle: "Inkomster",
            customActionTitle: report.Name
        );
        return View(reportViewModel);
    }

    // 🟢 GET: /income-reports/create
    [HttpGet("create")]
    public IActionResult Create()
    {
        ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
            customControllerTitle: "Inkomster",
            customActionTitle: "Lägg till ny rapport"
        );

        var model = new IncomeReportViewModel();
        return View(model);
    }

    // 🟢 POST: /income-reports/create
    [HttpPost("create")]
    public async Task<IActionResult> Create(IncomeReportViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Vänligen fyll i alla fält korrekt.";
            return View(model);
        }

        var report = new IncomeReport { Name = model.Name, Currency = model.Currency };

        _context.IncomeReports.Add(report);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Inkomstuppgiften har skapats!";
        return RedirectToAction("Details", new { id = report.Id });
    }

    // 🟡 UPPDATERA EN INKOMSTRAPPORT (PUT /income-reports/{id})
    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(int id, [FromBody] IncomeReportViewModel model)
    {
        if (
            model == null
            || string.IsNullOrWhiteSpace(model.Name)
            || string.IsNullOrWhiteSpace(model.Currency)
        )
        {
            return BadRequest(
                new { success = false, message = "Vänligen fyll i alla fält korrekt." }
            );
        }

        var report = await _context.IncomeReports.FindAsync(id);
        if (report == null)
        {
            return NotFound(new { success = false, message = "Inkomstuppgiften hittades inte." });
        }

        report.Name = model.Name;
        report.Currency = model.Currency;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Inkomstuppgiften har uppdaterats!";

        return Ok(
            new
            {
                success = true,
                name = report.Name,
                currency = report.Currency,
            }
        );
    }

    // 🟢 GET: /income-reports/{id}/taxsummary/import
    [HttpGet("{id}/taxsummaries/import")]
    public async Task<IActionResult> ImportTaxSummaries(int id)
    {
        var report = await _context.IncomeReports.FindAsync(id);
        if (report == null)
            return NotFound("Inkomstuppgiften hittades inte.");

        ViewData["IncomeReport"] = report;
        ViewData["IncomeReportId"] = id;
        ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
            itemName: report.Name,
            id: report.Id.ToString(),
            customActionTitle: "Importera Taxeringsuppgifter",
            customControllerTitle: "Inkomster"
        );

        return View("Import");
    }

    // 🟢 POST: /income-reports/{id}/taxsummary/import
    [HttpPost("{id}/taxsummaries/import")]
    public async Task<IActionResult> ImportTaxSummaries(int id, string taxText)
    {
        if (string.IsNullOrWhiteSpace(taxText))
        {
            TempData["ErrorMessage"] = "Ingen data att importera!";
            return RedirectToAction("Details", new { id });
        }

        var models = _taxSummaryImportService.ParseTaxText(taxText);

        if (!models.Any())
        {
            TempData["ErrorMessage"] =
                "Felaktigt format i inmatningen. Kontrollera att texten är korrekt formaterad.";
            return RedirectToAction("Details", new { id });
        }

        var report = await _context
            .IncomeReports.Include(r => r.TaxSummaries)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
        {
            return NotFound("Inkomstuppgiften hittades inte.");
        }

        var importedSummaries = models
            .Select(m => new TaxSummary
            {
                IncomeReportId = id,
                IncomeYear = m.IncomeYear,
                EmploymentIncome = m.EmploymentIncome,
                FinalTax = m.FinalTax,
            })
            .ToList();

        _context.TaxSummaries.AddRange(importedSummaries);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Importerat {importedSummaries.Count} inkomstuppgifter!";
        return RedirectToAction("Details", new { id });
    }

    // 🔴 RADERA EN INKOMSTUPPGIFT (POST /income-reports/{id}/delete)
    [HttpPost("{id}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await _context.IncomeReports.FindAsync(id);
        if (report == null)
            return NotFound();

        _context.IncomeReports.Remove(report);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Inkomstuppgiften har raderats!";
        return RedirectToAction("Index");
    }

    // 🛑 BEKRÄFTA RADERING
    [HttpPost("{id}/confirm-delete")]
    public IActionResult ConfirmDelete(int id, string returnAction, string cancelAction)
    {
        var report = _context.IncomeReports.Find(id);
        if (report == null)
            return NotFound();

        _modalService.SetDeleteConfirmation(
            this,
            $"inkomstuppgiften {report.Name}",
            id,
            "Delete", // Action som utför raderingen
            "IncomeReport", // Controller där raderingen sker
            returnAction, // Action att återgå till efter radering
            cancelAction // Action att återgå till om användaren klickar "Avbryt"
        );

        return RedirectToAction(cancelAction, new { id });
    }

    // 🛑 RADERA ALLA TAXERINGSUPPGIFTER FÖR EN INKOMSTRAPPORT (POST /income-reports/{id}/taxsummaries/clear)
    [HttpPost("{id}/taxsummaries/clear")]
    public async Task<IActionResult> ClearTaxSummaries(int id)
    {
        var taxSummaries = _context.TaxSummaries.Where(t => t.IncomeReportId == id);

        if (!await taxSummaries.AnyAsync())
        {
            TempData["WarningMessage"] = "Inga taxeringsuppgifter hittades att radera.";
            return RedirectToAction("Details", new { id });
        }

        int deletedCount = await taxSummaries.CountAsync();
        _context.TaxSummaries.RemoveRange(taxSummaries);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{deletedCount} taxeringsuppgifter har raderats.";
        return RedirectToAction("Details", new { id });
    }

    // 🟡 BEKRÄFTA RADERING AV ALLA TAXERINGSUPPGIFTER (POST /income-reports/{id}/taxsummaries/confirm-clear)
    [HttpPost("{id}/taxsummaries/confirm-clear")]
    public async Task<IActionResult> ConfirmClearTaxSummaries(
        int id,
        [FromServices] ModalService modalService
    )
    {
        var hasTaxSummaries = await _context.TaxSummaries.AnyAsync(t => t.IncomeReportId == id);

        if (!hasTaxSummaries)
        {
            TempData["ErrorMessage"] = "Det finns inga taxeringsuppgifter att rensa.";
            return RedirectToAction("Details", new { id });
        }

        var report = await _context.IncomeReports.FindAsync(id);
        if (report == null)
            return NotFound();

        modalService.SetClearTaxSummariesConfirmation(this, report.Name, id);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost("{id}/taxsummaries/update")]
    public async Task<IActionResult> UpdateTaxSummaries(
        int id,
        [FromBody] List<TaxSummaryViewModel> models
    )
    {
        var report = await _context
            .IncomeReports.Include(r => r.TaxSummaries)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
        {
            return NotFound(new { success = false, message = "Inkomstuppgiften hittades inte." });
        }

        foreach (var model in models)
        {
            var taxSummary = report.TaxSummaries.FirstOrDefault(t => t.Id == model.Id);
            if (taxSummary != null)
            {
                taxSummary.IncomeYear = model.IncomeYear;
                taxSummary.EmploymentIncome = model.EmploymentIncome;
                taxSummary.FinalTax = model.FinalTax;
            }
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Taxeringsuppgifter har uppdaterats!";
        return Ok(new { success = true });
    }
}
