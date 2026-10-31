using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application;
using ScoreRebanho.Domain;
using ScoreRebanho.Infrastructure.Data;
namespace ScoreRebanho.Infrastructure;

public static class RegistroInfrastructure
{
    public static IServiceCollection AdicionarInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(config.GetConnectionString("DefaultConnection") ?? "Data Source=score.db"));
        var p = config.GetSection("Premissas").Get<Premissas>() ?? new Premissas();
        p.Validar();
        services.AddSingleton<IProvedorPremissas>(new Provedor(p));
        services.AddScoped<IHistoricoScores, HistoricoScores>();
        services.AddScoped<IRebanhos, Rebanhos>();
        return services;
    }
    public static IdentityBuilder AdicionarStores(this IdentityBuilder builder) => builder.AddEntityFrameworkStores<ApplicationDbContext>();
    public static async Task MigrarBancoAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    }
    private sealed class Provedor(Premissas p) : IProvedorPremissas { public Premissas Obter() => p with { }; }
}


