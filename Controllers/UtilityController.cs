using GlobalInvest.Services;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInvest.Controllers
{
    [Route("utility")]
    public class UtilityController : Controller
    {
        private readonly BreadcrumbService _breadcrumbService;

        public UtilityController(BreadcrumbService breadcrumbService)
        {
            _breadcrumbService = breadcrumbService;
        }

        // 🟢 Returnera partial view för statusmeddelanden (GET /utility/status-message)
        [HttpGet("status-message")]
        public IActionResult GetStatusMessage()
        {
            return PartialView("~/Views/Shared/_StatusMessages.cshtml");
        }

        // 🟢 Sätt statusmeddelanden (POST /utility/status-message)
        [HttpPost("status-message")]
        public IActionResult SetStatusMessage(string type, string message)
        {
            if (string.IsNullOrEmpty(message))
                return BadRequest("Meddelandet får inte vara tomt.");

            switch (type.ToLower())
            {
                case "success":
                    TempData["SuccessMessage"] = message;
                    break;
                case "warning":
                    TempData["WarningMessage"] = message;
                    break;
                case "error":
                    TempData["ErrorMessage"] = message;
                    break;
                default:
                    TempData["Message"] = message;
                    break;
            }

            return Ok(
                new
                {
                    status = "Message set",
                    type,
                    message,
                }
            );
        }

        [HttpGet("breadcrumbs/update")]
        public IActionResult UpdateBreadcrumbs(
            [FromQuery] int id,
            [FromQuery] string customActionTitle,
            [FromQuery] string controller,
            [FromQuery] string customControllerTitle
        )
        {
            ViewData["Breadcrumbs"] = _breadcrumbService.GetBreadcrumbs(
                id: id.ToString(),
                customControllerTitle: customControllerTitle,
                customActionTitle: customActionTitle,
                overrideController: controller
            );

            return PartialView("~/Views/Shared/_Breadcrumbs.cshtml");
        }

        [HttpGet("download-sqlite")]
        public IActionResult DownloadSqliteDb()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "App.db");
            var contentType = "application/octet-stream";
            var fileName = "GlobalInvest.sqlite";

            if (!System.IO.File.Exists(filePath))
                return NotFound("Databasfilen hittades inte.");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, fileName);
        }

        [HttpGet("error")]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
