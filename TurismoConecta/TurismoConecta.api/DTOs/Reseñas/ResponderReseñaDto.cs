using System.ComponentModel.DataAnnotations;

namespace TurismoConecta.api.DTOs.Reseñas
{
    public class ResponderReseñaDto
    {
        [Required(ErrorMessage = "Escribe una respuesta.")]
        [MaxLength(1000, ErrorMessage = "La respuesta no puede superar 1000 caracteres.")]
        public string Respuesta { get; set; } = string.Empty;
    }
}
