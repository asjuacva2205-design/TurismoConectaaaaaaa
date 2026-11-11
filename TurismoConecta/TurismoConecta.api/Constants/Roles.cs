namespace TurismoConecta.api.Constants;

public static class Roles
{
    
    public const string AdminGeneral = "AdminGeneral";
    public const string AdminMunicipio = "AdminMunicipio";
    public const string AdminEstablecimiento = "AdminEstablecimiento";
    public const string Usuario = "Usuario";

    // ── Combinaciones para [Authorize(Roles = ...)] ──
    public const string GeneralOMunicipio = AdminGeneral + "," + AdminMunicipio;
    public const string GeneralOEstablecimiento = AdminGeneral + "," + AdminEstablecimiento;
    public const string TodosLosAdmins = AdminGeneral + "," + AdminMunicipio + "," + AdminEstablecimiento;
}
