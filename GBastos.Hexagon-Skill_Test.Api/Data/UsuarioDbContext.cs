using GBastos.Hexagon_Skill_Test.Api.Messaging.Outbox;
using GBastos.Hexagon_Skill_Test.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GBastos.Hexagon_Skill_Test.Api.Data;

public class UsuarioDbContext : DbContext
{
    public UsuarioDbContext(DbContextOptions<UsuarioDbContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Nome)
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.CPF)
            .IsRequired();
    }
}
