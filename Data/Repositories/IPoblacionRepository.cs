using ReniPressApp.Models;

namespace ReniPressApp.Data.Repositories
{
    public interface IPoblacionRepository
    {
        /// <summary>Años disponibles como columnas en dbo.poblacion (2014-2024).</summary>
        bool EsAnioValido(int anio);

        /// <summary>Población por departamento para el año indicado, ordenada de mayor a menor.</summary>
        List<Poblacion> ObtenerPorAnio(int anio);
    }
}
