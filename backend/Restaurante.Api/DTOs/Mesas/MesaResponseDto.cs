namespace Restaurante.Api.DTOs.Mesas;

public class MesaResponseDto
{
    public int IdMesa { get; set; }
    public int NumeroMesa { get; set; }
    public string Estado { get; set; } = string.Empty;
}