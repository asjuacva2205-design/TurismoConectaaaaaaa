using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TurismoConecta.web.Client.Services
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;

        public JwtAuthStateProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token = null;

            try
            {
                token = await _js.InvokeAsync<string?>("localStorage.getItem", "authToken");
            }
            catch (InvalidOperationException)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = jwt.Claims.ToList();

                // Asegurar compatibilidad de tipos de claim (role y ClaimTypes.Role)
                var roleClaims = claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role).ToList();
                foreach (var rc in roleClaims)
                {
                    if (rc.Type != ClaimTypes.Role)
                        claims.Add(new Claim(ClaimTypes.Role, rc.Value));
                    if (rc.Type != "role")
                        claims.Add(new Claim("role", rc.Value));

             
                }

                // Asegurar compatibilidad de Name
                var nameClaim = claims.FirstOrDefault(c => c.Type == "name" || c.Type == "unique_name" || c.Type == ClaimTypes.Name);
                if (nameClaim != null && nameClaim.Type != ClaimTypes.Name)
                {
                    claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
                }

                var distinctClaims = claims.DistinctBy(c => $"{c.Type}:{c.Value}").ToList();
                var identity = new ClaimsIdentity(distinctClaims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotificarCambioAutenticacion()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
