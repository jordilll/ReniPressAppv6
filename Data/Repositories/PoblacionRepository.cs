using Microsoft.Data.SqlClient;
using ReniPressApp.Models;

namespace ReniPressApp.Data.Repositories
{
    /// <summary>
    /// Acceso a datos de dbo.poblacion. Encapsula la consulta que antes vivía en
    /// PoblacionController, sin cambiar su comportamiento ni su forma de armar el SQL.
    /// </summary>
    public class PoblacionRepository : IPoblacionRepository
    {
        private readonly Conexion _conexion;

        // Whitelist de años permitidos (coinciden con las columnas de dbo.poblacion).
        // Se usa para armar el nombre de columna de forma segura (no se puede parametrizar
        // un nombre de columna con SqlParameter).
        private static readonly HashSet<int> AniosPermitidos = new()
        {
            2014, 2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023, 2024
        };

        public PoblacionRepository(Conexion conexion)
        {
            _conexion = conexion;
        }

        public bool EsAnioValido(int anio) => AniosPermitidos.Contains(anio);

        public List<Poblacion> ObtenerPorAnio(int anio)
        {
            if (!EsAnioValido(anio))
                throw new ArgumentException($"El año {anio} no es válido. Debe estar entre 2014 y 2024.");

            var lista = new List<Poblacion>();

            using var conn = _conexion.ObtenerConexionPoblacion();
            conn.Open();

            // El nombre de la columna del año se valida contra AniosPermitidos antes de
            // concatenarse, por lo que no existe riesgo de inyección SQL.
            var query = $@"
                SELECT
                    UBIGEO,
                    DEPARTAMENTO,
                    [{anio}] AS TotalPoblacion,
                    CAST(
                        [{anio}] * 100.0 /
                        (SELECT SUM([{anio}]) FROM dbo.poblacion)
                    AS DECIMAL(10,2)) AS Porcentaje
                FROM dbo.poblacion
                ORDER BY [{anio}] DESC";

            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Poblacion
                {
                    Ubigeo         = reader["UBIGEO"]?.ToString() ?? "",
                    Departamento   = reader["DEPARTAMENTO"]?.ToString() ?? "",
                    TotalPoblacion = Convert.ToInt64(reader["TotalPoblacion"]),
                    Porcentaje     = Convert.ToDecimal(reader["Porcentaje"])
                });
            }

            return lista;
        }
    }
}
