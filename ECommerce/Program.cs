using ECommerce.Components;
using ECommerce.DBContext;
using ECommerce.Services;
using ECommerce.Shared;
using ECommerce.Shared.Mapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Radzen;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Services ----------------

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();               // full authorization services, not just AddAuthorizationCore
builder.Services.AddCascadingAuthenticationState(); // required for [Authorize] / AuthorizeRouteView in Blazor Web Apps

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStyleService, StyleService>();
builder.Services.AddScoped<IFileService, FileUploadService>();
builder.Services.AddHttpContextAccessor();

//builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly); // was commented out — ProductService needs IMapper

var app = builder.Build();

// ---------------- Middleware pipeline ----------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Static files early, before auth/antiforgery — these don't need either.
app.MapStaticAssets();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"D:\Images"),
    RequestPath = "/Images"
});

app.UseAntiforgery();

// MUST come after routing/static files and before MapRazorComponents,
// or cookies never get read/written and [Authorize] silently does nothing.
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ---------------- Login / logout endpoints ----------------
// Plain HTTP endpoints, not Blazor components — this is what makes SignInAsync
// actually work (a real HTTP response, not a call from inside a SignalR circuit).

const string AdminUser = "admin";

// TODO: move to appsettings/user-secrets, never leave in source. Generate once via:
//   var salt = Guid.NewGuid().ToString();
//   var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("YourRealPassword" + salt)));
//const string AdminPasswordHash = "REPLACE_WITH_REAL_HASH";
const string PasswordSalt = "Admin@123";

//static bool VerifyPassword(string input, string salt, string expectedHash)
//{
//    var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input + salt)));
//    return CryptographicOperations.FixedTimeEquals(
//        Encoding.UTF8.GetBytes(hash),
//        Encoding.UTF8.GetBytes(expectedHash));
//}

app.MapPost("/Account/Login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/'))
        returnUrl = "/admin/products";

    if (username == AdminUser && password == PasswordSalt)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        return Results.Redirect(returnUrl);
    }

    return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}");
});

app.MapPost("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.Run();