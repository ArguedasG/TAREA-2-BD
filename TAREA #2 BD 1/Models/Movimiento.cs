namespace TAREA__2_BD_1.Models
{
    public class Movimiento
    {
        public DateTime FechaMovimiento { get; set; }
        public string NombreTipoMovimiento { get; set; }
        public decimal Monto { get; set; }
        public decimal NuevoSaldo { get; set; }
        public string NombreUsuario { get; set; }
        public string IP { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
    }
}
