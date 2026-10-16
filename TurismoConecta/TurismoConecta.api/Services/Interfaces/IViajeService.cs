using TurismoConecta.api.DTOs.Viaje;

namespace TurismoConecta.api.Services.Interfaces;

public interface IViajeService
{
    Task<List<PuebloViajeDto>> ObtenerPueblosAsync(CancellationToken ct = default);
}
