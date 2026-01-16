// using Microsoft.EntityFrameworkCore;
// using SIOMS.Data;
// using Microsoft.AspNetCore.Session;

// var builder = WebApplication.CreateBuilder(args);

// // Add services
// builder.Services.AddControllersWithViews();
// builder.Services.AddRazorPages();

// // Add DbContext
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // Add Session with proper configuration
// builder.Services.AddSession(options =>
// {
//     options.IdleTimeout = TimeSpan.FromMinutes(30);
//     options.Cookie.HttpOnly = true;
//     options.Cookie.IsEssential = true;
//     options.Cookie.Name = "SIOMS.Session";
// });

// // Add Area support
// builder.Services.AddRazorPages(options =>
// {
//     options.Conventions.AddAreaPageRoute("Admin", "/Admin/Index", "Admin");
// });

// var app = builder.Build();

// // Configure pipeline
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
// app.UseStaticFiles();

// app.UseRouting();
// app.UseSession(); // Session MUST come before authorization
// app.UseAuthorization();

// // Area route configuration
// app.MapAreaControllerRoute(
//     name: "Admin",
//     areaName: "Admin",
//     pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

// // Default route for non-admin pages
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");


// app.Run();


using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ ADD THIS - Fix authentication error
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.Name = "SIOMS.Auth";
    });

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "SIOMS.Session";
});

// Add Area support
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddAreaPageRoute("Admin", "/Admin/Index", "Admin");
});

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ ADD THESE - Order matters!
app.UseAuthentication();  // First
app.UseAuthorization();   // Second

app.UseSession();

// ✅ FIXED ROUTING
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();