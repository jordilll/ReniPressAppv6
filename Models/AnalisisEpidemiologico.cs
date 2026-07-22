namespace ReniPressApp.Models
{
    /// <summary>
    /// Fila del Módulo de Análisis Epidemiológico: combina los casos de IRA
    /// de un departamento con su población para un año determinado y calcula
    /// la tasa de incidencia y el nivel de riesgo asociado.
    /// </summary>
    public class AnalisisEpidemiologico
    {
        public int Posicion { get; set; }
        public string Ubigeo { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public int CasosIRA { get; set; }
        public long Poblacion { get; set; }

        /// <summary>Tasa de incidencia por cada 100 000 habitantes: (CasosIRA / Poblacion) * 100000.</summary>
        public decimal Tasa { get; set; }

        /// <summary>Nivel de riesgo: "Muy Alto", "Alto", "Medio" o "Bajo".</summary>
        public string Nivel { get; set; } = string.Empty;

        /// <summary>Ícono/emoji asociado al nivel de riesgo (🔴 🟠 🟡 🟢).</summary>
        public string NivelIcono { get; set; } = string.Empty;
    }
}
