namespace Restaurante.Api.Domain.Entities;

public class DetallePedido
{
    public int IdDetalle { get; set; }
    public int Cantidad { get; set; }
    public string? Observaciones { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public int IdPedido { get; set; }
    public Pedido? Pedido { get; set; }

    public int IdProducto { get; set; }
    public Producto? Producto { get; set; }
}