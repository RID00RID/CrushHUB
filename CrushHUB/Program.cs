using CrushHUB.Domain;
using CrushHUB.Domain.Entities;
using CrushHUB.Domain.Repositoryes.Abstract;
using CrushHUB.Domain.Repositoryes.EntityFramework;
using CrushHUB.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

IConfigurationBuilder configBuild = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

IConfiguration configuration = configBuild.Build();
AppConfig appConfig = configuration.GetSection("Project").Get<AppConfig>()!;

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(appConfig.Database.ConnectionString)
    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ScreenshotStorage>();
builder.Services.AddScoped<GameUserRegistry>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<DiscordNotifier>();

builder.Services.AddSingleton(appConfig);

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));


var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

app.Run();