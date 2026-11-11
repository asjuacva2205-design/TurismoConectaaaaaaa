namespace TurismoConecta.api.DTOs.Viaje;

/// Ten en cuenta esto Juliana de por Dios 
/// Resumen de un municipio para el modo viaje: dónde queda y qué ofrece,
/// listo para ser narrado ("a tu mano derecha encontrarás X, que ofrece ...").

public class PuebloViajeDto
{
    public int IdMunicipio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public string Ofrece { get; set; } = string.Empty;
}
