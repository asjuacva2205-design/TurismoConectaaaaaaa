namespace TurismoConecta.web.Client.Models
{
    public class FavoritoItemDto
    {
        public int? IdMunicipio { get; set; }
        public string? NombreMunicipio { get; set; }
        public int? IdNegocio { get; set; }
        public string? NombreNegocio { get; set; }
        public DateTime FechaGuardado { get; set; }
    }

    public class FavoritoToggleRespuesta
    {
        public bool Marcado { get; set; }
    }
}
