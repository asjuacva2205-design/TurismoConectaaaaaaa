using TurismoConecta.web.Client.Pages;
using TurismoConecta.web.Client.Services;
using TurismoConecta.web.Components;

var builder = WebApplication.CreateBuilder(args);

// Dirección de la API: se lee de appsettings.json (del servidor)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Falta 'ApiBaseUrl' en appsettings.json");
var apiUri = new Uri(apiBaseUrl);

builder.Services.AddSingleton(new ApiConfig(apiBaseUrl));
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiUri });
builder.Services.AddTransient<AuthorizationMessageHandler>();

void RegistrarApi<TServicio>() where TServicio : class =>
    builder.Services.AddHttpClient<TServicio>(c => c.BaseAddress = apiUri)
                    .AddHttpMessageHandler<AuthorizationMessageHandler>();

RegistrarApi<MunicipioApiService>();
RegistrarApi<NegocioApiService>();
RegistrarApi<UsuarioApiService>();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();


builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();


app.UseAuthorization();  

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TurismoConecta.web.Client._Imports).Assembly);

app.Run();
