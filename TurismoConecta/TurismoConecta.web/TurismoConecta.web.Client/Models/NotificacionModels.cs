namespace TurismoConecta.web.Client.Models
{
    /// <summary>Copia exacta del NotificacionDto de la API.</summary>
    public class NotificacionDto
    {
        public int IdNotificacion { get; set; }
        public string Codigo { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
