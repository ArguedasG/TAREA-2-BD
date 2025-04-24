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
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
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
                    return (int)codigoError.Value;
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

                        command.Parameters.AddWithValue("@inEmpleadoId", empleado.Id);
                        command.Parameters.AddWithValue("@inValorDocumentoIdentidad", empleado.ValorDocumentoIdentidad);
                        command.Parameters.AddWithValue("@inNombre", empleado.Nombre);
                        command.Parameters.AddWithValue("@inPuestoId", empleado.IdPuesto);
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

                        return (int)codigoError.Value;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Error de SQL: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error general: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
        public async Task<int> EliminarEmpleadoAsync(Empleado empleado, int idUsuario)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_DeleteEmpleado", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@inEmpleadoId", empleado.Id);
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
                        return (int)codigoError.Value;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Error de SQL: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error general: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
        public async Task<Empleado> ConsultarEmpleadoAsync(int idEmpleado, int idUsuario)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_ConsultarEmpleado", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@inEmpleadoId", idEmpleado);
                        command.Parameters.AddWithValue("@inUserId", idUsuario);

                        // Obtener IP local
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

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows && await reader.ReadAsync())
                            {
                                var empleado = new Empleado
                                {
                                    ValorDocumentoIdentidad = reader["ValorDocumentoIdentidad"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    NombrePuesto = reader["NombrePuesto"].ToString(),
                                    SaldoVacaciones = Convert.ToDecimal(reader["SaldoVacaciones"])
                                };
                                return empleado;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Error de SQL: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error general: {ex.Message}");
                throw;
            }
        }

        public async Task<DetalleMovimientos> ListarMovimientosPorEmpleadoAsync(string valorDocumentoIdentidad, int idUsuario)
        {
            var detalleMovimientos = new DetalleMovimientos();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_ListarMovimientosPorEmpleado", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@inValorDocumentoIdentidad", valorDocumentoIdentidad);
                    command.Parameters.AddWithValue("@inUserId", idUsuario);

                    // Obtener IP local
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

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                if (detalleMovimientos.ValorDocumentoIdentidad == null)
                                {
                                    detalleMovimientos.ValorDocumentoIdentidad = reader["ValorDocumentoIdentidad"].ToString();
                                    detalleMovimientos.NombreEmpleado = reader["NombreEmpleado"].ToString();
                                    detalleMovimientos.SaldoVacaciones = reader["SaldoVacaciones"] != DBNull.Value
                                        ? Convert.ToDecimal(reader["SaldoVacaciones"])
                                        : 0;
                                }

                                detalleMovimientos.Movimientos.Add(new Movimiento
                                {
                                    FechaMovimiento = reader["FechaMovimiento"] != DBNull.Value
                                        ? reader.GetDateTime(reader.GetOrdinal("FechaMovimiento"))
                                        : DateTime.MinValue,
                                    NombreTipoMovimiento = reader["NombreTipoMovimiento"]?.ToString() ?? "Desconocido",
                                    Monto = reader["Monto"] != DBNull.Value
                                        ? Convert.ToDecimal(reader["Monto"])
                                        : 0,
                                    NuevoSaldo = reader["NuevoSaldo"] != DBNull.Value
                                        ? Convert.ToDecimal(reader["NuevoSaldo"])
                                        : 0,
                                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? "Desconocido",
                                    IP = reader["IP"]?.ToString() ?? "N/A",
                                    FechaHoraRegistro = reader["FechaHoraRegistro"] != DBNull.Value
                                        ? reader.GetDateTime(reader.GetOrdinal("FechaHoraRegistro"))
                                        : DateTime.MinValue
                                });
                            }
                        }
                    }

                    int codigoError = (int)codigoErrorParam.Value;
                    if (codigoError != 0)
                    {
                        throw new Exception($"Error en ListarMovimientosPorEmpleado. Código de error: {codigoError}");
                    }
                }
            }

            return detalleMovimientos;
        }

        public async Task<int> InsertarMovimientoAsync(string valorDocumentoIdentidad, int idTipoMovimiento, decimal monto, int idUsuario)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_InsertarMovimiento", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@inValorDocumentoIdentidad", valorDocumentoIdentidad);
                        command.Parameters.AddWithValue("@inIdTipoMovimiento", idTipoMovimiento);
                        command.Parameters.AddWithValue("@inMonto", monto);
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

                        var codigoErrorParam = new SqlParameter("@outCodigoError", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(codigoErrorParam);

                        await command.ExecuteNonQueryAsync();

                        int codigoError = (int)codigoErrorParam.Value;
                        if (codigoError != 0)
                        {
                            throw new Exception($"Error en sp_InsertarMovimiento. Código de error: {codigoError}");
                        }

                        return codigoError;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Error de SQL: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
                throw new Exception("Ocurrió un error al intentar insertar el movimiento en la base de datos.", ex);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error general: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
                throw new Exception("Ocurrió un error inesperado al insertar el movimiento.", ex);
            }
        }

        public async Task<List<TipoMovimiento>> ObtenerTiposMovimientoAsync(int idUsuario)
        {
            var tiposMovimiento = new List<TipoMovimiento>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_ObtenerTipoMovimiento", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@inUserId", idUsuario);

                    // Obtener IP local
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

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tiposMovimiento.Add(new TipoMovimiento
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                TipoAccion = reader.GetString(reader.GetOrdinal("TipoAccion"))
                            });
                        }
                    }

                    int codigoError = (int)codigoErrorParam.Value;
                    if (codigoError != 0)
                    {
                        throw new Exception($"Error en sp_ObtenerTipoMovimiento. Código de error: {codigoError}");
                    }
                }
            }
            return tiposMovimiento;
        }
    }
}
