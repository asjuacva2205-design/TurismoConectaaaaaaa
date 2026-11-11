namespace TurismoConecta.api.DTOs.SitiosTuristicos
{
    /// <summary>
    /// Todo lo que necesita la pantalla de gestión en una sola respuesta:
    /// quién es el usuario, qué municipio administra y sus sitios (incluidos los ocultos).
    /// </summary>
    public class PanelSitiosDto
    {
        public bool EsAdminGeneral { get; set; }
        public int? IdMunicipioAsignado { get; set; }
        public string? NombreMunicipioAsignado { get; set; }
        public List<SitioTuristicoDto> Sitios { get; set; } = new();
    }
}
