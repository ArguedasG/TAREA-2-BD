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
                    command.Parameters.AddWithValue("@inUsername", username);
                    command.Parameters.AddWithValue("@inPassword", password);

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

                    var codigoErrorParam = new SqlParameter("@outCodigoError", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(codigoErrorParam);

                    var userIdParam = new SqlParameter("@outUserId", SqlDbType.Int)
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
                    command.Parameters.AddWithValue("@inFiltro", filtro);
                    command.Parameters.AddWithValue("@inUserId", idUsuario);

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
                    var codigoError = new SqlParameter("@outCodigoError", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(codigoError);

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

        public async Task<int> InsertarEmpleadoAsync(Empleado empleado, int idUsuario)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_InsertarEmpleado", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@inPuestoId", empleado.IdPuesto);
                    command.Parameters.AddWithValue("@inValorDocumentoIdentidad", empleado.ValorDocumentoIdentidad);
                    command.Parameters.AddWithValue("@inNombre", empleado.Nombre);
                    command.Parameters.AddWithValue("@inUserId", idUsuario);

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

                    var codigoError = new SqlParameter("@outCodigoError", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(codigoError);

                    await command.ExecuteNonQueryAsync();
                    return (int)codigoError.Value; // Código de error (0 si es éxito)
                }
            }
        }


        public async Task<List<Puesto>> ObtenerPuestosAsync(int idUsuario)
        {
            var puestos = new List<Puesto>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_ObtenerPuestos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@inUserId", idUsuario);

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

                        var outResultCodeParam = new SqlParameter("@outCodigoError", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outResultCodeParam);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                puestos.Add(new Puesto
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                    SalarioxHora = reader.GetDecimal(reader.GetOrdinal("SalarioxHora"))
                                });
                            }
                        }

                        int codigoError = (int)outResultCodeParam.Value;
                        if (codigoError != 0)
                        {
                            throw new Exception($"Error en sp_ObtenerPuestos. Código de error: {codigoError}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al obtener los puestos: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
            }

            return puestos;
        }

        public async Task<int> ActualizarEmpleadoAsync(Empleado empleado, int idUsuario)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_UpdateEmpleado", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros de entrada
                        command.Parameters.AddWithValue("@inEmpleadoId", empleado.Id);
                        command.Parameters.AddWithValue("@inValorDocumentoIdentidad", empleado.ValorDocumentoIdentidad);
                        command.Parameters.AddWithValue("@inNombre", empleado.Nombre);
                        command.Parameters.AddWithValue("@inPuestoId", empleado.IdPuesto);
                        command.Parameters.AddWithValue("@inUserId", idUsuario);

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

                        // Parámetro de salida
                        var codigoError = new SqlParameter("@outCodigoError", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(codigoError);

                        // Ejecutar el procedimiento almacenado
                        await command.ExecuteNonQueryAsync();

                        // Retornar el código de resultado
                        return (int)codigoError.Value;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Error de SQL: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
                throw; // Re-lanzar la excepción para que el controlador también pueda manejarla
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error general: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
