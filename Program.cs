using System.Globalization;
using GlobalInvest.Data;
using GlobalInvest.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Lägg till tjänster
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider(); // Lägg till sessionstöd för TempData
builder.Services.AddSession(); // Lägg till sessioner

builder.Services.AddHttpContextAccessor(); // 🟢 LÄGG TILL DENNA
builder.Services.AddSingleton<BreadcrumbService>(); // 🟢 ÄNDRAT TILL SINGLETON
builder.Services.AddScoped<ModalService>();
builder.Services.AddScoped<TaxSummaryImportService>();
builder.Services.AddScoped<CalculationService>();

// 🟢 Fix: Lägg till IUrlHelperFactory om ModalService använder URL-generering
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"))
);

// Ställ in kulturen till svenska
var cultureInfo = new CultureInfo("sv-SE");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();
app.UseWebSockets();

// Middleware-konfiguration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Utility/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Lägg till sessionstöd i pipeline
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
