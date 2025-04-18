using Microsoft.Data.SqlClient;
using System.Data;
using TAREA__2_BD_1.Models;
using System.Net;
using System.Net.Sockets;

namespace TAREA__2_BD_1.Services
{
    public class DatabaseService
    {

        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(int CodigoError, int? UserId)> LoginUsuarioAsync(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_LoginUsuario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    string myIP = "";
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            myIP = ip.ToString();
                            break;
                        }
                    }

                    command.Parameters.AddWithValue("@PostInIP", myIP);
                    var codigoErrorParam = new SqlParameter("@CodigoError", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(codigoErrorParam);
                    var userIdParam = new SqlParameter("@UserId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(userIdParam);
                    await command.ExecuteNonQueryAsync();
                    int codigoError = (int)codigoErrorParam.Value;
                    int? userId = userIdParam.Value != DBNull.Value ? (int?)userIdParam.Value : null;

                    return (codigoError, userId);
                }
            }
        }
        public async Task<List<Empleado>> ListarEmpleadosAsync(string filtro, int idUsuario)
        {
            var empleados = new List<Empleado>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_ListarEmpleados", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Filtro", filtro);
                    command.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    string myIP = "";
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            myIP = ip.ToString();
                            break;
                        }
                    }
                    command.Parameters.AddWithValue("@IP", myIP);
                    var codigoErrorParam = new SqlParameter("@ErrorCode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(codigoErrorParam);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            empleados.Add(new Empleado
                            {
                                ValorDocumentoIdentidad = reader.GetString("ValorDocumentoIdentidad"),
                                Nombre = reader.GetString("Nombre"),
                                IdPuesto = reader.GetInt32("IdPuesto"),
                                FechaContratacion = reader.GetDateTime("FechaContratacion"),
                                SaldoVacaciones = reader.GetInt32("SaldoVacaciones")
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


        //EDITAR ESTE MÉTODO PARA ELIMINAR EL SQL INCRUSTADO (FALTA EL STORED PROCEDURE)
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

        public async Task<int> ActualizarEmpleadoAsync(Empleado empleado, int idUsuario)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_UpdateEmpleado", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Parámetros de entrada
                    command.Parameters.AddWithValue("@inIdEmpleado", empleado.Id);
                    command.Parameters.AddWithValue("@inValorDocumentoIdentidad", empleado.ValorDocumentoIdentidad);
                    command.Parameters.AddWithValue("@inNombre", empleado.Nombre);
                    command.Parameters.AddWithValue("@inIdPuesto", empleado.IdPuesto);
                    command.Parameters.AddWithValue("@inIdPostByUser", idUsuario);

                    // Obtener la IP del cliente
                    string myIP = "";
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            myIP = ip.ToString();
                            break;
                        }
                    }
                    command.Parameters.AddWithValue("@inPostInIP", myIP);

                    // Agregar la hora actual
                    command.Parameters.AddWithValue("@inPostTime", DateTime.Now);

                    // Parámetro de salida
                    var resultCodeParam = new SqlParameter("@outResultCode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(resultCodeParam);

                    // Ejecutar el procedimiento almacenado
                    await command.ExecuteNonQueryAsync();

                    // Retornar el código de resultado
                    return (int)resultCodeParam.Value;
                }
            }
        }
    }
}
