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
       if (mesa.Estado == EstadoMesa.Ocupada)
    throw new Exception($"La mesa {mesa.NumeroMesa} ya está ocupada.");
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
    public async Task CancelarPedidoAsync(int idPedido)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.Mesa)
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

        if (pedido == null) throw new Exception("Pedido no encontrado.");

        // 1. Validar con el Patrón State
        var estadoActual = EstadoPedidoFactory.Crear(pedido.Estado);
        if (!estadoActual.PuedeCancelar())
        {
            throw new Exception($"El pedido no se puede cancelar porque está en estado: {estadoActual.Nombre}.");
        }

        // 2. Cambiar estado del pedido
        pedido.Estado = EstadoPedido.Cancelado;

        // 3. Regla de negocio RN-06: Verificar si hay otros pedidos activos en la misma mesa
        var otrosPedidosActivos = await _db.Pedidos
            .AnyAsync(p => p.IdMesa == pedido.IdMesa && 
                           p.IdPedido != pedido.IdPedido && 
                           p.Estado != EstadoPedido.Cancelado && 
                           p.Estado != EstadoPedido.Facturado);

        if (!otrosPedidosActivos && pedido.Mesa != null)
        {
            pedido.Mesa.Estado = EstadoMesa.Disponible;
        }

        await _db.SaveChangesAsync();

        // 4. Notificar a cocina para que elimine el pedido de su pantalla
        await _subject.NotificarCambioEstadoAsync(pedido.IdPedido, "Cancelado");
    }

    public async Task<Pedido> ModificarPedidoAsync(int idPedido, ModificarPedidoRequest request)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

        if (pedido == null) throw new Exception("Pedido no encontrado.");

        // 1. Validar con el Patrón State si el pedido se puede modificar
        var estadoActual = EstadoPedidoFactory.Crear(pedido.Estado);
        if (!estadoActual.PuedeModificar())
            throw new Exception($"El pedido no se puede modificar porque está en estado: {estadoActual.Nombre}.");

        // 2. Eliminar los detalles antiguos
        _db.DetallePedidos.RemoveRange(pedido.Detalles);

        // 3. Calcular nuevos totales y agregar nuevos detalles
        decimal nuevoTotal = 0;
        var nuevosDetalles = new List<DetallePedido>();

        foreach (var item in request.Items)
        {
            var producto = await _db.Productos.FindAsync(item.IdProducto);
            if (producto == null) throw new Exception($"El producto con ID {item.IdProducto} no existe.");
            if (!producto.Disponible) throw new Exception($"El producto '{producto.Nombre}' no está disponible.");

            var subtotal = producto.Precio * item.Cantidad;
            nuevoTotal += subtotal;

            nuevosDetalles.Add(new DetallePedido
            {
                IdPedido = pedido.IdPedido,
                IdProducto = producto.IdProducto,
                Cantidad = item.Cantidad,
                Observaciones = item.Observaciones,
                PrecioUnitario = producto.Precio,
                Subtotal = subtotal
            });
        }

        pedido.Total = nuevoTotal;
        _db.DetallePedidos.AddRange(nuevosDetalles);

        await _db.SaveChangesAsync();

        // 4. Notificar a cocina sobre la modificación
        await _subject.NotificarCambioEstadoAsync(pedido.IdPedido, "Modificado");

        return pedido;
    }

}