using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Services;

var builder = WebApplication.CreateBuilder(args);

// Locally this comes from appsettings.Development.json (git-ignored, copy the .example file);
// on Azure it comes from the App Service connection string setting.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. Copy appsettings.Development.example.json to appsettings.Development.json.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IMunicipalityAccessService, MunicipalityAccessService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

await SeedData.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
