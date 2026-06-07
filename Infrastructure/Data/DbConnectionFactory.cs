using System.Configuration;
using System.Data.SqlClient;

namespace Infrastructure.Data
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory()
        {
            _connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"]
                .ConnectionString;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
