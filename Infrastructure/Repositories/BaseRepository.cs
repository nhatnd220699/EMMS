using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class BaseRepository
    {
        protected readonly DbConnectionFactory _db;

        protected BaseRepository()
        {
            _db = new DbConnectionFactory();
        }
    }
}
