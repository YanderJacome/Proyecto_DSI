using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Domain.Entities;
using Restaurante.Api.Domain.Enums;
using Restaurante.Api.Domain.States;
using Restaurante.Api.DTOs.Pedidos;
using Restaurante.Api.Infrastructure.Data;

namespace Restaurante.Api.Application.Facades;

public class PedidoFacade
{
    private readonly RestauranteDbContext _db;

    public PedidoFacade(RestauranteDbContext db)
    {
        _db = db;
    }

    public async Task<Pedido> RegistrarPedidoAsync(CrearPedidoRequest request, int idUsuario)
    {
        // 1. Validar Mesa
        var mesa = await _db.Mesas.FindAsync(request.IdMesa);
        if (mesa == null)
            throw new Exception("La mesa no existe.");
            
        if (mesa.Estado != EstadoMesa.Disponible)
            throw new Exception($"La mesa {mesa.NumeroMesa} no está disponible (Estado actual: {mesa.Estado}).");

        // 2. Validar Productos y calcular totales
        decimal total = 0;
        var detalles = new List<DetallePedido>();

        foreach (var item in request.Items)
        {
            var producto = await _db.Productos.FindAsync(item.IdProducto);
            if (producto == null)
                throw new Exception($"El producto con ID {item.IdProducto} no existe.");
                
            if (!producto.Disponible)
                throw new Exception($"El producto '{producto.Nombre}' no está disponible.");

            var subtotal = producto.Precio * item.Cantidad;
            total += subtotal;

            detalles.Add(new DetallePedido
            {
                IdProducto = producto.IdProducto,
                Cantidad = item.Cantidad,
                Observaciones = item.Observaciones,
                PrecioUnitario = producto.Precio,
                Subtotal = subtotal
            });
        }

        // 3. Crear Pedido (Aplicando el patrón State para su estado inicial)
        var estadoInicial = EstadoPedidoFactory.Crear(EstadoPedido.EnviadoACocina);

        var pedido = new Pedido
        {
            IdMesa = request.IdMesa,
            IdUsuario = idUsuario,
            Fecha = DateTime.UtcNow,
            Estado = estadoInicial.Estado,
            Total = total,
            Detalles = detalles
        };

        // 4. Guardar en Base de Datos
        _db.Pedidos.Add(pedido);

        // 5. Actualizar Estado de la Mesa
        mesa.Estado = EstadoMesa.Ocupada;

        await _db.SaveChangesAsync(); // Guardado atómico

        // 6. Aquí iría Observer/SignalR (Lo implementaremos en la siguiente etapa)
        // await _notifier.NotificarNuevoPedidoAsync(pedido);

        return pedido;
    }
}