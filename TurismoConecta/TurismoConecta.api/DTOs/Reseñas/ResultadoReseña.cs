namespace TurismoConecta.api.DTOs.Reseñas
{
    /// <summary>Cómo terminó una operación sobre reseñas; el controlador lo traduce a un código HTTP.</summary>
    public enum ResultadoReseña
    {
        Ok,
        Invalida,       // 400
        NoEncontrada,   // 404
        SinPermiso      // 403
    }
}
