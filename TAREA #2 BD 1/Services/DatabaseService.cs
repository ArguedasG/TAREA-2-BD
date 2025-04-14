using Microsoft.Data.SqlClient;
using System.Data;
using TAREA__2_BD_1.Models;

namespace TAREA__2_BD_1.Services
{
    public class DatabaseService
    {

        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> LoginUsuarioAsync(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("LoginUsuario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    var result = await command.ExecuteScalarAsync();
                    return result != null && (int)result == 0; // Asume que 0 indica éxito
                }
            }
        }
        public async Task<List<Empleado>> ListarEmpleadosAsync(string filtro = "")
        {
            var empleados = new List<Empleado>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("ListarEmpleados", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Filtro", filtro);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            empleados.Add(new Empleado
                            {
                                Id = reader.GetInt32("Id"),
                                ValorDocumentoIdentidad = reader.GetString("ValorDocumentoIdentidad"),
                                Nombre = reader.GetString("Nombre"),
                                IdPuesto = reader.GetInt32("IdPuesto"),
                                FechaContratacion = reader.GetDateTime("FechaContratacion"),
                                SaldoVacaciones = reader.GetDecimal("SaldoVacaciones"),
                                EsActivo = reader.GetBoolean("EsActivo")
                            });
                        }
                    }
                }
            }
            return empleados;
        }

        public async Task<int> InsertarEmpleadoAsync(Empleado empleado)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("InsertarEmpleado", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ValorDocumentoIdentidad", empleado.ValorDocumentoIdentidad);
                    command.Parameters.AddWithValue("@Nombre", empleado.Nombre);
                    command.Parameters.AddWithValue("@IdPuesto", empleado.IdPuesto);
                    command.Parameters.AddWithValue("@FechaContratacion", empleado.FechaContratacion);
                    command.Parameters.AddWithValue("@SaldoVacaciones", empleado.SaldoVacaciones);
                    command.Parameters.AddWithValue("@EsActivo", empleado.EsActivo);
                    var returnValue = new SqlParameter("@ReturnVal", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(returnValue);
                    await command.ExecuteNonQueryAsync();
                    return (int)returnValue.Value; // Código de error (0 si es éxito)
                }
            }
        }

        public async Task<List<Puesto>> ObtenerPuestosAsync()
        {
            var puestos = new List<Puesto>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT Id, Nombre, SalarioxHora FROM Puesto ORDER BY Nombre", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            puestos.Add(new Puesto
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                SalarioxHora = reader.GetDecimal("SalarioxHora")
                            });
                        }
                    }
                }
            }
            return puestos;
        }
    }
}
