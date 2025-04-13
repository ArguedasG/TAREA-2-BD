namespace TAREA__2_BD_1.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string ValorDocumentoIdentidad { get; set; }
        public string Nombre { get; set; }
        public int IdPuesto { get; set; }
        public string NombrePuesto { get; set; } // Para mostrar en la UI
        public DateTime FechaContratacion { get; set; }
        public decimal SaldoVacaciones { get; set; }
        public bool EsActivo { get; set; }
    }
}
