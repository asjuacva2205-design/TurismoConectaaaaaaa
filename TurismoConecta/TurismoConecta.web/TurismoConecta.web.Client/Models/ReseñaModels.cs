namespace TurismoConecta.web.Client.Models
{
    // Mismo contrato que DTOs/Reseñas/ReseñaDto.cs del API
    public class ReseñaDto
    {
        public int IdReseña { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";

        // Lugar reseñado: uno de los dos viene con valor
        public int? IdMunicipio { get; set; }
        public int? IdNegocio { get; set; }
        public string NombreLugar { get; set; } = "";

        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public string? Respuesta { get; set; }
        public DateTime? FechaRespuesta { get; set; }

        // true = visible al público; false = retenida por el filtro antispam (HU-30)
        public bool Moderada { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CrearReseñaRequest
    {
        public int? IdMunicipio { get; set; }
        public int? IdNegocio { get; set; }
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
    }
}
