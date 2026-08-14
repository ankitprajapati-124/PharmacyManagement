using Microsoft.AspNetCore.Authentication.Cookies;
using PharmacyManagement.Repositories;
using PharmacyManagement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();

builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();

builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();

builder.Services.AddScoped<IDashboardRepository,DashboardRepository>();

builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IUserService,UserService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuditLogRepository,AuditLogRepository>();
builder.Services.AddScoped<IAuditLogService,AuditLogService>();

builder.Services.AddScoped<IDashboardService,DashboardService>();
builder.Services.AddScoped<IAuthService,AuthService>();

builder.Services.AddScoped<IReportRepository,ReportRepository>();
builder.Services.AddScoped<IReportService,ReportService>();

builder.Services.AddScoped<IMedicineService, MedicineService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
