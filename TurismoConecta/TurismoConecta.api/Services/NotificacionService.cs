using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Notificaciones;
using TurismoConecta.api.Hubs;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Services
{
    public class NotificacionService : INotificacionService
    {
        private const int MaxListado = 30;
        private const int LargoMaximoMensaje = 500;   // igual que la columna "mensaje" en SQL

        private readonly AppDbContext _context;
        private readonly IHubContext<NotificacionesHub> _hub;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(
            AppDbContext context,
            IHubContext<NotificacionesHub> hub,
            ILogger<NotificacionService> logger)
        {
            _context = context;
            _hub = hub;
            _logger = logger;
        }

        // ─────────────────────────────────────────────
        // ENVIAR: guarda en BD + envía por SignalR
        // ─────────────────────────────────────────────
        public async Task EnviarAsync(int idUsuario, string codigoTipo, string mensaje, CancellationToken ct = default)
        {
            try
            {
                // 1. Buscar el tipo por su código; si no existe, crearlo (red de seguridad)
                var tipo = await _context.TiposNotificacions
                    .FirstOrDefaultAsync(t => t.Codigo == codigoTipo, ct);

                if (tipo is null)
                {
                    tipo = new TiposNotificacion { Codigo = codigoTipo, Descripcion = codigoTipo };
                    _context.TiposNotificacions.Add(tipo);
                    await _context.SaveChangesAsync(ct);
                }

                // 2. Guardar en la BD (historial)
                var notificacion = new Notificacion
                {
                    IdUsuario = idUsuario,
                    IdTipoNotificacion = tipo.IdTipoNotificacion,
                    Mensaje = mensaje.Length > LargoMaximoMensaje ? mensaje[..LargoMaximoMensaje] : mensaje,
                    Leida = false,
                    FechaCreacion = DateTime.Now
                };

                _context.Notificacions.Add(notificacion);
                await _context.SaveChangesAsync(ct);

                // 3. Enviar en vivo SOLO al grupo de ese usuario
                var dto = new NotificacionDto
                {
                    IdNotificacion = notificacion.IdNotificacion,
                    Codigo = tipo.Codigo,
                    Mensaje = notificacion.Mensaje,
                    Leida = false,
                    FechaCreacion = notificacion.FechaCreacion
                };

                await _hub.Clients
                    .Group(NotificacionesHub.GrupoUsuario(idUsuario))
                    .SendAsync("NuevaNotificacion", dto, ct);
            }
            catch (Exception ex)
            {
                // Si el aviso falla, la operación principal (p. ej. aprobar) NO debe fallar
                _logger.LogError(ex, "No se pudo enviar la notificación {Codigo} al usuario {IdUsuario}", codigoTipo, idUsuario);
            }
        }

        // ─────────────────────────────────────────────
        // LISTAR: las más recientes primero
        // ─────────────────────────────────────────────
        public async Task<List<NotificacionDto>> ListarAsync(int idUsuario, bool soloNoLeidas, CancellationToken ct = default)
        {
            return await _context.Notificacions
                .AsNoTracking()
                .Where(n => n.IdUsuario == idUsuario && (!soloNoLeidas || !n.Leida))
                .OrderByDescending(n => n.FechaCreacion)
                .Take(MaxListado)
                .Select(n => new NotificacionDto
                {
                    IdNotificacion = n.IdNotificacion,
                    Codigo = n.IdTipoNotificacionNavigation.Codigo,
                    Mensaje = n.Mensaje,
                    Leida = n.Leida,
                    FechaCreacion = n.FechaCreacion
                })
                .ToListAsync(ct);
        }

        // ─────────────────────────────────────────────
        // MARCAR UNA COMO LEÍDA (solo si es del usuario)
        // ─────────────────────────────────────────────
        public async Task<bool> MarcarLeidaAsync(int idNotificacion, int idUsuario, CancellationToken ct = default)
        {
            var notificacion = await _context.Notificacions
                .FirstOrDefaultAsync(n => n.IdNotificacion == idNotificacion && n.IdUsuario == idUsuario, ct);

            if (notificacion is null) return false;

            if (!notificacion.Leida)
            {
                notificacion.Leida = true;
                await _context.SaveChangesAsync(ct);
            }

            return true;
        }

        // ─────────────────────────────────────────────
        // MARCAR TODAS COMO LEÍDAS
        // ─────────────────────────────────────────────
        public async Task<int> MarcarTodasLeidasAsync(int idUsuario, CancellationToken ct = default)
        {
            return await _context.Notificacions
                .Where(n => n.IdUsuario == idUsuario && !n.Leida)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true), ct);
        }
    }
}
