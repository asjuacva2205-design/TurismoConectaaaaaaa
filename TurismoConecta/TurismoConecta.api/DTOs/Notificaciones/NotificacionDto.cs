namespace TurismoConecta.api.DTOs.Notificaciones
{
    /// <summary>Lo que recibe el navegador: por la API (historial) y por SignalR (en vivo).</summary>
    public class NotificacionDto
    {
        public int IdNotificacion { get; set; }
        public string Codigo { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
