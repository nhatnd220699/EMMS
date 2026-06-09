using Application.Interfaces;
using Application.Services;
using Infrastructure.Repositories;
using Shared.Common;
using Shared.DTOs;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Text.RegularExpressions;

namespace EMMS.Controllers
{
    [RoutePrefix("api/fixture")]
    public class FixtureController : ApiController
    {
        private readonly IFixtureService _fixtureService;
        public FixtureController()
        {
            _fixtureService = new FixtureService(new FixtureRepository());
        }

        [HttpGet]
        [Route("options")]
        public async Task<IHttpActionResult> GetFilterOptions()
        {
            try
            {
                var result = await _fixtureService.GetFilterOptionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<FixtureFilterOptionDto>
                {
                    Success = false,
                    Message = ex.GetBaseException().Message,
                    Data = null
                });
            }
        }

        [HttpGet]
        [Route("dashboard")]
        public async Task<IHttpActionResult> GetFixtureDashboard(string type = "FATP", string line = "", string department = "")
        {
            try
            {
                var result = await _fixtureService.GetFixtureDashboardsAsync(type, line, department);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<List<FixtureDashboardDto>>
                {
                    Success = false,
                    Message = ex.GetBaseException().Message,
                    Data = null
                });
            }
        }

        [HttpGet]
        [Route("monthly")]
        public async Task<IHttpActionResult> GetFixtureMonthly(string eqSn, string type = "FATP", string line = "", string department = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(eqSn))
                {
                    return Ok(new ApiResponse<FixtureMonthlyDto>
                    {
                        Success = false,
                        Message = "EQ_SN is required.",
                        Data = null
                    });
                }

                var result = await _fixtureService.GetFixtureMonthlyAsync(type, line, department, eqSn);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<FixtureMonthlyDto>
                {
                    Success = false,
                    Message = ex.GetBaseException().Message,
                    Data = null
                });
            }
        }

        [HttpGet]
        [Route("daily")]
        public async Task<IHttpActionResult> GetFixtureDaily(string eqSn, string date = "", string type = "FATP", string line = "", string department = "", string shift = "Day", string uploadId = "", bool isApproverMode = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(eqSn))
                {
                    return Ok(new ApiResponse<FixtureDailyDto>
                    {
                        Success = false,
                        Message = "EQ_SN is required.",
                        Data = null
                    });
                }

                var chkDay = ToChkDay(date);
                var result = await _fixtureService.GetFixtureDailyAsync(type, line, department, eqSn, chkDay, shift, uploadId, isApproverMode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<FixtureDailyDto>
                {
                    Success = false,
                    Message = ex.GetBaseException().Message,
                    Data = null
                });
            }
        }

        [HttpPost]
        [Route("daily/save")]
        public async Task<IHttpActionResult> SaveFixtureDaily()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var fixture = BuildFixtureFromRequest(httpRequest);

                var type = GetFormValue(httpRequest, "type", "FATP");
                var line = GetFormValue(httpRequest, "line", "");
                var department = GetFormValue(httpRequest, "department", "");

                SaveImageFiles(httpRequest, fixture);

                var result = await _fixtureService.SaveFixtureDailyAsync(type, line, department, fixture);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<FixtureDailyDto>
                {
                    Success = false,
                    Message = ex.GetBaseException().Message,
                    Data = null
                });
            }
        }

        private DateTime ParseDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return DateTime.Today;

            DateTime result;
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result;

            if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result;

            return DateTime.TryParse(date, out result) ? result : DateTime.Today;
        }

        private string ToChkDay(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return "";

            DateTime result;
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result.ToString("yyyyMMdd");

            if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result.ToString("yyyyMMdd");

            return DateTime.TryParse(date, out result) ? result.ToString("yyyyMMdd") : "";
        }

        private string GetFormValue(HttpRequest request, string key, string defaultValue = "")
        {
            var value = request.Form[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }

        private bool GetFormBool(HttpRequest request, string key)
        {
            var value = GetFormValue(request, key, "false");
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
        }

        private int GetFormInt(HttpRequest request, string key)
        {
            int result;
            return int.TryParse(GetFormValue(request, key, "0"), out result) ? result : 0;
        }

        private string UnescapeJsonString(string value)
        {
            return (value ?? "")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }

        private List<string> GetExistingImagePaths(HttpRequest request, int index)
        {
            var raw = GetFormValue(request, "items[" + index + "].ExistingImagePaths", "[]");
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(raw)) return result;

            var matches = Regex.Matches(raw, "\"((?:\\\\.|[^\"\\\\])*)\"");
            foreach (Match match in matches)
            {
                var value = UnescapeJsonString(match.Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(value))
                    result.Add(value.Trim());
            }

            return result.Distinct().ToList();
        }

        private FixtureDailyDto BuildFixtureFromRequest(HttpRequest request)
        {
            var itemCount = GetFormInt(request, "itemCount");
            var fixture = new FixtureDailyDto
            {
                EQ_SN = GetFormValue(request, "eqSn"),
                EQ_Name = GetFormValue(request, "eqName"),
                EQ_Model = GetFormValue(request, "eqModel"),
                Line = GetFormValue(request, "line"),
                Date = ParseDate(GetFormValue(request, "date")),
                Shift = GetFormValue(request, "shift", "Day"),
                IsApproverMode = GetFormBool(request, "isApproverMode")
            };

            for (var i = 0; i < itemCount; i++)
            {
                fixture.CheckItems.Add(new DailyItemInputDto
                {
                    Number_Order = GetFormInt(request, "items[" + i + "].Number_Order"),
                    MT_No = GetFormValue(request, "items[" + i + "].MT_No"),
                    MT_Item = GetFormValue(request, "items[" + i + "].MT_Item"),
                    MT_Desc = GetFormValue(request, "items[" + i + "].MT_Desc"),
                    Upload_Result = GetFormValue(request, "items[" + i + "].Upload_Result"),
                    Upload_ID = GetFormValue(request, "items[" + i + "].Upload_ID"),
                    Approval_ID = GetFormValue(request, "items[" + i + "].Approval_ID"),
                    Approval_Result = GetFormValue(request, "items[" + i + "].Approval_Result"),
                    Remark = GetFormValue(request, "items[" + i + "].Remark"),
                    ExistingImagePaths = GetExistingImagePaths(request, i)
                });
            }

            return fixture;
        }

        private void SaveImageFiles(HttpRequest request, FixtureDailyDto fixture)
        {
            if (request.Files == null || request.Files.Count == 0 || fixture == null || fixture.CheckItems == null)
                return;

            var dateFolder = fixture.Date.ToString("yyyyMMdd");
            var eqSnFolder = MakeSafeFileName(fixture.EQ_SN);
            var relativeFolder = "Content/Images/Maintenance/" + dateFolder + "/" + eqSnFolder;
            var physicalFolder = HttpContext.Current.Server.MapPath("~/" + relativeFolder);

            if (!Directory.Exists(physicalFolder))
                Directory.CreateDirectory(physicalFolder);

            for (var i = 0; i < fixture.CheckItems.Count; i++)
            {
                var item = fixture.CheckItems[i];
                var fileKey = "imageFiles_" + i;

                for (var fileIndex = 0; fileIndex < request.Files.Count; fileIndex++)
                {
                    if (!string.Equals(request.Files.GetKey(fileIndex), fileKey, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var file = request.Files[fileIndex];
                    if (file == null || file.ContentLength <= 0)
                        continue;

                    var extension = Path.GetExtension(file.FileName);
                    if (!IsAllowedImageExtension(extension))
                        continue;

                    var fileName = MakeSafeFileName(item.MT_No) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + extension;
                    var physicalPath = Path.Combine(physicalFolder, fileName);
                    file.SaveAs(physicalPath);

                    item.ExistingImagePaths.Add(relativeFolder + "/" + fileName);
                }

                item.ExistingImagePaths = item.ExistingImagePaths
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
            }
        }

        private bool IsAllowedImageExtension(string extension)
        {
            extension = (extension ?? "").ToLowerInvariant();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp" || extension == ".webp";
        }

        private string MakeSafeFileName(string value)
        {
            value = string.IsNullOrWhiteSpace(value) ? "file" : value.Trim();
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Replace(' ', '_');
        }
    }
}
