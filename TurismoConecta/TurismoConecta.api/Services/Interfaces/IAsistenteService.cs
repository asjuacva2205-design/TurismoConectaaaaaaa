using TurismoConecta.api.DTOs.Asistente;

namespace TurismoConecta.api.Services.Interfaces;

public interface IAsistenteService
{
    Task<string> ResponderAsync(string mensaje, List<TurnoChatDto> historial, string? contexto, CancellationToken ct = default);
}
