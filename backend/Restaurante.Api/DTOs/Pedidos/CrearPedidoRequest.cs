using System.ComponentModel.DataAnnotations;

namespace Restaurante.Api.DTOs.Pedidos;

public class CrearPedidoRequest
{
    [Required]
    public int IdMesa { get; set; }

    [Required, MinLength(1, ErrorMessage = "El pedido debe tener al menos un producto.")]
    public List<ItemPedidoRequest> Items { get; set; } = new();
}

public class ItemPedidoRequest
{
    [Required]
    public int IdProducto { get; set; }

    [Required, Range(1, 100, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    public string? Observaciones { get; set; }
}