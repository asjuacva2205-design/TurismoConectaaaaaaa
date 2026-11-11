namespace TurismoConecta.api.Constants;

public static class ReglasPassword
{
    // Mínimo 8 caracteres, al menos una letra y al menos un número.
    public const string Patron = @"^(?=.*[A-Za-z])(?=.*\d).{8,}$";

    public const string Mensaje =
        "La contraseña debe tener al menos 8 caracteres e incluir letras y números.";
}
