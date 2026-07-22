using Microsoft.Data.SqlClient;
using ReniPressApp.Models;

namespace ReniPressApp.Data.Repositories
{
    /// <summary>
    /// Acceso a datos de IRAestadisticas. Encapsula la consulta que antes vivía en
    /// IraController, sin cambiar su comportamiento.
    /// </summary>
    public class IraRepository : IIraRepository
    {
        private readonly Conexion _conexion;

        public IraRepository(Conexion conexion)
        {
            _conexion = conexion;
        }

        public List<EstadisticaIRA> ObtenerCasosPorAnio(int anio)
        {
            var lista = new List<EstadisticaIRA>();

            using var conn = _conexion.ObtenerConexionSalud();
            conn.Open();

            var query = @"
                SELECT
                    Departamento,
                    SUM(CasosIRA) AS TotalCasos,
                    CAST(
                        SUM(CasosIRA)*100.0/
                        (
                            SELECT SUM(CasosIRA)
                            FROM IRAestadisticas
                            WHERE Año=@anio
                        )
                    AS DECIMAL(10,2)) AS Porcentaje
                FROM IRAestadisticas
                WHERE Año=@anio
                GROUP BY Departamento
                ORDER BY TotalCasos DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@anio", anio);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new EstadisticaIRA
                {
                    Departamento = reader["Departamento"]?.ToString() ?? "",
                    TotalCasos   = Convert.ToInt32(reader["TotalCasos"]),
                    Porcentaje   = Convert.ToDecimal(reader["Porcentaje"])
                });
            }

            return lista;
        }
    }
}
