using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
