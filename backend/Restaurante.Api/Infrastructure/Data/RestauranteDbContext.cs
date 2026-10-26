using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Domain.Entities;

namespace Restaurante.Api.Infrastructure.Data;

public class RestauranteDbContext : DbContext
{
    public RestauranteDbContext(DbContextOptions<RestauranteDbContext> options)
        : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallePedidos => Set<DetallePedido>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol);
            entity.Property(e => e.NombreRol).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.NombreRol).IsUnique();
        });

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TipoDocumento).IsRequired().HasMaxLength(20);
            entity.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Correo).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ContrasenaHash).IsRequired().HasMaxLength(255);

            // RN-09: correo y número de documento únicos
            entity.HasIndex(e => e.Correo).IsUnique();
            entity.HasIndex(e => e.NumeroDocumento).IsUnique();

            entity.HasOne(e => e.Rol)
                  .WithMany(r => r.Usuarios)
                  .HasForeignKey(e => e.IdRol)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Mesa
        modelBuilder.Entity<Mesa>(entity =>
        {
            entity.HasKey(e => e.IdMesa);
            entity.HasIndex(e => e.NumeroMesa).IsUnique();
        });

        // Producto
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Categoria).HasMaxLength(50);
        });

        // Pedido
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Mesa)
                  .WithMany(m => m.Pedidos)
                  .HasForeignKey(e => e.IdMesa)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Pedidos)
                  .HasForeignKey(e => e.IdUsuario)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // DetallePedido
        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.IdDetalle);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Observaciones).HasMaxLength(300);

            entity.HasOne(e => e.Pedido)
                  .WithMany(p => p.Detalles)
                  .HasForeignKey(e => e.IdPedido)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                  .WithMany(p => p.Detalles)
                  .HasForeignKey(e => e.IdProducto)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Factura (relación 1 a 1 con Pedido)
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Pedido)
                  .WithOne(p => p.Factura)
                  .HasForeignKey<Factura>(e => e.IdPedido)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Pago
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago);
            entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Factura)
                  .WithMany(f => f.Pagos)
                  .HasForeignKey(e => e.IdFactura)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}