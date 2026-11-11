using TurismoConecta.web.Client.Constants;


namespace TurismoConecta.web.Client.Models
{
    public class UsuarioAdminItemDto
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

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    }

    public class RolInfo
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class CambioRolRequest
    {
        public int IdUsuario { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }

    public class UsuarioAdminEdicionRequest
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

    public class UsuarioAdminCrearRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Rol { get; set; } = Roles.Usuario;
        public int? MunicipioAsignadoId { get; set; }
    }
}
