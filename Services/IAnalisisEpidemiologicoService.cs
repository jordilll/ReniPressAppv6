using ReniPressApp.Models;

namespace ReniPressApp.Services
{
    public class AnalisisEpidemiologicoResultado
    {
        public List<AnalisisEpidemiologico> Detalle { get; set; } = new();
        public ResumenEpidemiologico Resumen { get; set; } = new();
    }

    public interface IAnalisisEpidemiologicoService
    {
        /// <summary>
        /// Cruza población (dbo.poblacion) con casos de IRA (IRAestadisticas) para el año
        /// indicado, calcula la tasa de incidencia por 100 000 habitantes, el nivel de
        /// riesgo y arma el ranking ordenado de mayor a menor riesgo.
        /// </summary>
        AnalisisEpidemiologicoResultado ObtenerAnalisis(int anio);

        /// <summary>
        /// Responde preguntas en lenguaje natural sobre el riesgo epidemiológico de IRA
        /// usando la información combinada de población y casos (para el módulo de IA).
        /// </summary>
        string ResponderPregunta(string pregunta, int anio);
    }
}
