using Microsoft.AspNetCore.Authentication.Cookies;
using SV21T1020659.Web.Appcodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLogging();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.Cookie.Name = "AuthenticationCookie";   // Tên của cookie
                    option.LoginPath = "/Account/Login";           // URL đến trang đăng nhập
                    option.AccessDeniedPath = "/Account/AccessDenined"; // URL đến trang trong trường bị cấm
                    option.ExpireTimeSpan = TimeSpan.FromDays(360);  // Thời gian hiệu lực
                });
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(60);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});




var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

ApplicationContext.Configure
(
    context: app.Services.GetRequiredService<IHttpContextAccessor>(),
    enviroment: app.Services.GetRequiredService<IWebHostEnvironment>()
);
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB");
SV21T1020629.BusinessLayers.Configuration.Initialize(connectionString);

app.Run();
