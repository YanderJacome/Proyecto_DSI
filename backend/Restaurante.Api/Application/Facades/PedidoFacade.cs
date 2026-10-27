using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Application.Observers; // <-- NUEVO
using Restaurante.Api.Domain.Entities;
using Restaurante.Api.Domain.Enums;
using Restaurante.Api.Domain.States;
using Restaurante.Api.DTOs.Pedidos;
using Restaurante.Api.Infrastructure.Data;

namespace Restaurante.Api.Application.Facades;

public class PedidoFacade
{
    private readonly RestauranteDbContext _db;
    private readonly PedidoSubject _subject; // <-- NUEVO

    public PedidoFacade(RestauranteDbContext db, PedidoSubject subject)
    {
        _db = db;
        _subject = subject;
    }

    public async Task<Pedido> RegistrarPedidoAsync(CrearPedidoRequest request, int idUsuario)
    {
        // ... (El mismo código de validación de mesa y productos de la etapa anterior) ...
        var mesa = await _db.Mesas.FindAsync(request.IdMesa);
        if (mesa == null) throw new Exception("La mesa no existe.");
        if (mesa.Estado != EstadoMesa.Disponible) throw new Exception($"La mesa {mesa.NumeroMesa} no está disponible.");

        decimal total = 0;
        var detalles = new List<DetallePedido>();

        foreach (var item in request.Items)
        {
            var producto = await _db.Productos.FindAsync(item.IdProducto);
            if (producto == null) throw new Exception($"El producto no existe.");
            if (!producto.Disponible) throw new Exception($"El producto '{producto.Nombre}' no está disponible.");

            var subtotal = producto.Precio * item.Cantidad;
            total += subtotal;
            detalles.Add(new DetallePedido {
                IdProducto = producto.IdProducto, Cantidad = item.Cantidad,
                Observaciones = item.Observaciones, PrecioUnitario = producto.Precio, Subtotal = subtotal
            });
        }

        var estadoInicial = EstadoPedidoFactory.Crear(EstadoPedido.EnviadoACocina);

        var pedido = new Pedido {
            IdMesa = request.IdMesa, IdUsuario = idUsuario, Fecha = DateTime.UtcNow,
            Estado = estadoInicial.Estado, Total = total, Detalles = detalles
        };

        _db.Pedidos.Add(pedido);
        mesa.Estado = EstadoMesa.Ocupada;

        await _db.SaveChangesAsync();

        // NOTIFICAR A LOS OBSERVERS (Aquí ocurre la magia de SignalR)
        await _subject.NotificarNuevoPedidoAsync(pedido.IdPedido);

        return pedido;
    }
}