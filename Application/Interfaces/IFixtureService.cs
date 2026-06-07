using Shared.Common;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFixtureService
    {
        Task<ApiResponse<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string fixtureId, string line, int status);
    }
}
