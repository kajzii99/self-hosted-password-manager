using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SelfHostedPasswordManager.Data;
using SelfHostedPasswordManager.Encryption;
using SelfHostedPasswordManager.Services;
using AspNetCore.Unobtrusive.Ajax;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<PasswordGeneratorService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddControllersWithViews();
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opt =>
{
    opt.Lockout.AllowedForNewUsers = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
    opt.Lockout.MaxFailedAccessAttempts = 3;
}).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddUnobtrusiveAjax();
    //services.AddUnobtrusiveAjax(useCdn: true, injectScriptIfNeeded: false);
    //...
}

void Configure(IApplicationBuilder app)
{
    //...
    app.UseStaticFiles();

    //It is required for serving 'jquery-unobtrusive-ajax.min.js' embedded script file.
    app.UseUnobtrusiveAjax(); //It is suggested to place it after UseStaticFiles()
    //...
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

/*app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");*/

//app.MapRazorPages();

app.Run();
