namespace TAREA__2_BD_1.Models
{
    public class TipoMovimiento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string TipoAccion { get; set; } // "Credito" o "Debito"
    }
}
