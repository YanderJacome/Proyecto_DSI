namespace Restaurante.Api.DTOs.Pedidos;

public class PedidoResponseDto
{
    public int IdPedido { get; set; }
    public int IdMesa { get; set; }
    public int NumeroMesa { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; }
}