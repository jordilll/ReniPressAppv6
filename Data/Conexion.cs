using Microsoft.Data.SqlClient;

namespace ReniPressApp.Data
{
    public class Conexion
    {
        private readonly string _connHospitales;
        private readonly string _connSalud;
        private readonly string _connPoblacion;

        public Conexion(IConfiguration configuration)
        {
            _connHospitales = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");
            _connSalud = configuration.GetConnectionString("SaludConnection")
                ?? throw new InvalidOperationException("Cadena de conexión 'SaludConnection' no encontrada.");
            _connPoblacion = configuration.GetConnectionString("PoblacionConnection")
                ?? throw new InvalidOperationException("Cadena de conexión 'PoblacionConnection' no encontrada.");
        }

        public SqlConnection ObtenerConexion() => new SqlConnection(_connHospitales);

        public SqlConnection ObtenerConexionHospitales() => new SqlConnection(_connHospitales);

        public SqlConnection ObtenerConexionSalud() => new SqlConnection(_connSalud);

        public SqlConnection ObtenerConexionPoblacion() => new SqlConnection(_connPoblacion);
    }
}
