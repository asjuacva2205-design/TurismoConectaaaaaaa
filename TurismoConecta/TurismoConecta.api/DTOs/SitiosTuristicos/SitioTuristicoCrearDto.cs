using System.ComponentModel.DataAnnotations;

namespace TurismoConecta.api.DTOs.SitiosTuristicos
{
    public class SitioTuristicoCrearDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
        public string Nombre { get; set; } = "";

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(500)]
        public string? ImagenUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un municipio.")]
        public int IdMunicipio { get; set; }

        public int? IdCategoria { get; set; }

        [Range(0, 6000, ErrorMessage = "La altitud debe estar entre 0 y 6000 msnm.")]
        public int? Altitud { get; set; }

        public bool Destacado { get; set; }
    }

    public class SitioTuristicoEditarDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = "";

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(500)]
        public string? ImagenUrl { get; set; }

        public int? IdCategoria { get; set; }

        [Range(0, 6000)]
        public int? Altitud { get; set; }

        public bool Destacado { get; set; }
    }
}
