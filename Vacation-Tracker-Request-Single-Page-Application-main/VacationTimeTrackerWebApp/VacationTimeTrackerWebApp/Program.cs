using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using VacationTimeTrackerWebApp.ClaimsTransformation;
using VacationTimeTrackerWebApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration.
var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.LaunchSettings.json")
           .Build();

var defaultConnection = configuration.GetConnectionString("Default");

// Add services to the container.
var Services = builder.Services;
Services.AddControllers();
Services.AddRazorPages();
Services.AddTransient<IDBContext, DBContext>();
Services.AddTransient<IDBEmployeesRepository, EmployeesRepository>();
Services.AddTransient<IDBRequestsRepository, RequestsRepository>();
Services.AddTransient<IDBEntitlementsRepository, EntitlementsRepository>();
Services.AddTransient<IClaimsTransformation, GoogleClaimsTransformation>();
// Configure our DbContext sub-class.
Services.AddDbContext<DBContext>(options => options.UseMySQL(defaultConnection));
Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.LoginPath = "/user/google-signin";
        options.AccessDeniedPath = "/unauthorised";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.MaxAge = options.ExpireTimeSpan; // optional
        options.SlidingExpiration = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId = configuration["GoogleAuth:ClientId"];
        options.ClientSecret = configuration["GoogleAuth:ClientSecret"];
        options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
        options.SaveTokens = true;
    });
Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthorizedUser", policy =>
    {
        policy.RequireAssertion(context => context.User.HasClaim("Authorized", "True"));
    });
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAssertion(context => context.User.HasClaim("AccessType", "Admin"));
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{   // Production Error page.
    app.UseExceptionHandler("/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Stops the app from using http in foward headers such as a redirect uri.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

// Uses.
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Maps.
app.MapControllers();
app.MapRazorPages();


// Start application.
app.Run();
