namespace TurismoConecta.api.DTOs.SitiosTuristicos
{
    public class SitioTuristicoDto
    {
        public int IdSitioTuristico { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }

        // Municipio
        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = "";
        public string? Clima { get; set; }

        // Datos del sitio
        public int? Altitud { get; set; }
        public bool Destacado { get; set; }
        public bool Activo { get; set; }

        // Categoría
        public int? IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public string? IconoCategoria { get; set; }

        public double? Calificacion { get; set; }
    }
}
