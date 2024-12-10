using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using moqaren.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container with JSON cycle handling
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure database context with SQL Server and retry policy
builder.Services.AddDbContext<MoqarenContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }
    ));

// Configure CORS policy for local development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins",
        builder =>
        {
            builder.WithOrigins("https://localhost:7212", "http://localhost:5212")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configure session with security settings
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);    // Set session timeout to 30 minutes
    options.Cookie.HttpOnly = true;                    // Prevent client-side access to the cookie
    options.Cookie.IsEssential = true;                 // Mark as essential for GDPR compliance
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Require HTTPS
});

// Add caching services
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

// Configure logging providers
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add environment-specific logging
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}
else
{
    builder.Logging.AddEventLog();
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Enable HTTP Strict Transport Security
}

// Enable CORS
app.UseCors("AllowedOrigins");

// Require HTTPS
app.UseHttpsRedirection();

// Configure static file caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24; // 24 hours
        ctx.Context.Response.Headers["Cache-Control"] =
            "public,max-age=" + durationInSeconds;
    }
});

// Configure middleware pipeline
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();  // Single session middleware placement
app.UseResponseCaching();

// Security headers middleware
app.Use(async (context, next) =>
{
    // Add security headers
    context.Response.Headers["X-Frame-Options"] = "DENY";  // Prevent clickjacking
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";  // Prevent MIME type sniffing
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";  // Enable XSS filtering

    // Request logging
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation(
        "Request {method} {url} => {statusCode}",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode);

    await next();
});

// Configure routes




app.MapControllerRoute(
    name: "category",
    pattern: "category/{categoryName}",
    defaults: new { controller = "Products", action = "Index" }
);

app.MapControllerRoute(
    name: "compare",
    pattern: "compare/{id}",
    defaults: new { controller = "Products", action = "Compare" }
);

app.MapControllerRoute(
    name: "about",
    pattern: "about",
    defaults: new { controller = "Home", action = "About" });

app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Home", action = "Login" });

app.MapControllerRoute(
    name: "register",
    pattern: "register",
    defaults: new { controller = "Home", action = "Register" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize and migrate database
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MoqarenContext>();
        context.Database.Migrate();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

// Start the application
app.Run();

