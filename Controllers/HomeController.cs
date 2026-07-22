using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ReniPressApp.Data;
using ReniPressApp.Models;

namespace ReniPressApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly Conexion _conexion;

        public HomeController(Conexion conexion)
        {
            _conexion = conexion;
        }

        public IActionResult Index()
        {
            ViewBag.Departamentos = ObtenerDepartamentos();
            ViewBag.Provincias = new List<string>();
            ViewBag.Distritos = new List<string>();
            return View(new List<Establecimiento>());
        }

        [HttpPost]
        public IActionResult Buscar(string? nombre, string? departamento, string? provincia, string? distrito)
        {
            var resultados = new List<Establecimiento>();

            try
            {
                using var conn = _conexion.ObtenerConexion();
                conn.Open();

                var query = @"SELECT COD_IPRESS, INSTITUCION, NOMBRE, CLASIFICACION,
                                     DEPARTAMENTO, PROVINCIA, DISTRITO, UBIGEO
                              FROM [correcion2.00]
                              WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(nombre))
                    query += " AND (NOMBRE LIKE @nombre OR COD_IPRESS LIKE @nombre)";
                if (!string.IsNullOrWhiteSpace(departamento))
                    query += " AND DEPARTAMENTO = @departamento";
                if (!string.IsNullOrWhiteSpace(provincia))
                    query += " AND PROVINCIA = @provincia";
                if (!string.IsNullOrWhiteSpace(distrito))
                    query += " AND DISTRITO = @distrito";

                query += " ORDER BY NOMBRE";

                using var cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrWhiteSpace(nombre))
                    cmd.Parameters.AddWithValue("@nombre", $"%{nombre.Trim()}%");
                if (!string.IsNullOrWhiteSpace(departamento))
                    cmd.Parameters.AddWithValue("@departamento", departamento.Trim());
                if (!string.IsNullOrWhiteSpace(provincia))
                    cmd.Parameters.AddWithValue("@provincia", provincia.Trim());
                if (!string.IsNullOrWhiteSpace(distrito))
                    cmd.Parameters.AddWithValue("@distrito", distrito.Trim());

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    resultados.Add(new Establecimiento
                    {
                        COD_IPRESS    = reader["COD_IPRESS"]?.ToString()    ?? "",
                        INSTITUCION   = reader["INSTITUCION"]?.ToString()   ?? "",
                        NOMBRE        = reader["NOMBRE"]?.ToString()        ?? "",
                        CLASIFICACION = reader["CLASIFICACION"]?.ToString() ?? "",
                        DEPARTAMENTO  = reader["DEPARTAMENTO"]?.ToString()  ?? "",
                        PROVINCIA     = reader["PROVINCIA"]?.ToString()     ?? "",
                        DISTRITO      = reader["DISTRITO"]?.ToString()      ?? "",
                        UBIGEO        = reader["UBIGEO"]?.ToString()        ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al consultar la base de datos: {ex.Message}";
            }

            ViewBag.Departamentos = ObtenerDepartamentos();
            ViewBag.DepSeleccionado = departamento;
            ViewBag.ProvSeleccionada = provincia;
            ViewBag.DistSeleccionado = distrito;
            ViewBag.NombreBuscado = nombre;
            ViewBag.Provincias = string.IsNullOrWhiteSpace(departamento)
                ? new List<string>()
                : ObtenerProvincias(departamento);
            ViewBag.Distritos = string.IsNullOrWhiteSpace(provincia)
                ? new List<string>()
                : ObtenerDistritos(departamento!, provincia);

            ViewBag.TotalResultados = resultados.Count;
            return View("Index", resultados);
        }

        [HttpGet]
        public JsonResult ObtenerProvinciasPorDep(string departamento)
        {
            var provincias = ObtenerProvincias(departamento);
            return Json(provincias);
        }

        [HttpGet]
        public JsonResult ObtenerDistritosPorProv(string departamento, string provincia)
        {
            var distritos = ObtenerDistritos(departamento, provincia);
            return Json(distritos);
        }

        private List<string> ObtenerDepartamentos()
        {
            var lista = new List<string>();
            try
            {
                using var conn = _conexion.ObtenerConexion();
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT DISTINCT DEPARTAMENTO FROM [correcion2.00] WHERE DEPARTAMENTO IS NOT NULL ORDER BY DEPARTAMENTO",
                    conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(reader["DEPARTAMENTO"]?.ToString() ?? "");
            }
            catch { }
            return lista;
        }

        private List<string> ObtenerProvincias(string departamento)
        {
            var lista = new List<string>();
            try
            {
                using var conn = _conexion.ObtenerConexion();
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT DISTINCT PROVINCIA FROM [correcion2.00] WHERE DEPARTAMENTO = @dep AND PROVINCIA IS NOT NULL ORDER BY PROVINCIA",
                    conn);
                cmd.Parameters.AddWithValue("@dep", departamento);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(reader["PROVINCIA"]?.ToString() ?? "");
            }
            catch { }
            return lista;
        }

        private List<string> ObtenerDistritos(string departamento, string provincia)
        {
            var lista = new List<string>();
            try
            {
                using var conn = _conexion.ObtenerConexion();
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT DISTINCT DISTRITO FROM [correcion2.00] WHERE DEPARTAMENTO = @dep AND PROVINCIA = @prov AND DISTRITO IS NOT NULL ORDER BY DISTRITO",
                    conn);
                cmd.Parameters.AddWithValue("@dep", departamento);
                cmd.Parameters.AddWithValue("@prov", provincia);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(reader["DISTRITO"]?.ToString() ?? "");
            }
            catch { }
            return lista;
        }
    }
}
