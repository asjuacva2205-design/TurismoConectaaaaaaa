using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Auth;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Constants;
using Microsoft.Extensions.Configuration;

namespace TurismoConecta.api.Services;


public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Usuario> _hasher = new();

    public AuthService(AppDbContext db, JwtService jwt, IEmailService emailService, IConfiguration configuration)
    {
        _db = db;
        _jwt = jwt;
        _emailService = emailService;
        _configuration = configuration;
    } 

    public async Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto dto)
    {
        bool emailExiste = await _db.Usuarios.AnyAsync(u => u.Email == dto.Email);
        if (emailExiste) throw new InvalidOperationException("El email ya está registrado.");

        var rolUsuario = await _db.Rols.FirstOrDefaultAsync(r => r.Nombre == Roles.Usuario)
            ?? throw new InvalidOperationException("Rol 'Usuario' no encontrado.");

        var nuevoUsuario = new Usuario
        {
            Nombre           = dto.Nombre,
            Apellido         = dto.Apellido,
            Email            = dto.Email,
            Telefono         = dto.Telefono,
            IdRol            = rolUsuario.IdRol,
            FechaRegistro    = DateTime.UtcNow,
            EmailConfirmado  = false,
            Activo           = true,
            PasswordHash     = string.Empty
        };

        nuevoUsuario.PasswordHash = _hasher.HashPassword(nuevoUsuario, dto.Password);

        _db.Usuarios.Add(nuevoUsuario);
        await _db.SaveChangesAsync();

        nuevoUsuario.IdRolNavigation = rolUsuario;

        var (token, expira) = _jwt.GenerarToken(nuevoUsuario);
        return new AuthResponseDto
        {
            Token         = token,
            Expira        = expira,
            IdUsuario     = nuevoUsuario.IdUsuario,
            NombreCompleto = $"{nuevoUsuario.Nombre} {nuevoUsuario.Apellido}",
            Email         = nuevoUsuario.Email,
            Rol           = rolUsuario.Nombre
        };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null)
            return;

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        usuario.PasswordResetToken = token;
        usuario.PasswordResetExpira = DateTime.UtcNow.AddMinutes(30);
        await _db.SaveChangesAsync();

        var frontendUrl = _configuration["App:FrontendUrl"]?.TrimEnd('/')
    ?? throw new InvalidOperationException("Falta 'App:FrontendUrl' en appsettings.json");
        var enlace = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(dto.Email)}&token={token}";
        var cuerpo = $@"
        <h3>Recuperación de contraseña - TurismoConecta</h3>
        <p>Haz clic en el siguiente enlace para restablecer tu contraseña. Este enlace expira en 30 minutos.</p>
        <p><a href='{enlace}'>Restablecer contraseña</a></p>";

        await _emailService.EnviarCorreoAsync(dto.Email, "Recupera tu contraseña", cuerpo);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.PasswordResetToken == dto.Token);

        if (usuario == null || usuario.PasswordResetExpira == null || usuario.PasswordResetExpira < DateTime.UtcNow)
            throw new InvalidOperationException("El enlace de recuperación no es válido o ya expiró.");

        usuario.PasswordHash = _hasher.HashPassword(usuario, dto.NuevaPassword);
        usuario.PasswordResetToken = null;
        usuario.PasswordResetExpira = null;

        await _db.SaveChangesAsync();
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var usuario = await _db.Usuarios.Include(u => u.IdRolNavigation).FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (usuario == null || !usuario.Activo) throw new UnauthorizedAccessException("Credenciales inválidas.");

        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password);
        if (resultado == PasswordVerificationResult.Failed) throw new UnauthorizedAccessException("Credenciales inválidas.");

        var (token, expira) = _jwt.GenerarToken(usuario);
        return new AuthResponseDto
        {
            Token          = token,
            Expira         = expira,
            IdUsuario      = usuario.IdUsuario,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
            Email          = usuario.Email,
            Rol            = usuario.IdRolNavigation.Nombre
        };
    }
}
