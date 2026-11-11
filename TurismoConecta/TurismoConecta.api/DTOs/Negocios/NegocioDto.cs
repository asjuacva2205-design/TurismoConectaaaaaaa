namespace TurismoConecta.api.DTOs.Negocios
{
    public class NegocioDto
    {
        public int IdNegocio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public int IdMunicipio { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string NombreMunicipio { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Horario { get; set; }
        public string? Direccion { get; set; }
        public decimal? Latitud { get; set; }   // HU-26: para el mapa de ubicación
        public decimal? Longitud { get; set; }
        public double? PromedioCalificacion { get; set; }
        public List<string> Galeria { get; set; } = new();
    }
}   
