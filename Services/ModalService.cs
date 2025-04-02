using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GlobalInvest.Services
{
    public class ModalService
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModalService(
            IUrlHelperFactory urlHelperFactory,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetClearNavCoursesConfirmation(Controller controller, string fundName, int id)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(controller.ControllerContext);
            controller.TempData["ModalTitle"] = "Bekräfta rensning av NAV-kurser";
            controller.TempData["ModalBody"] =
                $"Är du säker på att du vill rensa alla NAV-kurser för {fundName}?";
            controller.TempData["PrimaryButtonText"] = "Ja, rensa";
            controller.TempData["PrimaryButtonColor"] = "btn-danger";
            controller.TempData["ModalActionUrl"] = urlHelper.Action(
                "ClearNavCourses",
                "Fund",
                new { id }
            );

            // 🔹 Återgå till Details
            controller.TempData["ReturnUrl"] = urlHelper.Action("Details", "Fund", new { id });
            controller.TempData["SecondaryButtonText"] = "Avbryt";
        }

        public void SetDeleteConfirmation(
            Controller controller,
            string entityName,
            int id,
            string deleteAction,
            string controllerName,
            string returnAction,
            string cancelAction
        )
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(controller.ControllerContext);

            controller.TempData["ModalTitle"] = "Bekräfta borttagning";
            controller.TempData["ModalBody"] = $"Är du säker på att du vill ta bort {entityName}?";
            controller.TempData["PrimaryButtonText"] = "Ja, ta bort";
            controller.TempData["PrimaryButtonColor"] = "btn-danger";

            // 🟢 URL för att radera entiteten
            controller.TempData["ModalActionUrl"] = urlHelper.Action(
                deleteAction,
                controllerName,
                new { id }
            );

            // 🟢 URL för att gå tillbaka vid "Avbryt"
            controller.TempData["ReturnUrl"] = urlHelper.Action(
                cancelAction,
                controllerName,
                cancelAction == "Details" ? new { id } : null
            );

            // 🟢 URL för att gå tillbaka efter lyckad radering
            controller.TempData["SuccessReturnUrl"] = urlHelper.Action(
                returnAction,
                controllerName,
                returnAction == "Details" ? new { id } : null
            );

            controller.TempData["SecondaryButtonText"] = "Avbryt";
        }

        public void SetClearTaxSummariesConfirmation(
            Controller controller,
            string reportName,
            int id
        )
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(controller.ControllerContext);
            controller.TempData["ModalTitle"] = "Bekräfta rensning av taxeringsuppgifter";
            controller.TempData["ModalBody"] =
                $"Är du säker på att du vill rensa alla taxeringsuppgifter för {reportName}?";
            controller.TempData["PrimaryButtonText"] = "Ja, rensa";
            controller.TempData["PrimaryButtonColor"] = "btn-danger";
            controller.TempData["ModalActionUrl"] = urlHelper.Action(
                "ClearTaxSummaries",
                "IncomeReport",
                new { id }
            );

            // 🔹 Återgå till Details
            controller.TempData["ReturnUrl"] = urlHelper.Action(
                "Details",
                "IncomeReport",
                new { id }
            );
            controller.TempData["SecondaryButtonText"] = "Avbryt";
        }
    }
}
