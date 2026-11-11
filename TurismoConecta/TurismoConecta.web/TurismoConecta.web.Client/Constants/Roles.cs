namespace TurismoConecta.web.Client.Constants;

public static class Roles
{
    public const string AdminGeneral = "AdminGeneral";
    public const string AdminMunicipio = "AdminMunicipio";
    public const string AdminEstablecimiento = "AdminEstablecimiento";
    public const string Usuario = "Usuario";

    public const string GeneralOMunicipio = AdminGeneral + "," + AdminMunicipio;
    public const string GeneralOEstablecimiento = AdminGeneral + "," + AdminEstablecimiento;
}
