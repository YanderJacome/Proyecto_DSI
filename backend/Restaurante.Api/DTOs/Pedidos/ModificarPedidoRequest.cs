using System.ComponentModel.DataAnnotations;

namespace Restaurante.Api.DTOs.Pedidos;

public class ModificarPedidoRequest
{
    [Required, MinLength(1, ErrorMessage = "El pedido debe tener al menos un producto.")]
    public List<ItemPedidoRequest> Items { get; set; } = new();
}