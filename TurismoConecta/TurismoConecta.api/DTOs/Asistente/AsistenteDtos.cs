using System.ComponentModel.DataAnnotations;

namespace TurismoConecta.api.DTOs.Asistente;

public class AsistenteMensajeDto
{
    [Required(ErrorMessage = "El mensaje no puede estar vacío.")]
    [MaxLength(600, ErrorMessage = "El mensaje es demasiado largo (máximo 600 caracteres).")]
    public string Mensaje { get; set; } = string.Empty;

    [MaxLength(20)]
    public List<TurnoChatDto> Historial { get; set; } = new();

    /// <summary>Situación del viaje en curso (sin coordenadas del usuario). Opcional.</summary>
    [MaxLength(1200)]
    public string? Contexto { get; set; }
}

public class TurnoChatDto
{
    public string Rol { get; set; } = "user";   // Esta el user para que responda depediendo del rol que este abarca
    public string Contenido { get; set; } = string.Empty;
}

public class AsistenteRespuestaDto
{
    public string Respuesta { get; set; } = string.Empty;
}
