namespace TAREA__2_BD_1.Models
{
    public class DetalleMovimientos
    {
        public string ValorDocumentoIdentidad { get; set; }
        public string NombreEmpleado { get; set; }
        public decimal SaldoVacaciones { get; set; }
        public List<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }
}
