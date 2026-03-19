using GBastos.Hexagon_Skill_Test.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GBastos.Hexagon_Skill_Test.Api.Data;

public class UsuarioDbContext : DbContext
{
    public UsuarioDbContext(DbContextOptions<UsuarioDbContext> options) : base(options) { }
    public DbSet<Usuario> Usuarios => Set<Usuario>();
}