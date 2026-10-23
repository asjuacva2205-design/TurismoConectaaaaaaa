namespace TurismoConecta.api.DTOs.Negocios
{
    // HU-24: bandeja de pendientes.
    // Trae todo lo que el admin necesita para decidir sin abrir otra pantalla:
    // fotos, contacto, ubicación y datos del propietario.
    public class NegocioPendienteDto
    {
        public int IdNegocio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string NombrePropietario { get; set; } = string.Empty;
        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = string.Empty;
        public string NombreCategoria { get; set; } = string.Empty;

        // ◆ NUEVO: detalle para el panel de revisión
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Horario { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public List<string> Fotos { get; set; } = new();
        public List<string> Etiquetas { get; set; } = new();

        // ◆ NUEVO: confianza en el propietario
        public string EmailPropietario { get; set; } = string.Empty;
        public int NegociosAprobadosPropietario { get; set; }
    }
}
