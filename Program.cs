using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using foodbyte.Data;
using foodbyte.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using AspNetCoreRateLimit;
using EonaCat.Web.RateLimiter.Endpoints.Extensions;
using NuGet.Protocol.Plugins;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();




builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

var myOptions = new MyRateLimitOptions();
builder.Configuration.GetSection(MyRateLimitOptions.MyRateLimit).Bind(myOptions);
var fixedPolicy = "fixed";


int maxRejectionsBeforeTimeout = 2;
int rejection = 0;


builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "login", options1 =>
    {
        options1.PermitLimit = 4;
        options1.Window = TimeSpan.FromSeconds(12);

    });
    
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsync(
                $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds. " , cancellationToken: token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync(
                "Too many requests. Please try again later. " , cancellationToken: token);
        }
    };
    
    
    
    
});


var app = builder.Build();



//call the database initializer
using (var services = app.Services.CreateScope())
{
    var db = services.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var um = services.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var rm = services.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    ApplicationDbInitializer.Initialize(db,um,rm);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}













app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseRateLimiter();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}");
app.MapRazorPages();



// Group






//static string GetTicks() => (DateTime.Now.Ticks & 0x11111).ToString("00000");

//app.MapGet("/", () => Results.Ok($"Hello {GetTicks()}")).RequireRateLimiting("login");


app.Run();
