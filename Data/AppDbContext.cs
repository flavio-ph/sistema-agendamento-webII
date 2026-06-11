using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Models;
using System.Reflection.Emit;

namespace SistemaAgendamentoWebII.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Establishment> Establishments { get; set; }
    public DbSet<Professional> Professionals { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    // Adicionar os restantes DbSets aqui...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Chave Primária Composta para Favorites
        modelBuilder.Entity<Favorite>()
            .HasKey(f => new { f.ClientId, f.ProfessionalId });

        // Garantir que o Email é único
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Client)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Professional)
            .WithMany()
            .HasForeignKey(f => f.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}