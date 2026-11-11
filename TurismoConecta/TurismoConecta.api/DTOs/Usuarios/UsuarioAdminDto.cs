using System.ComponentModel.DataAnnotations;
using TurismoConecta.api.Constants;

namespace TurismoConecta.api.DTOs.Usuarios
{
    public class UsuarioAdminDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Rol { get; set; } = string.Empty;
        public int? MunicipioAsignadoId { get; set; }
        public string? NombreMunicipioAsignado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public string? FotoUrl { get; set; }
    }

    public class RolDto
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class UsuarioAdminEdicionDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Rol { get; set; } = string.Empty;
        public int? MunicipioAsignadoId { get; set; }
        public bool Activo { get; set; } = true;
        public string? FotoBase64 { get; set; }
        public bool EliminarFoto { get; set; }
    }

    public class UsuarioAdminCrearDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [RegularExpression(ReglasPassword.Patron, ErrorMessage = ReglasPassword.Mensaje)]
        public string Password { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Rol { get; set; } = string.Empty;

        public int? MunicipioAsignadoId { get; set; }
    }
}
