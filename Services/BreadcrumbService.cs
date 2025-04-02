namespace GlobalInvest.Services
{
    public class BreadcrumbItem
    {
        public string Title { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public RouteValueDictionary RouteValues { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class BreadcrumbService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BreadcrumbService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<BreadcrumbItem> GetBreadcrumbs(
            string? itemName = null,
            string? id = null,
            string? customControllerTitle = null,
            string? overrideController = null, // 🔥 Lägg till en alternativ controller
            string? customActionTitle = null,
            bool includeController = true,
            bool includeCurrentAction = true
        )
        {
            var routeData = _httpContextAccessor.HttpContext?.GetRouteData();
            var controller = !string.IsNullOrEmpty(overrideController)
                ? overrideController.ToLower() // ✅ Använd overrideController om den finns
                : routeData?.Values["controller"]?.ToString()?.ToLower(); // Annars hämta från requesten

            var action = routeData?.Values["action"]?.ToString()?.ToLower();
            var entityId = id ?? routeData?.Values["id"]?.ToString();

            var breadcrumbs = new List<BreadcrumbItem>();

            // 🟢 Lägg till första nivån (controller-index) endast om includeController är true
            if (!string.IsNullOrEmpty(controller) && includeController)
            {
                breadcrumbs.Add(
                    new BreadcrumbItem
                    {
                        Title = customControllerTitle ?? controller, // ✅ Använd custom title om det finns, annars controller-namnet
                        Controller = controller,
                        Action = "Index", // 🔥 Standardrouten för listan
                    }
                );
            }

            // 🟢 Hantera specifika poster (Details, Import etc.
            if (!string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(itemName))
            {
                breadcrumbs.Add(
                    new BreadcrumbItem
                    {
                        Title = itemName,
                        Controller = controller,
                        Action = "Details",
                        RouteValues = new RouteValueDictionary { { "id", entityId } },
                        IsActive = false,
                    }
                );
            }

            // 🟢 Lägg till nuvarande action om det behövs
            if (includeCurrentAction && !string.IsNullOrEmpty(action) && action != "index")
            {
                breadcrumbs.Add(
                    new BreadcrumbItem
                    {
                        Title = customActionTitle ?? action, // ✅ Använd custom title om det finns
                        Controller = controller,
                        Action = action,
                        RouteValues =
                            entityId != null
                                ? new RouteValueDictionary { { "id", entityId } }
                                : null,
                        IsActive = true,
                    }
                );
            }
            return breadcrumbs;
        }
    }
}
