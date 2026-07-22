using Microsoft.AspNetCore.Mvc;
using ReniPressApp.Data.Repositories;

namespace ReniPressApp.Controllers
{
    public class IraController : Controller
    {
        private readonly IIraRepository _iraRepo;

        public IraController(IIraRepository iraRepo)
        {
            _iraRepo = iraRepo;
        }

        [HttpGet]
        public IActionResult Estadisticas()
        {
            return View();
        }

        [HttpGet]
        public JsonResult ObtenerCasosPorDepartamento(int anio)
        {
            try
            {
                var lista = _iraRepo.ObtenerCasosPorAnio(anio);
                return Json(lista);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
