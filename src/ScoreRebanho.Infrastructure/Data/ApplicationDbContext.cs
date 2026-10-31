using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ScoreRebanho.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<RebanhoPersistido>(entity =>
        {
            entity.ToTable("Rebanhos");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.UsuarioId).IsRequired();
            entity.Property(r => r.Nome).HasMaxLength(100).IsRequired();
            entity.Property(r => r.Propriedade).HasMaxLength(150).IsRequired();
            entity.Property(r => r.EntradaJson).IsRequired();
            entity.Property(r => r.ResultadoJson).IsRequired();
            entity.HasIndex(r => new { r.UsuarioId, r.AtualizadoEmUtc });
        });
        builder.Entity<ScorePersistido>(entity =>
        {
            entity.ToTable("Scores");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.UsuarioId).IsRequired();
            entity.Property(s => s.EntradaJson).IsRequired();
            entity.Property(s => s.ResultadoJson).IsRequired();
            entity.HasIndex(s => s.CriadoEmUtc);
        });
    }
}



