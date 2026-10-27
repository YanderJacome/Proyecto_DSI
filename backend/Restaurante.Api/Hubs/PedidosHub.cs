using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Restaurante.Api.Hubs;

[Authorize]
public class PedidosHub : Hub
{
        public async Task UnirseAGrupo(string grupo)
    {
        // Usamos ?. para evitar el warning de NULL
        bool isAdmin = Context.User?.IsInRole("Administrador") ?? false;

        if (grupo == "Cocina" && (Context.User?.IsInRole("Cocina") == true || isAdmin))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Cocina");
        }
        else if (grupo == "Meseros" && (Context.User?.IsInRole("Mesero") == true || isAdmin))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Meseros");
        }
    }
}