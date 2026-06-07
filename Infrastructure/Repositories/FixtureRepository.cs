using Application.Interfaces;
using Dapper;
using Shared.DTOs;
using Shared.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class FixtureRepository : BaseRepository, IFixtureRepository
    {
        async public Task<DataDto<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string fixtureId, string line, int status)
        {
            using (var conn = _db.CreateConnection())
            {
                using (var multi = conn.QueryMultiple(
                "SP_Fixture",
                new
                {
                    FixtureId = fixtureId,
                    Line = line,
                    Status = status
                },
                commandType: CommandType.StoredProcedure))
                {
                    var result =
                        multi.Read<ResultMessageDto>()
                             .FirstOrDefault();

                    var fixtures =
                        multi.Read<FixtureDashboardDto>()
                             .ToList();

                    return new DataDto<List<FixtureDashboardDto>>
                    {
                        Result = result?.IResult,
                        Message = result?.IMessage,
                        Data = fixtures
                    };
                }
            }
        }
    }
}
