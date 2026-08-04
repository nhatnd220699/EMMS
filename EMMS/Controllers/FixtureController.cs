using Application.Interfaces;
using Application.Mapper;
using Application.Services;
using Infrastructure.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Common;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace EMMS.Controllers
{
    [RoutePrefix("api/fixture")]
    public class FixtureController : ApiController
    {
        private readonly IFixtureService _fixtureService;
        private readonly IFixtureMapper _fixtureMapper;

        public FixtureController()
        {
            _fixtureService = new FixtureService(new FixtureRepository(), new FileService());
            _fixtureMapper = new FixtureMapper();
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Current != null
                && HttpContext.Current.Session != null
                && HttpContext.Current.Session["UserId"] != null;
        }

        private IHttpActionResult LoginRequired<T>()
        {
            return Content(HttpStatusCode.Unauthorized, new ApiResponse<T>
            {
                Success = false,
                Message = "LOGIN_REQUIRED",
                Data = default
            });
        }

        [HttpGet]
        [Route("options")]
        public async Task<IHttpActionResult> GetFilterOptions()
        {
            try
            {
                if (!IsLoggedIn()) return LoginRequired<FixtureFilterOptionDto>();

                var result = await _fixtureService.GetFilterOptionsAsync();

                if (!result.Success)
                {
                    switch (result.ErrorCode)
                    {
                        case "SYSTEM_ERROR":
                        case "DB_ERROR":
                            return Content(HttpStatusCode.InternalServerError, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                        case "BUSINESS_ERROR":
                        default:
                            return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                    }
                }

                return Ok(ApiResponseNew<FixtureFilterOptionDto>.Ok(result.Data, result.Message));
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("dashboard")]
        public async Task<IHttpActionResult> GetFixtureDashboard(string type = "FATP", string line = "", string department = "", string fixtureSN = "")
        {
            try
            {
                if (!IsLoggedIn()) return LoginRequired<List<FixtureDashboardDto>>();

                var result = await _fixtureService.GetFixtureDashboardsAsync(type, line, department, fixtureSN);

                if (!result.Success)
                {
                    switch (result.ErrorCode)
                    {
                        case "SYSTEM_ERROR":
                        case "DB_ERROR":
                            return Content(HttpStatusCode.InternalServerError, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                        case "BUSINESS_ERROR":
                        default:
                            return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                    }
                }

                return Ok(ApiResponseNew<List<FixtureDashboardDto>>.Ok(result.Data, result.Message));
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("monthly")]
        public async Task<IHttpActionResult> GetFixtureMonthly(string eqSn, string type = "FATP", string line = "", string department = "", string month = "")
        {
            try
            {
                if (!IsLoggedIn()) return LoginRequired<FixtureMonthlyDto>();

                if (string.IsNullOrWhiteSpace(eqSn))
                {
                    return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail("Fixture SN is required"));
                }

                var chkDay = ToMonthChkDay(month);
                var result = await _fixtureService.GetFixtureMonthlyAsync(type, line, department, eqSn, chkDay);

                if (!result.Success)
                {
                    switch (result.ErrorCode)
                    {
                        case "SYSTEM_ERROR":
                        case "DB_ERROR":
                            return Content(HttpStatusCode.InternalServerError, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                        case "BUSINESS_ERROR":
                        default:
                            return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                    }
                }

                return Ok(ApiResponseNew<FixtureMonthlyDto>.Ok(result.Data, result.Message));
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("daily")]
        public async Task<IHttpActionResult> GetFixtureDaily(string eqSn, string date = "", string type = "FATP", string line = "", string department = "", string shift = "Day", string uploadId = "", bool isApproverMode = false)
        {
            try
            {
                if (!IsLoggedIn()) return LoginRequired<FixtureDailyDto>();

                if (string.IsNullOrWhiteSpace(eqSn))
                {
                    return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail("Fixture SN is required"));
                }

                var chkDay = ToChkDay(date);

                var result = await _fixtureService.GetFixtureDailyAsync(type, line, department, eqSn, chkDay, shift, uploadId, isApproverMode);

                if (!result.Success)
                {
                    switch (result.ErrorCode)
                    {
                        case "SYSTEM_ERROR":
                        case "DB_ERROR":
                            return Content(HttpStatusCode.InternalServerError, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                        case "BUSINESS_ERROR":
                        default:
                            return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                    }
                }

                return Ok(ApiResponseNew<FixtureDailyDto>.Ok(result.Data, result.Message));
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("image/getInfo")]
        public async Task<IHttpActionResult> GetImageInfo()
        {
            try
            {
                var request = HttpContext.Current.Request;
                if (request.Files.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail("Image is required"));
                }

                var file = request.Files[0];

                var result = await _fixtureService.GetImageInfoAsync(file);

                //return Json(result);

                if (!result.Success)
                {
                    switch (result.ErrorCode)
                    {
                        case "SYSTEM_ERROR":
                        case "DB_ERROR":
                            return Content(HttpStatusCode.InternalServerError, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                        case "BUSINESS_ERROR":
                        default:
                            return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                    }
                }

                return Ok(ApiResponseNew<JObject>.Ok(result.Data, result.Message));

            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }


        [HttpPost]
        [Route("daily/submit")]
        public async Task<IHttpActionResult> SaveFixture()
        {
            try
            {
                var request = HttpContext.Current.Request;

                var action = request.Form["Action"];
                var type = request.Form["Type"];
                var department = request.Form["Department"];

                if (request == null || request.Form.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail("Form no data"));
                }

                var model = _fixtureMapper.Map(request);

                var result = await _fixtureService.SaveFixtureDailyAsync(model, request.Files, action, type, department);

                if (!result.Success)
                {
                    switch (result.ErrorCode)
                    {
                        case "SYSTEM_ERROR":
                        case "DB_ERROR":
                            return Content(HttpStatusCode.InternalServerError, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                        case "BUSINESS_ERROR":
                        default:
                            return Content(HttpStatusCode.BadRequest, ApiResponseNew<object>.Fail(result.Message, result.ErrorCode));
                    }
                }

                return Ok(ApiResponseNew<string>.Ok(null, result.Message));
            }
            catch (Exception)
            {
                return InternalServerError();
            }

        }

        private string ToChkDay(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return "";

            if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result.ToString("yyyyMMdd");

            if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result.ToString("yyyyMMdd");

            return DateTime.TryParse(date, out result) ? result.ToString("yyyyMMdd") : "";
        }

        private string ToMonthChkDay(string month)
        {
            if (string.IsNullOrWhiteSpace(month)) return "";

            if (DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return new DateTime(result.Year, result.Month, 1).ToString("yyyyMMdd");

            if (DateTime.TryParseExact(month, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return new DateTime(result.Year, result.Month, 1).ToString("yyyyMMdd");

            if (DateTime.TryParseExact(month, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return new DateTime(result.Year, result.Month, 1).ToString("yyyyMMdd");

            if (DateTime.TryParseExact(month, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return new DateTime(result.Year, result.Month, 1).ToString("yyyyMMdd");

            return DateTime.TryParse(month, out result) ? new DateTime(result.Year, result.Month, 1).ToString("yyyyMMdd") : "";
        }
    }
}
