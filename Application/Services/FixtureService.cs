using Application.Interfaces;
using Shared.Common;
using Shared.DTOs;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ApiResponse<FixtureFilterOptionDto>> GetFilterOptionsAsync()
        {
            try
            {
                var typeResult = await _fixtureRepository.GetTypesAsync();
                var lineResult = await _fixtureRepository.GetLinesAsync();
                var departmentResult = await _fixtureRepository.GetDepartmentsAsync();

                var success = typeResult.Result.IsPass() && lineResult.Result.IsPass() && departmentResult.Result.IsPass();

                return new ApiResponse<FixtureFilterOptionDto>
                {
                    Success = success,
                    Message = success ? "Get filter options OK!" : "Get filter options failed.",
                    Data = success ? new FixtureFilterOptionDto
                    {
                        Types = typeResult.Data,
                        Lines = lineResult.Data,
                        Departments = departmentResult.Data
                    } : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FixtureFilterOptionDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string type, string line, string department)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureDashboardsAsync(type, line, department);

                var success = repoResult.Result.IsPass();

                var rows = repoResult.Data;
                var data = rows
                .OrderBy(x => x.Number_Order)
                .ThenBy(x => x.EQ_SN)
                .ToList();

                return new ApiResponse<List<FixtureDashboardDto>>
                {
                    Success = success,
                    Message = repoResult.Message,
                    Data = success ? data : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<FixtureDashboardDto>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<FixtureMonthlyDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureMonthlyAsync(type, line, department, eqSn);

                var success = repoResult.Result.IsPass();

                return new ApiResponse<FixtureMonthlyDto>
                {
                    Success = success,
                    Message = repoResult.Message,
                    Data = success ? repoResult.Data : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FixtureMonthlyDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<FixtureDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureDailyAsync(type, line, department, eqSn, chkDay, shift, uploadId, isApproverMode);

                var success = repoResult.Result.IsPass();

                return new ApiResponse<FixtureDto>
                {
                    Success = success,
                    Message = repoResult.Message,
                    Data = success ? repoResult.Data : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FixtureDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<FixtureDto>> SaveFixtureDailyAsync(string type, string line, string department, FixtureDto fixture)
        {
            try
            {
                var repoResult = await _fixtureRepository.SaveFixtureDailyAsync(type, line, department, fixture);

                var success = repoResult.Result.IsPass();

                return new ApiResponse<FixtureDto>
                {
                    Success = success,
                    Message = repoResult.Message,
                    Data = success ? repoResult.Data : null
                };
            }
            catch(Exception ex)
            {
                return new ApiResponse<FixtureDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}
