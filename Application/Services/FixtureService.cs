using Application.Interfaces;
using Shared.Common;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class FixtureService : IFixtureService
    {
        private readonly IFixtureRepository _fixtureRepository;

        public FixtureService(IFixtureRepository fixtureRepository)
        {
            _fixtureRepository = fixtureRepository;
        }

        async public Task<ApiResponse<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string fixtureId, string line, int status)
        {
            var repoResult = await _fixtureRepository.GetFixtureDashboardsAsync(fixtureId, line, status);

            // DB trả lỗi
            if (repoResult.Result != "Success")
            {
                return new ApiResponse<List<FixtureDashboardDto>>
                {
                    Success = false,
                    Message = repoResult.Message,
                    Data = null
                };
            }

            // Success
            return new ApiResponse<List<FixtureDashboardDto>>
            {
                Success = true,
                Message = "Get fixtures successfully",
                Data = repoResult.Data
            };
        }
    }
}
