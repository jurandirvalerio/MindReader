using Microsoft.EntityFrameworkCore;
using MindReader.Infrastructure;
using MindReader.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add configuration from environment variable
builder.Configuration.AddEnvironmentVariables();
if (Environment.GetEnvironmentVariable("CLAUDE_API_KEY") is { } claudeKey && !string.IsNullOrEmpty(claudeKey))
    builder.Configuration["ClaudeApiKey"] = claudeKey;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MindReader API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MindReaderDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
