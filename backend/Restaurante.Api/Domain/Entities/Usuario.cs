namespace Restaurante.Api.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    // Hash de la contraseña, nunca texto plano
    public string ContrasenaHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    // Bloqueo por intentos fallidos (RN-10)
    public int IntentosFallidos { get; set; } = 0;
    public DateTime? BloqueadoHasta { get; set; }

    public int IdRol { get; set; }
    public Rol? Rol { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}