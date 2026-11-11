namespace TurismoConecta.api.DTOs.Municipios
{
    /// <summary>
    /// Versión mínima de un municipio para pintarlo en el mapa
    /// y para el buscador de la página de inicio.
    /// </summary>
    public class MunicipioMapaDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
    }
}
