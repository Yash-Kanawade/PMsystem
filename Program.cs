using Microsoft.EntityFrameworkCore;
using PMSystem.Data;
using PMSystem.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5050); // HTTP
    options.ListenAnyIP(5051, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session configuration (for future use)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP for dev
});

// Register filters (disabled for now)
builder.Services.AddScoped<AuthenticationFilter>();
builder.Services.AddScoped<ManagerOnlyFilter>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var retryCount = 0;
        var maxRetries = 10;
        
        while (retryCount < maxRetries)
        {
            try
            {
                db.Database.Migrate();
                Console.WriteLine("✅ Database migration completed successfully");
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"⚠️ Database not ready, retrying... ({retryCount}/{maxRetries})");
                Console.WriteLine($"Error: {ex.Message}");
                
                if (retryCount >= maxRetries)
                {
                    Console.WriteLine("❌ Failed to connect to database after maximum retries");
                    throw;
                }
                
                Thread.Sleep(2000);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex.Message}");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 PM System API is running");
Console.WriteLine("📡 HTTP:  http://localhost:5050");
Console.WriteLine("🔒 HTTPS: https://localhost:5051");
Console.WriteLine("📚 Swagger: https://localhost:5051/swagger");

app.Run();