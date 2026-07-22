namespace ReniPressApp.Models
{
    /// <summary>
    /// Indicadores agregados (KPIs) del Módulo de Análisis Epidemiológico
    /// para un año determinado: tasa nacional y departamentos de mayor/menor riesgo.
    /// </summary>
    public class ResumenEpidemiologico
    {
        public int Anio { get; set; }
        public int TotalDepartamentos { get; set; }
        public int TotalCasos { get; set; }
        public long TotalPoblacion { get; set; }

        /// <summary>Casos por cada 100 000 habitantes a nivel nacional.</summary>
        public decimal TasaNacional { get; set; }

        public string DepartamentoMayorRiesgo { get; set; } = string.Empty;
        public decimal TasaMayorRiesgo { get; set; }
        public int CasosMayorRiesgo { get; set; }
        public string NivelMayorRiesgo { get; set; } = string.Empty;

        public string DepartamentoMenorRiesgo { get; set; } = string.Empty;
        public decimal TasaMenorRiesgo { get; set; }
        public int CasosMenorRiesgo { get; set; }
        public string NivelMenorRiesgo { get; set; } = string.Empty;
    }
}
