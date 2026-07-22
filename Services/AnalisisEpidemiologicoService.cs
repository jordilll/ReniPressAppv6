using System.Globalization;
using System.Text;
using ReniPressApp.Data.Repositories;
using ReniPressApp.Models;

namespace ReniPressApp.Services
{
    /// <summary>
    /// Convierte los datos crudos de Población e IRA en un Análisis Epidemiológico:
    /// tasa de incidencia por 100 000 habitantes, nivel de riesgo y ranking.
    /// La población deja de ser un dato aislado y pasa a ser el denominador que le
    /// da contexto de riesgo a los casos de IRA.
    /// </summary>
    public class AnalisisEpidemiologicoService : IAnalisisEpidemiologicoService
    {
        private readonly IPoblacionRepository _poblacionRepo;
        private readonly IIraRepository _iraRepo;

        // Umbrales de clasificación de riesgo (tasa por 100 000 habitantes).
        private const decimal UmbralMuyAlto = 2000m;
        private const decimal UmbralAlto    = 1500m;
        private const decimal UmbralMedio   = 1000m;

        public AnalisisEpidemiologicoService(IPoblacionRepository poblacionRepo, IIraRepository iraRepo)
        {
            _poblacionRepo = poblacionRepo;
            _iraRepo = iraRepo;
        }

        public AnalisisEpidemiologicoResultado ObtenerAnalisis(int anio)
        {
            var poblacion = _poblacionRepo.ObtenerPorAnio(anio);
            var casos     = _iraRepo.ObtenerCasosPorAnio(anio);

            // Diccionario de casos IRA indexado por nombre de departamento normalizado,
            // para poder cruzarlo con población (bases de datos distintas -> se cruza por LINQ, no por JOIN SQL).
            var casosPorDepartamento = casos
                .GroupBy(c => Normalizar(c.Departamento))
                .ToDictionary(g => g.Key, g => g.Sum(c => c.TotalCasos));

            // La población es la base del universo de departamentos: si un departamento
            // no registra casos de IRA en el año consultado, se muestra con 0 casos y tasa 0,
            // en vez de desaparecer del análisis.
            var detalle = poblacion
                .Select(p =>
                {
                    var clave = Normalizar(p.Departamento);
                    var casosIra = casosPorDepartamento.TryGetValue(clave, out var c) ? c : 0;
                    var tasa = p.TotalPoblacion > 0
                        ? Math.Round(casosIra * 100000m / p.TotalPoblacion, 2)
                        : 0m;
                    var (nivel, icono) = ClasificarRiesgo(tasa);

                    return new AnalisisEpidemiologico
                    {
                        Ubigeo       = p.Ubigeo,
                        Departamento = p.Departamento,
                        CasosIRA     = casosIra,
                        Poblacion    = p.TotalPoblacion,
                        Tasa         = tasa,
                        Nivel        = nivel,
                        NivelIcono   = icono
                    };
                })
                .OrderByDescending(d => d.Tasa)
                .ToList();

            for (int i = 0; i < detalle.Count; i++)
                detalle[i].Posicion = i + 1;

            var resumen = ConstruirResumen(anio, detalle);

            return new AnalisisEpidemiologicoResultado { Detalle = detalle, Resumen = resumen };
        }

        private static ResumenEpidemiologico ConstruirResumen(int anio, List<AnalisisEpidemiologico> detalle)
        {
            var totalCasos      = detalle.Sum(d => d.CasosIRA);
            var totalPoblacion  = detalle.Sum(d => d.Poblacion);
            var tasaNacional    = totalPoblacion > 0
                ? Math.Round(totalCasos * 100000m / totalPoblacion, 2)
                : 0m;

            var mayorRiesgo = detalle.Count > 0 ? detalle[0] : null;                 // ya viene ordenado desc por tasa
            var menorRiesgo = detalle.Count > 0 ? detalle[^1] : null;

            return new ResumenEpidemiologico
            {
                Anio                     = anio,
                TotalDepartamentos       = detalle.Count,
                TotalCasos               = totalCasos,
                TotalPoblacion           = totalPoblacion,
                TasaNacional             = tasaNacional,

                DepartamentoMayorRiesgo  = mayorRiesgo?.Departamento ?? "—",
                TasaMayorRiesgo          = mayorRiesgo?.Tasa ?? 0m,
                CasosMayorRiesgo         = mayorRiesgo?.CasosIRA ?? 0,
                NivelMayorRiesgo         = mayorRiesgo?.Nivel ?? "—",

                DepartamentoMenorRiesgo  = menorRiesgo?.Departamento ?? "—",
                TasaMenorRiesgo          = menorRiesgo?.Tasa ?? 0m,
                CasosMenorRiesgo         = menorRiesgo?.CasosIRA ?? 0,
                NivelMenorRiesgo         = menorRiesgo?.Nivel ?? "—"
            };
        }

        /// <summary>Clasifica una tasa por 100 000 habitantes en un nivel de riesgo con su ícono.</summary>
        private static (string Nivel, string Icono) ClasificarRiesgo(decimal tasa)
        {
            if (tasa > UmbralMuyAlto) return ("Muy Alto", "🔴");
            if (tasa >= UmbralAlto)   return ("Alto", "🟠");
            if (tasa >= UmbralMedio)  return ("Medio", "🟡");
            return ("Bajo", "🟢");
        }

        /// <summary>Quita tildes, pasa a mayúsculas y recorta espacios, para cruzar nombres de departamento entre bases distintas.</summary>
        private static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            var normalizado = texto.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (categoria != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // ==================== Módulo de IA ====================

        public string ResponderPregunta(string pregunta, int anio)
        {
            if (string.IsNullOrWhiteSpace(pregunta))
                return "Por favor, formula una pregunta sobre el riesgo epidemiológico de IRA.";

            var resultado = ObtenerAnalisis(anio);
            var detalle = resultado.Detalle;
            var resumen = resultado.Resumen;

            if (detalle.Count == 0)
                return $"No hay datos de población y/o casos de IRA disponibles para el año {anio}.";

            var q = Normalizar(pregunta);

            // 1) Comparar dos departamentos (si se detectan dos nombres de la lista en la pregunta)
            var mencionados = detalle
                .Where(d => q.Contains(Normalizar(d.Departamento)))
                .DistinctBy(d => d.Departamento)
                .ToList();

            if (q.Contains("COMPARA") && mencionados.Count >= 2)
            {
                var a = mencionados[0];
                var b = mencionados[1];
                return $"Comparación epidemiológica {anio}: " +
                       $"{a.Departamento} registra {a.CasosIRA:N0} casos de IRA con una población de {a.Poblacion:N0} " +
                       $"habitantes, lo que da una tasa de {a.Tasa:N2} por cada 100 000 habitantes (riesgo {a.Nivel}). " +
                       $"{b.Departamento} registra {b.CasosIRA:N0} casos con una población de {b.Poblacion:N0} " +
                       $"habitantes, con una tasa de {b.Tasa:N2} por cada 100 000 habitantes (riesgo {b.Nivel}). " +
                       $"En términos de riesgo relativo a su población, {(a.Tasa >= b.Tasa ? a.Departamento : b.Departamento)} " +
                       $"presenta mayor riesgo epidemiológico que {(a.Tasa >= b.Tasa ? b.Departamento : a.Departamento)}.";
            }

            // 2) Departamentos con nivel "Muy Alto"
            if (q.Contains("MUY ALTO"))
            {
                var muyAltos = detalle.Where(d => d.Nivel == "Muy Alto").ToList();
                if (muyAltos.Count == 0)
                    return $"Para el año {anio}, ningún departamento presenta un nivel de riesgo Muy Alto (tasa mayor a 2000 por 100 000 habitantes).";

                var lista = string.Join(", ", muyAltos.Select(d => $"{d.Departamento} ({d.Tasa:N2})"));
                return $"En {anio}, los departamentos con riesgo Muy Alto 🔴 (tasa mayor a 2000 por 100 000 habitantes) son: {lista}.";
            }

            // 3) Departamento con muchos casos pero baja tasa (alto en casos, bajo en riesgo relativo)
            if ((q.Contains("MUCHOS CASOS") || q.Contains("MAS CASOS")) &&
                (q.Contains("BAJA TASA") || q.Contains("BAJO RIESGO") || q.Contains("POCA TASA")))
            {
                var porCasos = detalle.OrderByDescending(d => d.CasosIRA).ToList();
                AnalisisEpidemiologico? candidato = null;
                var mayorDesfase = int.MinValue;
                for (int i = 0; i < porCasos.Count; i++)
                {
                    var rankCasos = i + 1;              // 1 = más casos
                    var rankTasa  = porCasos[i].Posicion; // 1 = mayor tasa
                    var desfase = rankTasa - rankCasos;   // grande = muchos casos pero tasa/riesgo bajo
                    if (desfase > mayorDesfase)
                    {
                        mayorDesfase = desfase;
                        candidato = porCasos[i];
                    }
                }

                if (candidato == null)
                    return "No fue posible determinar un departamento con muchos casos y baja tasa para el año consultado.";

                return $"En {anio}, {candidato.Departamento} es el caso más representativo de 'muchos casos, baja tasa': " +
                       $"registra {candidato.CasosIRA:N0} casos de IRA, pero al tener una población de {candidato.Poblacion:N0} " +
                       $"habitantes su tasa es de solo {candidato.Tasa:N2} por 100 000 habitantes (riesgo {candidato.Nivel}). " +
                       $"Esto muestra que el volumen de casos por sí solo no refleja el nivel real de riesgo epidemiológico.";
            }

            // 4) Mayor tasa / mayor riesgo considerando población
            if (q.Contains("MAYOR TASA") || q.Contains("MAYOR RIESGO") || q.Contains("MAS RIESGO"))
            {
                var top = detalle[0];
                return $"El departamento con mayor riesgo de IRA en {anio}, considerando su población, es {top.Departamento}: " +
                       $"{top.Tasa:N2} casos por cada 100 000 habitantes ({top.CasosIRA:N0} casos sobre una población de " +
                       $"{top.Poblacion:N0} habitantes), con un nivel de riesgo {top.Nivel} {top.NivelIcono}.";
            }

            // 5) Menor tasa / menor riesgo
            if (q.Contains("MENOR TASA") || q.Contains("MENOR RIESGO"))
            {
                var bottom = detalle[^1];
                return $"El departamento con menor riesgo de IRA en {anio}, considerando su población, es {bottom.Departamento}: " +
                       $"{bottom.Tasa:N2} casos por cada 100 000 habitantes ({bottom.CasosIRA:N0} casos sobre una población de " +
                       $"{bottom.Poblacion:N0} habitantes), con un nivel de riesgo {bottom.Nivel} {bottom.NivelIcono}.";
            }

            // 6) Pregunta sobre un departamento puntual (sin ser una comparación)
            if (mencionados.Count == 1)
            {
                var d = mencionados[0];
                return $"{d.Departamento} ({anio}): {d.CasosIRA:N0} casos de IRA, población de {d.Poblacion:N0} habitantes, " +
                       $"tasa de {d.Tasa:N2} por cada 100 000 habitantes, nivel de riesgo {d.Nivel} {d.NivelIcono} " +
                       $"(posición {d.Posicion} de {detalle.Count} en el ranking de riesgo).";
            }

            // 7) Respuesta general por defecto: panorama nacional
            return $"Panorama epidemiológico de IRA {anio}: tasa nacional de {resumen.TasaNacional:N2} casos por 100 000 " +
                   $"habitantes ({resumen.TotalCasos:N0} casos, {resumen.TotalPoblacion:N0} habitantes). " +
                   $"Mayor riesgo: {resumen.DepartamentoMayorRiesgo} ({resumen.TasaMayorRiesgo:N2} por 100 000, nivel {resumen.NivelMayorRiesgo}). " +
                   $"Menor riesgo: {resumen.DepartamentoMenorRiesgo} ({resumen.TasaMenorRiesgo:N2} por 100 000, nivel {resumen.NivelMenorRiesgo}). " +
                   $"Puedes preguntar por un departamento específico, comparar dos departamentos, o consultar los de riesgo Muy Alto.";
        }
    }
}
