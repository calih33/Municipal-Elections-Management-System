using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
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

// JWT for the mobile app API. Added as an extra scheme so the cookie login stays the
// default for the website; API controllers opt in with
// [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)].
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (jwtSettings.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must be at least 32 characters. Set it in appsettings.Development.json locally " +
        "(see the .example file) or as the Jwt__Key app setting on Azure.");
}
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        // keep claim names as they are in the token ("sub", "email", "name")
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = jwtSettings.SigningKey,
            NameClaimType = JwtRegisteredClaimNames.Name,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

// Admin area: must be in an admin group (Super Admin or a municipality admin group)
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AppRoles.AdminPolicy, policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context => context.User.HasClaim(c => c.Type == ClaimTypes.Role)));

builder.Services.AddScoped<IMunicipalityAccessService, MunicipalityAccessService>();

// voting guide cookie
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VotingGuideService>();

// image uploads
builder.Services.AddSingleton<ImageStorageService>();

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

// serve uploaded images at /uploads
var imageStorage = app.Services.GetRequiredService<ImageStorageService>();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imageStorage.RootPath),
    RequestPath = "/uploads"
});
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
