using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TurismoConecta.web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


// ─────────────────────────────────────────────────────────────
// 1. Dirección de la API: se lee UNA vez de wwwroot/appsettings.json
// ─────────────────────────────────────────────────────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Falta 'ApiBaseUrl' en wwwroot/appsettings.json");
var apiUri = new Uri(apiBaseUrl);

// 2. ApiConfig: las páginas lo usan para armar URLs de imágenes
builder.Services.AddSingleton(new ApiConfig(apiBaseUrl));

// 3. HttpClient "suelto": lo usan Login, Register, Perfil, etc. con @inject HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiUri });

// 4. Handler que agrega el token JWT a cada petición
builder.Services.AddTransient<AuthorizationMessageHandler>();

// 5. Servicios tipados: todos con la misma dirección y el token
void RegistrarApi<TServicio>() where TServicio : class =>
    builder.Services.AddHttpClient<TServicio>(c => c.BaseAddress = apiUri)
                    .AddHttpMessageHandler<AuthorizationMessageHandler>();

RegistrarApi<MunicipioApiService>();
RegistrarApi<ItinerarioApiService>();
RegistrarApi<EtiquetaApiService>();
RegistrarApi<SitioTuristicoApiService>();
RegistrarApi<AsistenteApiService>();
RegistrarApi<NegocioApiService>();
RegistrarApi<UsuarioApiService>();
RegistrarApi<NotificacionApiService>();


//Servicios clave para que <AuthorizeView> no se congele:
builder.Services.AddScoped<NotificacionTiempoRealService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

await builder.Build().RunAsync();
