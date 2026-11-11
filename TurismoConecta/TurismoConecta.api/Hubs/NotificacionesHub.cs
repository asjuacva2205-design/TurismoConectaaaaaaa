using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TurismoConecta.api.Hubs
{
    /// <summary>
    /// Canal en tiempo real. Cada usuario conectado entra a su propio grupo
    /// "usuario-{id}" para que el servidor le envíe notificaciones solo a él.
    /// </summary>
    [Authorize]
    public class NotificacionesHub : Hub
    {
        /// <summary>Nombre del grupo de un usuario. También lo usa NotificacionService.</summary>
        public static string GrupoUsuario(int idUsuario) => $"usuario-{idUsuario}";

        /// <summary>Id del usuario del token. JwtBearer convierte "nameid" en ClaimTypes.NameIdentifier.</summary>
        private string? IdUsuarioActual => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public override async Task OnConnectedAsync()
        {
            if (int.TryParse(IdUsuarioActual, out var idUsuario))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GrupoUsuario(idUsuario));
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (int.TryParse(IdUsuarioActual, out var idUsuario))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoUsuario(idUsuario));
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>Solo para pruebas: devuelve el mensaje a quien lo envió.</summary>
        public async Task EnviarNotificacionPrueba(string mensaje)
        {
            await Clients.Caller.SendAsync("RecibirNotificacion", $"Eco del servidor: {mensaje}");
        }
    }
}
