namespace TurismoConecta.api.DTOs.Negocios
{
    // KPIs del dashboard municipal: cuántos negocios hay en cada estado
    public class NegocioResumenDto
    {
        public int Aprobados { get; set; }
        public int Pendientes { get; set; }
        public int Rechazados { get; set; }
        public int Total => Aprobados + Pendientes + Rechazados;
    }
}
