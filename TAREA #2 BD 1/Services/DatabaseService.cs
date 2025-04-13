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
    }
}
