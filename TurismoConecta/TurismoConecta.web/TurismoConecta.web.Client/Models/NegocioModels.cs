using System.ComponentModel.DataAnnotations;

namespace TurismoConecta.web.Client.Models
{
    public class NegocioItemDto
    {
        public int IdNegocio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobado, Rechazado
        public int IdCategoria { get; set; }
        public int IdMunicipio { get; set; }
        public string? NombreMunicipio { get; set; }
        public string? Telefono { get; set; }
        public string? Horario { get; set; }
        public string? Direccion { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public double? PromedioCalificacion { get; set; }
        public List<string> Galeria { get; set; } = new();

        public string Portada => Galeria.FirstOrDefault() ?? "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?q=80&w=800&auto=format&fit=crop";

        public string NombreCategoria { get; set; } = string.Empty;
    }

    public class FormNegocioDto
    {
        [Required(ErrorMessage = "El nombre del establecimiento es obligatorio")]
        [MaxLength(150, ErrorMessage = "Máximo 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Máximo 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una categoría")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida")]
        public int IdCategoria { get; set; } = 0;

        [Required(ErrorMessage = "Debes seleccionar un municipio")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un municipio")]
        public int IdMunicipio { get; set; }

        [MaxLength(30, ErrorMessage = "Teléfono máximo 30 caracteres")]
        public string? Telefono { get; set; }

        [MaxLength(200, ErrorMessage = "Horario máximo 200 caracteres")]
        public string? Horario { get; set; }

        [MaxLength(250, ErrorMessage = "Dirección máxima 250 caracteres")]
        public string? Direccion { get; set; }

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        public List<string> ImagenesGaleria { get; set; } = new();
    }

    public class NegocioPendienteAprobacionDto
    {
        public int IdNegocio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string NombrePropietario { get; set; } = string.Empty;
        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = string.Empty;
        public string NombreCategoria { get; set; } = string.Empty;

        // ◆ NUEVO: mismo contrato que NegocioPendienteDto de la API
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Horario { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public List<string> Fotos { get; set; } = new();
        public List<string> Etiquetas { get; set; } = new();
        public string EmailPropietario { get; set; } = string.Empty;
        public int NegociosAprobadosPropietario { get; set; }
    }


    public class CategoriaDto
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Icono { get; set; }
    }
}

