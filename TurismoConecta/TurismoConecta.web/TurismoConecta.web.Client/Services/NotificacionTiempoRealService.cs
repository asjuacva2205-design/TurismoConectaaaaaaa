using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    /// <summary>
    /// Mantiene UNA conexión SignalR y la lista de notificaciones de la sesión.
    /// Los componentes se suscriben al evento Cambio para redibujarse.
    /// </summary>
    public sealed class NotificacionTiempoRealService : IAsyncDisposable
    {
        private readonly NotificacionApiService _api;
        private readonly ApiConfig _config;
        private readonly IJSRuntime _js;
        private readonly SemaphoreSlim _candado = new(1, 1);

        private HubConnection? _conexion;

        public List<NotificacionDto> Notificaciones { get; private set; } = new();
        public int NoLeidas => Notificaciones.Count(n => !n.Leida);
        public HubConnectionState Estado => _conexion?.State ?? HubConnectionState.Disconnected;

        /// <summary>Se dispara cuando llega algo nuevo o cambia la conexión.</summary>
        public event Action? Cambio;

        /// <summary>Se dispara con CADA notificación que llega en vivo. Las páginas la usan para recargar sus datos.</summary>
        public event Action<NotificacionDto>? NotificacionRecibida;

        public NotificacionTiempoRealService(NotificacionApiService api, ApiConfig config, IJSRuntime js)
        {
            _api = api;
            _config = config;
            _js = js;
        }

        // ─────────────────────────────────────────────
        // CONECTAR (una sola vez por sesión)
        // ─────────────────────────────────────────────
        public async Task IniciarAsync()
        {
            await _candado.WaitAsync();
            try
            {
                if (_conexion is not null) return;

                // 1. Historial: lo que llegó mientras estaba desconectado
                Notificaciones = await _api.ListarAsync();
                AvisarCambio();

                // 2. Conexión en vivo con el token
                _conexion = new HubConnectionBuilder()
                    .WithUrl($"{_config.BaseUrl}/hubs/notificaciones", opciones =>
                    {
                        opciones.AccessTokenProvider = ObtenerTokenAsync;
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // 3. Escuchar el evento que envía NotificacionService
                _conexion.On<NotificacionDto>("NuevaNotificacion", nueva =>
                {
                    Notificaciones.Insert(0, nueva);
                    AvisarCambio();
                    NotificacionRecibida?.Invoke(nueva);
                });

                _conexion.Reconnecting += _ => { AvisarCambio(); return Task.CompletedTask; };
                _conexion.Closed += _ => { AvisarCambio(); return Task.CompletedTask; };
                _conexion.Reconnected += async _ =>
                {
                    Notificaciones = await _api.ListarAsync();   // por si llegó algo durante el corte
                    AvisarCambio();
                };

                await _conexion.StartAsync();
                AvisarCambio();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notificaciones] No se pudo conectar al hub: {ex.Message}");
                if (_conexion is not null)
                {
                    await _conexion.DisposeAsync();
                    _conexion = null;   // permite reintentar después
                }
                AvisarCambio();
            }
            finally
            {
                _candado.Release();
            }
        }

        // ─────────────────────────────────────────────
        // MARCAR LEÍDAS
        // ─────────────────────────────────────────────
        public async Task MarcarLeidaAsync(NotificacionDto notificacion)
        {
            if (notificacion.Leida) return;

            if (await _api.MarcarLeidaAsync(notificacion.IdNotificacion))
            {
                notificacion.Leida = true;
                AvisarCambio();
            }
        }

        public async Task MarcarTodasLeidasAsync()
        {
            if (NoLeidas == 0) return;

            if (await _api.MarcarTodasLeidasAsync())
            {
                foreach (var n in Notificaciones) n.Leida = true;
                AvisarCambio();
            }
        }

        // ─────────────────────────────────────────────
        // DESCONECTAR
        // ─────────────────────────────────────────────
        public async Task DetenerAsync()
        {
            if (_conexion is not null)
            {
                await _conexion.DisposeAsync();
                _conexion = null;
            }
            Notificaciones = new();
            AvisarCambio();
        }

        public async ValueTask DisposeAsync() => await DetenerAsync();

        // ─────────────────────────────────────────────
        // AYUDANTES
        // ─────────────────────────────────────────────
        private async Task<string?> ObtenerTokenAsync()
        {
            try
            {
                return await _js.InvokeAsync<string?>("localStorage.getItem", "authToken");
            }
            catch
            {
                return null;
            }
        }

        private void AvisarCambio() => Cambio?.Invoke();
    }
}
