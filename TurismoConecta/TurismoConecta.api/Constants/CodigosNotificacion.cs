namespace TurismoConecta.api.Constants
{
    
    /// Códigos de la tabla TiposNotificacion. 
    public static class CodigosNotificacion
    {
        // Ya existen en la BD
        public const string FavoritoNuevo = "FAV_NUEVO";
        public const string InfoGeneral = "INFO_GENERAL";
        public const string PromoNegocio = "PROMO_NEGOCIO";

        // HU-31 (creados en la Fase 1)
        public const string NegocioAprobado = "NEGOCIO_APROBADO";
        public const string NegocioRechazado = "NEGOCIO_RECHAZADO";
        public const string NegocioPendiente = "NEGOCIO_PENDIENTE";

        // HU-28 / HU-29 (NotificacionService crea el tipo en la BD si aún no existe)
        public const string ReseñaNueva = "RESENA_NUEVA";
        public const string ReseñaRespondida = "RESENA_RESPONDIDA";

    }
}
