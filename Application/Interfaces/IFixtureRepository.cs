using Shared.DTOs;
using Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFixtureRepository
    {
        Task<DataDto<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string fixtureId, string line, int status);
    }
}
