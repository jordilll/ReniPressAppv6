namespace ReniPressApp.Models
{
    public class Poblacion
    {
        public string Ubigeo { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public long TotalPoblacion { get; set; }
        public decimal Porcentaje { get; set; }
    }
}
