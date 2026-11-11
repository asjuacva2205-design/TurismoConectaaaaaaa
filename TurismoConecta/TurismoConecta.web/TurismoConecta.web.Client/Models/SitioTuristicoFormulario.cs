using System.ComponentModel.DataAnnotations;

namespace TurismoConecta.web.Client.Models
{
    /// <summary>
    /// Modelo del formulario de crear/editar sitio turístico.
    /// Las validaciones se muestran en pantalla ANTES de llamar a la API.
    /// </summary>
    public class SitioTuristicoFormulario
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
        public string Nombre { get; set; } = "";

        [StringLength(1000, ErrorMessage = "La descripción no puede superar 1000 caracteres.")]
        public string? Descripcion { get; set; }

        [StringLength(500)]
        public string? ImagenUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un municipio.")]
        public int IdMunicipio { get; set; }

        [Required(ErrorMessage = "Selecciona una categoría.")]
        public int? IdCategoria { get; set; }

        [Range(0, 6000, ErrorMessage = "La altitud debe estar entre 0 y 6.000 msnm.")]
        public int? Altitud { get; set; }

        public bool Destacado { get; set; }
    }
}
