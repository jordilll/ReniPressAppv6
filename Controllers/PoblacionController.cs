using Microsoft.AspNetCore.Mvc;
using ReniPressApp.Data.Repositories;
using ReniPressApp.Services;

namespace ReniPressApp.Controllers
{
    public class PoblacionController : Controller
    {
        private readonly IPoblacionRepository _poblacionRepo;
        private readonly IAnalisisEpidemiologicoService _analisisService;

        public PoblacionController(IPoblacionRepository poblacionRepo, IAnalisisEpidemiologicoService analisisService)
        {
            _poblacionRepo = poblacionRepo;
            _analisisService = analisisService;
        }

        [HttpGet]
        public IActionResult Estadisticas()
        {
            return View();
        }

        /// <summary>
        /// Endpoint original de Población (se mantiene sin cambios de contrato para no
        /// romper integraciones existentes que ya lo consuman).
        /// </summary>
        [HttpGet]
        public JsonResult ObtenerPoblacionPorDepartamento(int anio)
        {
            if (!_poblacionRepo.EsAnioValido(anio))
            {
                return Json(new { error = $"El año {anio} no es válido. Debe estar entre 2014 y 2024." });
            }

            try
            {
                var lista = _poblacionRepo.ObtenerPorAnio(anio);
                return Json(lista);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Módulo de Análisis Epidemiológico: cruza población y casos de IRA para el año
        /// indicado, y devuelve el detalle por departamento (tasa, nivel de riesgo, ranking)
        /// junto con los KPIs nacionales.
        /// </summary>
        [HttpGet]
        public JsonResult ObtenerAnalisisEpidemiologico(int anio)
        {
            if (!_poblacionRepo.EsAnioValido(anio))
            {
                return Json(new { error = $"El año {anio} no es válido. Debe estar entre 2014 y 2024." });
            }

            try
            {
                var resultado = _analisisService.ObtenerAnalisis(anio);
                return Json(new { detalle = resultado.Detalle, resumen = resultado.Resumen });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint de soporte para el módulo de IA: responde preguntas sobre riesgo
        /// epidemiológico de IRA usando la información combinada de población y casos.
        /// </summary>
        [HttpGet]
        public JsonResult ObtenerRespuestaIA(string pregunta, int anio)
        {
            if (!_poblacionRepo.EsAnioValido(anio))
            {
                return Json(new { error = $"El año {anio} no es válido. Debe estar entre 2014 y 2024." });
            }

            try
            {
                var respuesta = _analisisService.ResponderPregunta(pregunta, anio);
                return Json(new { respuesta });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
