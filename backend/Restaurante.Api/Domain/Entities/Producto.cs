namespace Restaurante.Api.Domain.Entities;

public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Disponible { get; set; } = true;

    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}