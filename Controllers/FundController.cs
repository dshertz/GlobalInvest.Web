using System.Globalization;
using GlobalInvest.Data;
using GlobalInvest.Helpers;
using GlobalInvest.Models;
using GlobalInvest.Services;
using GlobalInvest.ViewModels;
using GlobalInvest.ViewModels.Fund;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("funds")]
public class FundController : Controller
{
    private readonly AppDbContext _context;
    private readonly BreadcrumbService _breadcrumbService;
    private readonly ModalService _modalService;

    public FundController(
        AppDbContext context,
        BreadcrumbService breadcrumbService,
        ModalService modalService
    )
    {
        _modalService = modalService;
        _breadcrumbService = breadcrumbService;
        _context = context;
    }

    // 🟢 LISTA ALLA FONDER (GET /funds)
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var funds = await _context
            .Funds.Include(f => f.NavCourses)
            .Select(f => new FundIndexViewModel
            {
                Id = f.Id,
                Name = f.Name,
                Currency = f.Currency,
                NavSummary = new NavSummaryViewModel
                {
                    StartDate = f.NavCourses.Any()
                        ? f.NavCourses.Min(n => (DateTime?)n.Date)
                        : null,
                    EndDate = f.NavCourses.Any() ? f.NavCourses.Max(n => (DateTime?)n.Date) : null,
                    TotalCourses = f.NavCourses.Count,
                },
            })
            .ToListAsync();

        ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
            customControllerTitle: "Fonder"
        );

        return View(funds);
    }

    // 🔍 HÄMTA EN FOND (GET /funds/{id})
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id, int? pageNumber)
    {
        int pageSize = 50;

        var fund = await _context.Funds.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

        if (fund == null)
            return NotFound();

        // 🟢 Hämta sammanfattning med **ett** anrop istället för 4
        var navSummaryData = await _context
            .NavCourses.Where(n => n.FundId == id)
            .GroupBy(n => 1) // Skapar en enda rad med alla värden
            .Select(g => new
            {
                StartDate = g.Min(n => (DateTime?)n.Date),
                EndDate = g.Max(n => (DateTime?)n.Date),
                TotalCourses = g.Count(),
            })
            .FirstOrDefaultAsync();

        var navSummary = new NavSummaryViewModel
        {
            StartDate = navSummaryData?.StartDate,
            EndDate = navSummaryData?.EndDate,
            TotalCourses = navSummaryData?.TotalCourses ?? 0,
        };

        // 🟢 Paginera NAV-kurser (utan `.Include(n => n.Fund)`)
        var navCoursesQuery = _context
            .NavCourses.Where(n => n.FundId == id)
            .OrderBy(n => n.Date)
            .Select(n => new NavCourseViewModel
            {
                Id = n.Id,
                // NavValue = n.NavValue,
                Value = n.Value?.ToString("N2") ?? "-",
                Date = n.Date.ToString("yyyy-MM-dd"),
                FundName = fund.Name, // ✅ Ingen extra databasfråga behövs
                Currency = fund.Currency, // ✅ Ingen extra databasfråga behövs
            })
            .AsNoTracking();

        var paginatedNavCourses = await PaginatedList<NavCourseViewModel>.CreateAsync(
            navCoursesQuery,
            pageNumber ?? 1,
            pageSize
        );

        // 🟢 Hämta unika år i **ett** SQL-anrop
        var availableYears = await _context
            .NavCourses.Where(n => n.FundId == id)
            .Select(n => n.Date.Year)
            .Distinct()
            .OrderBy(y => y)
            .ToListAsync();

        var viewModel = new FundDetailsViewModel
        {
            Id = fund.Id,
            Name = fund.Name,
            Currency = fund.Currency,
            NavSummary = navSummary,
            AvailableYears = availableYears,
            NavCourses = paginatedNavCourses,
        };

        ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
            customActionTitle: fund.Name,
            customControllerTitle: "Fonder"
        );

        return View(viewModel);
    }

    // 🟢 VISA FORMULÄRET FÖR ATT SKAPA EN FOND (GET /funds/create)
    [HttpGet("create")]
    public IActionResult Create()
    {
        ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
            customControllerTitle: "Fonder",
            customActionTitle: "Lägg till ny fond"
        );
        return View();
    }

    // 🔵 SKAPA EN NY FOND (POST /funds)
    [HttpPost("create")]
    public async Task<IActionResult> Create(Fund fund)
    {
        if (ModelState.IsValid)
        {
            _context.Funds.Add(fund);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Fonden har skapats!";
            return RedirectToAction("Details", new { id = fund.Id });
        }

        TempData["ErrorMessage"] = "Misslyckades med att skapa fonden.";
        return View(fund);
    }

    // 🟡 UPPDATERA EN FOND (PUT /funds/{id})
    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(int id, [FromBody] FundEditViewModel model)
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

        var fund = await _context.Funds.FindAsync(id);
        if (fund == null)
        {
            return NotFound(new { success = false, message = "Fonden hittades inte." });
        }

        fund.Name = model.Name;
        fund.Currency = model.Currency;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Fondinformationen har uppdaterats!";

        return Ok(
            new
            {
                success = true,
                name = fund.Name,
                currency = fund.Currency,
            }
        );
    }

    // 🔴 RADERA EN FOND (DELETE /funds/{id})
    [HttpPost("{id}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var fund = await _context.Funds.FindAsync(id);
        if (fund == null)
            return NotFound();

        _context.Funds.Remove(fund);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Fonden har tagits bort.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/confirm-delete")]
    public IActionResult ConfirmDelete(int id, string returnAction, string cancelAction)
    {
        var fund = _context.Funds.Find(id);
        if (fund == null)
            return NotFound();

        _modalService.SetDeleteConfirmation(
            this,
            $"fonden {fund.Name}",
            id,
            "Delete", // Action som utför raderingen
            "Fund", // Controller där raderingen sker
            returnAction, // Action att återgå till efter radering
            cancelAction // Action att återgå till om användaren klickar "Avbryt"
        );

        return RedirectToAction(cancelAction, new { id });
    }

    // 🟢 GET: /funds/{id}/navcourses/import
    [HttpGet("{id}/navcourses/import")]
    public async Task<IActionResult> ImportNavCourses(int id)
    {
        var fund = await _context.Funds.FindAsync(id);
        if (fund == null)
            return NotFound("Fonden hittades inte.");

        // 🔹 Logga för att se vad som finns i TempData vid GET
        Console.WriteLine($"🟢 GET ImportNavCourses för fond {id}");
        Console.WriteLine($"TempData ErrorMessage vid GET: {TempData["ErrorMessage"]}");

        // 🔹 Rensa endast om GET inte kommer från en tidigare redirect (förhindra felmeddelande vid navigering)
        if (!HttpContext.Request.Headers["Referer"].ToString().Contains("import"))
        {
            TempData.Remove("ErrorMessage");
        }

        ViewData["Fund"] = fund;
        ViewData["FundId"] = id;
        ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
            customControllerTitle: "Fonder",
            customActionTitle: "Importera NAV-kurser",
            itemName: fund.Name,
            id: fund.Id.ToString()
        );
        return View("Import");
    }

    // 🟢 POST: /funds/{id}/navcourses/import
    [HttpPost("{id}/navcourses/import")]
    public async Task<IActionResult> ImportNavCourses(int id, [FromForm] string navText)
    {
        if (string.IsNullOrWhiteSpace(navText))
        {
            TempData["ErrorMessage"] = "Textrutan kan inte vara tom.";
            return RedirectToAction("ImportNavCourses", new { id });
        }

        var fund = await _context
            .Funds.Include(f => f.NavCourses)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (fund == null)
            return NotFound("Fonden hittades inte.");

        var lines = navText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var errors = new List<string>();
        var importedCourses = new List<NavCourse>();

        foreach (var line in lines)
        {
            var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
            {
                errors.Add($"⚠️ Ogiltig rad: {line}");
                continue;
            }

            if (
                !decimal.TryParse(
                    parts[1].Trim().Replace(",", "."),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var navValue
                ) || !DateTime.TryParse(parts[3], out var date)
            )
            {
                errors.Add($"⚠️ Felaktigt format på rad: {line}");
                continue;
            }

            bool exists = _context.NavCourses.Any(n =>
                n.FundId == id && n.Date == date && n.NavValue == navValue
            );
            if (!exists)
            {
                var navCourse = new NavCourse
                {
                    FundId = id,
                    Date = date,
                    NavValue = navValue,
                };
                importedCourses.Add(navCourse);
                _context.NavCourses.Add(navCourse);
            }
            else
            {
                errors.Add($"⚠️ Dubblett hittad: {line}");
            }
        }

        if (importedCourses.Any())
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] =
                $"{importedCourses.Count} NAV-kurser importerades framgångsrikt!";
            return RedirectToAction("Details", new { id });
        }

        // 🔹 Om inga kurser importerades, sätt felmeddelandet i TempData och skicka tillbaka användaren
        if (errors.Any())
        {
            TempData["ErrorMessage"] = string.Join("<br>", errors);
        }

        return RedirectToAction("ImportNavCourses", new { id });
    }

    // 🛑 RADERA ALLA NAV-KURSER FÖR EN FOND (POST /funds/{id}/navcourses/clear)
    [HttpPost("{id}/navcourses/clear")]
    public async Task<IActionResult> ClearNavCourses(int id)
    {
        var navCourses = _context.NavCourses.Where(n => n.FundId == id);

        if (!await navCourses.AnyAsync())
        {
            TempData["WarningMessage"] = "Inga NAV-kurser hittades att radera.";
            return RedirectToAction("Details", new { id });
        }

        int deletedCount = await navCourses.CountAsync();
        _context.NavCourses.RemoveRange(navCourses);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{deletedCount} NAV-kurser har raderats.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost("{id}/navcourses/confirm-clear")]
    public async Task<IActionResult> ConfirmClearNavCourses(
        int id,
        [FromServices] ModalService modalService
    )
    {
        var hasNavCourses = await _context.NavCourses.AnyAsync(n => n.FundId == id);

        if (!hasNavCourses)
        {
            TempData["ErrorMessage"] = "Det finns inga NAV-kurser att rensa.";
            return RedirectToAction("Details", new { id });
        }

        var fund = await _context.Funds.FindAsync(id);
        if (fund == null)
            return NotFound();

        modalService.SetClearNavCoursesConfirmation(this, fund.Name, id);
        return RedirectToAction("Details", new { id });
    }

    // 🟢 HÄMTA PAGINERADE NAV-KURSER (GET /funds/{id}/navcourses)
    [HttpGet("{id}/navcourses")]
    public async Task<IActionResult> GetNavCourses(
        int id,
        int pageNumber = 1,
        int pageSize = 50,
        string years = ""
    )
    {
        var selectedYears = string.IsNullOrEmpty(years)
            ? new List<int>()
            : years.Split(',').Select(int.Parse).ToList();

        var query = _context
            .NavCourses.Include(nav => nav.Fund)
            .Where(nav =>
                nav.FundId == id && (!selectedYears.Any() || selectedYears.Contains(nav.Date.Year))
            )
            .OrderBy(nav => nav.Date);

        var totalCourses = await query.CountAsync();

        var paginatedCourses = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(nav => new
            {
                FundName = nav.Fund.Name ?? "-",
                Currency = nav.Fund.Currency ?? "-",
                Date = nav.Date.ToString("yyyy-MM-dd"),
                NavValue = nav.NavValue.ToString("N2"),
            })
            .ToListAsync();

        var response = new { TotalCourses = totalCourses, Data = paginatedCourses };

        return Json(response);
    }
}
