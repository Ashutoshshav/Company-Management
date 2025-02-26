using System.Text;
using Digitization.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DinkToPdf;
using DinkToPdf.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Configure JWT Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]); // Secret key from appsettings.json
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Issuer from appsettings.json
        ValidAudience = builder.Configuration["Jwt:Audience"], // Audience from appsettings.json
        IssuerSigningKey = new SymmetricSecurityKey(key),
        // Ensure token expiration is checked
        ClockSkew = TimeSpan.Zero,  // Optional: to immediately expire tokens at expiration
        RequireExpirationTime = true,
        RoleClaimType = "Role",
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for user: {context.Principal.Identity.Name}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // Retrieve the token from cookies
            var token = context.Request.Cookies["Token"];
            Console.WriteLine($"Token received: {token}");
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token; // Set token in the context
                Console.WriteLine($"Token received: {token}");
            }

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Redirect to login page when unauthorized
            if (!context.Response.HasStarted)
            {
                context.Response.Redirect("/Auth/Login");
            }
            context.HandleResponse(); // Prevent default behavior
            return Task.CompletedTask;
        }
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SharedAccess", policy =>
        policy.RequireAuthenticatedUser());
    options.AddPolicy("ManagementAccess", policy =>
        policy.RequireRole("Management", "HR", "CA", "Director"));
    options.AddPolicy("DirectorAccess", policy =>
        policy.RequireRole("Director"));
});

// Register EmailService as a singleton (shared instance)
builder.Services.AddSingleton<EmailService>();

// Register ScheduledEmailService as a background service
builder.Services.AddHostedService<ScheduledEmailService>();

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
