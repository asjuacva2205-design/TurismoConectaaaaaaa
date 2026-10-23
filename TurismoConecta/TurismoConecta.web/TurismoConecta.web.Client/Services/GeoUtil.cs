namespace TurismoConecta.web.Client.Services
{
    /// <summary>Cálculos geográficos para el modo viaje (distancia, rumbo y lado relativo).</summary>
    public static class GeoUtil
    {
        private const double RadioTierraKm = 6371.0088;

        public enum Lado { Adelante, Derecha, Izquierda, Atras }

        /// <summary>Distancia en km entre dos puntos (fórmula de Haversine).</summary>
        public static double DistanciaKm(double lat1, double lng1, double lat2, double lng2)
        {
            var dLat = ARad(lat2 - lat1);
            var dLng = ARad(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ARad(lat1)) * Math.Cos(ARad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return 2 * RadioTierraKm * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        }

        /// <summary>Rumbo inicial (0–360°, 0 = norte, 90 = este) para ir del punto 1 al punto 2.</summary>
        public static double Rumbo(double lat1, double lng1, double lat2, double lng2)
        {
            var phi1 = ARad(lat1);
            var phi2 = ARad(lat2);
            var dLng = ARad(lng2 - lng1);

            var y = Math.Sin(dLng) * Math.Cos(phi2);
            var x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(dLng);

            return (ADeg(Math.Atan2(y, x)) + 360) % 360;
        }

        /// <summary>
        /// Dónde queda un punto respecto a hacia dónde te mueves.
        /// rumboViaje = tu dirección de marcha; rumboAlPunto = rumbo desde ti hacia el punto.
        /// </summary>
        public static Lado LadoRelativo(double rumboViaje, double rumboAlPunto)
        {
            var relativo = ((rumboAlPunto - rumboViaje + 540) % 360) - 180; // -180..180 (positivo = derecha)
            var abs = Math.Abs(relativo);

            if (abs <= 25) return Lado.Adelante;
            if (abs > 120) return Lado.Atras;
            return relativo > 0 ? Lado.Derecha : Lado.Izquierda;
        }

        private static double ARad(double grados) => grados * Math.PI / 180;
        private static double ADeg(double radianes) => radianes * 180 / Math.PI;
    }
}
