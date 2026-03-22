using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MindReader.Application.UseCases;
using MindReader.Domain.Interfaces;
using MindReader.Infrastructure.Persistence;
using MindReader.Infrastructure.Persistence.Repositories;
using MindReader.Infrastructure.Services;

namespace MindReader.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=mindreader.db";

        services.AddDbContext<MindReaderDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<IOracleMissRepository, OracleMissRepository>();

        var apiKey = configuration["ClaudeApiKey"]
            ?? Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
            ?? throw new InvalidOperationException("Claude API key is not configured.");

        services.AddHttpClient("claude", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<IClaudeAIService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient("claude");
            return new ClaudeAIService(httpClient, apiKey);
        });

        services.AddScoped<StartGameUseCase>();
        services.AddScoped<AnswerQuestionUseCase>();
        services.AddScoped<RecordMissUseCase>();

        return services;
    }
}
