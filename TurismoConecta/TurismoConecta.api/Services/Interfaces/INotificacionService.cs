using TurismoConecta.api.DTOs.Notificaciones;

namespace TurismoConecta.api.Services.Interfaces
{
    public interface INotificacionService
    {
        /// <summary>Guarda la notificación y la envía en vivo si el usuario está conectado.</summary>
        Task EnviarAsync(int idUsuario, string codigoTipo, string mensaje, CancellationToken ct = default);

        /// <summary>Últimas notificaciones del usuario (historial).</summary>
        Task<List<NotificacionDto>> ListarAsync(int idUsuario, bool soloNoLeidas, CancellationToken ct = default);

        /// <summary>Marca una como leída. Devuelve false si no existe o no es del usuario.</summary>
        Task<bool> MarcarLeidaAsync(int idNotificacion, int idUsuario, CancellationToken ct = default);

        /// <summary>Marca todas las del usuario como leídas. Devuelve cuántas cambió.</summary>
        Task<int> MarcarTodasLeidasAsync(int idUsuario, CancellationToken ct = default);
    }
}
