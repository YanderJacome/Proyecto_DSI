using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Domain.Entities;
using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(RestauranteDbContext db)
    {
        // Aplicar migraciones pendientes (seguro si ya se aplicaron)
        await db.Database.MigrateAsync();

        // 1. Roles
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Rol { NombreRol = "Administrador" },
                new Rol { NombreRol = "Mesero" },
                new Rol { NombreRol = "Cajero" },
                new Rol { NombreRol = "Cocina" }
            );
            await db.SaveChangesAsync();
        }

        // 2. Usuarios de prueba (uno por rol)
        if (!await db.Usuarios.AnyAsync())
        {
            var roles = await db.Roles.ToDictionaryAsync(r => r.NombreRol);

            db.Usuarios.AddRange(
                new Usuario
                {
                    Nombre = "Administrador General",
                    TipoDocumento = "CC",
                    NumeroDocumento = "1000000001",
                    Correo = "admin@restaurante.com",
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Activo = true,
                    IdRol = roles["Administrador"].IdRol
                },
                new Usuario
                {
                    Nombre = "Mesero Principal",
                    TipoDocumento = "CC",
                    NumeroDocumento = "1000000002",
                    Correo = "mesero@restaurante.com",
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword("Mesero123!"),
                    Activo = true,
                    IdRol = roles["Mesero"].IdRol
                },
                new Usuario
                {
                    Nombre = "Cajero Principal",
                    TipoDocumento = "CC",
                    NumeroDocumento = "1000000003",
                    Correo = "cajero@restaurante.com",
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword("Cajero123!"),
                    Activo = true,
                    IdRol = roles["Cajero"].IdRol
                },
                new Usuario
                {
                    Nombre = "Cocinero Principal",
                    TipoDocumento = "CC",
                    NumeroDocumento = "1000000004",
                    Correo = "cocina@restaurante.com",
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword("Cocina123!"),
                    Activo = true,
                    IdRol = roles["Cocina"].IdRol
                }
            );
            await db.SaveChangesAsync();
        }

        // 3. Mesas (1 a 8, todas disponibles)
        if (!await db.Mesas.AnyAsync())
        {
            for (int i = 1; i <= 8; i++)
            {
                db.Mesas.Add(new Mesa
                {
                    NumeroMesa = i,
                    Estado = EstadoMesa.Disponible
                });
            }
            await db.SaveChangesAsync();
        }

        // 4. Productos por categoría
        if (!await db.Productos.AnyAsync())
        {
            db.Productos.AddRange(
                // Entradas
                new Producto { Nombre = "Nachos con queso", Precio = 12000m, Categoria = "Entradas", Disponible = true },
                new Producto { Nombre = "Alitas BBQ", Precio = 18000m, Categoria = "Entradas", Disponible = true },
                new Producto { Nombre = "Papas fritas", Precio = 8000m, Categoria = "Entradas", Disponible = true },
                new Producto { Nombre = "Ensalada César", Precio = 15000m, Categoria = "Entradas", Disponible = true },
                // Platos
                new Producto { Nombre = "Hamburguesa clásica", Precio = 25000m, Categoria = "Platos", Disponible = true },
                new Producto { Nombre = "Pizza pepperoni", Precio = 30000m, Categoria = "Platos", Disponible = true },
                new Producto { Nombre = "Bandeja paisa", Precio = 32000m, Categoria = "Platos", Disponible = true },
                new Producto { Nombre = "Pechuga a la plancha", Precio = 28000m, Categoria = "Platos", Disponible = true },
                new Producto { Nombre = "Pasta carbonara", Precio = 27000m, Categoria = "Platos", Disponible = true },
                // Bebidas
                new Producto { Nombre = "Limonada natural", Precio = 6000m, Categoria = "Bebidas", Disponible = true },
                new Producto { Nombre = "Gaseosa", Precio = 5000m, Categoria = "Bebidas", Disponible = true },
                new Producto { Nombre = "Cerveza", Precio = 8000m, Categoria = "Bebidas", Disponible = true },
                new Producto { Nombre = "Café", Precio = 4000m, Categoria = "Bebidas", Disponible = true },
                // Postres
                new Producto { Nombre = "Brownie con helado", Precio = 12000m, Categoria = "Postres", Disponible = true },
                new Producto { Nombre = "Cheesecake", Precio = 11000m, Categoria = "Postres", Disponible = true },
                new Producto { Nombre = "Flan casero", Precio = 9000m, Categoria = "Postres", Disponible = true }
            );
            await db.SaveChangesAsync();
        }
    }
}