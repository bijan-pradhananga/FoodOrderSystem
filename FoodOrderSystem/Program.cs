using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodOrderSystem.Data;
using FoodOrderSystem.Areas.Identity.Data;
using FoodOrderSystem.Services;
using FoodOrderSystem.Services.Options;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection") ?? throw new InvalidOperationException("Connection string 'AuthDbContextConnection' not found.");

builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Use custom logic for redirecting to different login pages based on user role or request path.
    options.Events.OnRedirectToLogin = context =>
    {
        // Check if the path requested is in the Admin area
        if (context.Request.Path.StartsWithSegments("/Admin"))
        {
            context.Response.Redirect("/Admin/Login" + "?ReturnUrl=" + context.Request.Path);
        }
        else
        {
            // For regular users, redirect to the default login page
            context.Response.Redirect("/Identity/Account/Login" + "?ReturnUrl=" + context.Request.Path);
        }
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        // Check if the path requested is in the Admin area
        if (context.Request.Path.StartsWithSegments("/Admin"))
        {
            context.Response.Redirect("/Admin/Login");
        }
        else
        {
            // For regular users, redirect to the default access denied page
            context.Response.Redirect("/Identity/Account/Login");
        }
        return Task.CompletedTask;
    };
});



// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// Register EsewaPaymentService with HttpClient
builder.Services.AddHttpClient<EsewaPaymentService>();
builder.Services.Configure<EsewaPaymentServiceOptions>(builder.Configuration.GetSection("eSewa"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();    // Ensure authentication middleware is added
app.UseAuthorization();     // Ensure authorization middleware is added

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "admin",
    pattern: "{controller=Admin}/{action=Login}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await SeedData.Initialize(services, userManager);
}


app.Run();
