using ReniPressApp.Models;

namespace ReniPressApp.Data.Repositories
{
    public interface IIraRepository
    {
        /// <summary>Casos de IRA agrupados por departamento para el año indicado, ordenados de mayor a menor.</summary>
        List<EstadisticaIRA> ObtenerCasosPorAnio(int anio);
    }
}
