using System;
using System.Data.SqlClient;

namespace Infrastructure.Data
{
    public class UnitOfWork : IDisposable
    {
        public SqlConnection Connection { get; }
        public SqlTransaction Transaction { get; private set; }

        public UnitOfWork(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
            Connection.Open();

            Transaction = Connection.BeginTransaction();
        }

        public void Commit()
        {
            Transaction.Commit();
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }

        public void Dispose()
        {
            Transaction?.Dispose();

            if (Connection.State == System.Data.ConnectionState.Open)
                Connection.Close();

            Connection.Dispose();
        }
    }
}
