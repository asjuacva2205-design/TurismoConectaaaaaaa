using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Usuarios;
using TurismoConecta.api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using TurismoConecta.api.Constants;
using TurismoConecta.api.Models;

namespace TurismoConecta.api.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly PasswordHasher<Usuario> _hasher = new();


        public UsuarioService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        
        public async Task<bool> AsignarRolAsync(AssignRoleRequestDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == dto.IdUsuario);

            if (usuario == null)
                return false;

            var rolNuevo = await _context.Rols
                .FirstOrDefaultAsync(r => r.Nombre == dto.NombreRol);

            if (rolNuevo == null)
                return false;

            usuario.IdRol = rolNuevo.IdRol;
            await _context.SaveChangesAsync();

            return true;
        }

        
        public async Task<PerfilResponseDto?> ObtenerPerfilAsync(int idUsuario)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
                return null;

            return new PerfilResponseDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Rol = usuario.IdRolNavigation.Nombre,
                FotoUrl = usuario.FotoUrl 
            };
        }

        
        public async Task<bool> ActualizarPerfilAsync(int idUsuario, ActualizarPerfilRequestDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
                return false;

            
            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Telefono = dto.Telefono;

            
            if (!string.IsNullOrEmpty(dto.FotoBase64))
            {
                try
                {
                   
                    var base64Data = dto.FotoBase64;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }

                    
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    
                    var folderPath = Path.Combine(_env.WebRootPath, "images", "perfiles");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    
                    var fileName = $"perfil_{usuario.IdUsuario}_{DateTime.UtcNow.Ticks}.jpg";
                    var filePath = Path.Combine(folderPath, fileName);

                    
                    if (!string.IsNullOrEmpty(usuario.FotoUrl))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, usuario.FotoUrl.TrimStart('/'));
                        if (File.Exists(oldPath)) File.Delete(oldPath);
                    }

                    
                    await File.WriteAllBytesAsync(filePath, imageBytes);

                    
                    usuario.FotoUrl = $"/images/perfiles/{fileName}";
                }
                catch (Exception)
                {
                    
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UsuarioAdminDto>> ListarUsuariosAsync()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Include(u => u.MunicipioAsignado)
                .OrderByDescending(u => u.FechaRegistro)
                .ToListAsync();

            return usuarios.Select(u => new UsuarioAdminDto
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                Telefono = u.Telefono,
                Rol = u.IdRolNavigation.Nombre,
                MunicipioAsignadoId = u.MunicipioAsignadoId,
                NombreMunicipioAsignado = u.MunicipioAsignado != null ? u.MunicipioAsignado.Nombre : null,
                FechaRegistro = u.FechaRegistro,
                Activo = u.Activo,
                FotoUrl = u.FotoUrl
            }).ToList();
        }

        public async Task<List<RolDto>> ListarRolesAsync()
        {
            var roles = await _context.Rols.ToListAsync();
            return roles.Select(r => new RolDto
            {
                IdRol = r.IdRol,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion
            }).ToList();
        }

        public async Task<bool> ActualizarUsuarioAdminAsync(int idUsuario, UsuarioAdminEdicionDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null) return false;

            usuario.Nombre = dto.Nombre.Trim();
            usuario.Apellido = dto.Apellido.Trim();
            usuario.Email = dto.Email.Trim().ToLowerInvariant();
            usuario.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            usuario.Activo = dto.Activo;
            usuario.MunicipioAsignadoId = dto.MunicipioAsignadoId;

            if (!string.IsNullOrWhiteSpace(dto.Rol))
            {
                var rol = await _context.Rols.FirstOrDefaultAsync(r => r.Nombre == dto.Rol);
                if (rol != null)
                {
                    usuario.IdRol = rol.IdRol;
                }
            }

            if (dto.EliminarFoto)
            {
                if (!string.IsNullOrEmpty(usuario.FotoUrl))
                {
                    try
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, usuario.FotoUrl.TrimStart('/'));
                        if (File.Exists(oldPath)) File.Delete(oldPath);
                    }
                    catch { }
                }
                usuario.FotoUrl = null;
            }
            else if (!string.IsNullOrWhiteSpace(dto.FotoBase64))
            {
                try
                {
                    var base64Data = dto.FotoBase64;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }

                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    var folderPath = Path.Combine(_env.WebRootPath, "images", "perfiles");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileName = $"perfil_{usuario.IdUsuario}_{DateTime.UtcNow.Ticks}.jpg";
                    var filePath = Path.Combine(folderPath, fileName);

                    if (!string.IsNullOrEmpty(usuario.FotoUrl))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, usuario.FotoUrl.TrimStart('/'));
                        if (File.Exists(oldPath)) File.Delete(oldPath);
                    }

                    await File.WriteAllBytesAsync(filePath, imageBytes);
                    usuario.FotoUrl = $"/images/perfiles/{fileName}";
                }
                catch { }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarUsuarioAsync(int idUsuario)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Favoritos)
                .Include(u => u.Notificacions)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null) return false;

            // Verificar dependencias con registros turísticos/comerciales
            bool tieneDependencias = await _context.Negocios.AnyAsync(n => n.IdUsuario == idUsuario)
                || await _context.Itinerarios.AnyAsync(i => i.IdUsuario == idUsuario)
                || await _context.Reseñas.AnyAsync(r => r.IdUsuario == idUsuario);

            if (tieneDependencias)
            {
                // Soft delete: Desactivar la cuenta para mantener la integridad histórica
                usuario.Activo = false;
                await _context.SaveChangesAsync();
                return true;
            }

            // Eliminación completa si no tiene dependencias críticas
            if (usuario.Favoritos.Any()) _context.Favoritos.RemoveRange(usuario.Favoritos);
            if (usuario.Notificacions.Any()) _context.Notificacions.RemoveRange(usuario.Notificacions);

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool exito, string? error)> CrearUsuarioAdminAsync(UsuarioAdminCrearDto dto)
        {
            // 1. Normalizar el correo para comparar y guardar siempre igual
            var email = dto.Email.Trim().ToLowerInvariant();

            // 2. El correo no puede repetirse
            if (await _context.Usuarios.AnyAsync(u => u.Email == email))
                return (false, "Ya existe una cuenta registrada con ese correo.");

            // 3. El rol debe existir en la tabla Rol
            var rol = await _context.Rols.FirstOrDefaultAsync(r => r.Nombre == dto.Rol);
            if (rol is null)
                return (false, $"El rol '{dto.Rol}' no existe.");

            // 4. Un AdminMunicipio necesita un municipio real asignado
            int? municipioAsignado = null;
            if (rol.Nombre == Roles.AdminMunicipio)
            {
                if (dto.MunicipioAsignadoId is null)
                    return (false, "Un administrador municipal debe tener un municipio asignado.");

                bool municipioExiste = await _context.Municipios
                    .AnyAsync(m => m.IdMunicipio == dto.MunicipioAsignadoId);
                if (!municipioExiste)
                    return (false, "El municipio seleccionado no existe.");

                municipioAsignado = dto.MunicipioAsignadoId;
            }

            // 5. Crear el usuario
            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Email = email,
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                IdRol = rol.IdRol,
                MunicipioAsignadoId = municipioAsignado,
                FechaRegistro = DateTime.UtcNow,
                EmailConfirmado = false,
                Activo = true,
                PasswordHash = string.Empty
            };

            // 6. Cifrar la contraseña (mismo método que AuthService → el login la reconocerá)
            usuario.PasswordHash = _hasher.HashPassword(usuario, dto.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> CambiarEstadoAsync(int idUsuario, bool activo)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null) return false;

            usuario.Activo = activo;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
