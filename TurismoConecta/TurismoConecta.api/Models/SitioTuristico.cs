namespace TurismoConecta.api.Models
{
    public partial class SitioTuristico
    {
        // ── Columnas que ya existían ──
        public int IdSitioTuristico { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public int IdMunicipio { get; set; }
        public int? IdReseña { get; set; }

        // ── Columnas nuevas ──
        public int? IdCategoria { get; set; }
        public int? Altitud { get; set; }
        public bool Destacado { get; set; }
        public bool Activo { get; set; }

        // ── Propiedades de navegación ──
        public virtual Municipio IdMunicipioNavigation { get; set; } = null!;
        public virtual Reseña? IdReseñaNavigation { get; set; }
        public virtual Categoria? IdCategoriaNavigation { get; set; }
    }
}
