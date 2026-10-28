using Microsoft.EntityFrameworkCore;
using TankManagement.Api.Models;

namespace TankManagement.Api.Data;

public class TankManagementDbContext : DbContext
{
    public TankManagementDbContext(DbContextOptions<TankManagementDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Tanque> Tanques => Set<Tanque>();
    public DbSet<UsuarioTanque> UsuarioTanques => Set<UsuarioTanque>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.UsuarioNombre).HasColumnName("Usuario").HasMaxLength(50).IsRequired();
            e.Property(x => x.Contrasena).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.UsuarioNombre).IsUnique();
            e.HasOne(x => x.Rol).WithMany(x => x.Usuarios).HasForeignKey(x => x.RolId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tanque>(e =>
        {
            e.ToTable("Tanques");
            e.HasKey(x => x.Id);
            e.Property(x => x.TipoContenido).HasMaxLength(100).IsRequired();
            e.Property(x => x.CapacidadActualCM3).HasPrecision(18, 2);
            e.Property(x => x.CapacidadTotalCM3).HasPrecision(18, 2);
            e.Property(x => x.Diametro).HasPrecision(10, 2);
            e.Property(x => x.AlturaCM).HasColumnType("double");
            e.Property(x => x.Ubicacion).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<UsuarioTanque>(e =>
        {
            e.ToTable("UsuarioTanque");
            e.HasKey(x => new { x.UsuarioId, x.TanqueId });
            e.HasOne(x => x.Usuario).WithMany(x => x.UsuarioTanques).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tanque).WithMany(x => x.UsuarioTanques).HasForeignKey(x => x.TanqueId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Solicitud>(e =>
        {
            e.ToTable("Solicitudes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Apellido).HasMaxLength(100).IsRequired();
            e.Property(x => x.Dni).HasMaxLength(30).IsRequired();
            e.Property(x => x.Direccion).HasMaxLength(255).IsRequired();
            e.Property(x => x.Fecha).HasColumnType("date");
            e.Property(x => x.Hora).HasColumnType("time");
            e.Property(x => x.Estado).HasDefaultValue(false);
        });
    }
}
