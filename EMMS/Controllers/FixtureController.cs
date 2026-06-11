using Application.Interfaces;
using Application.Services;
using Infrastructure.Repositories;
using Shared.Common;
using Shared.DTOs;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

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
        public async Task<IHttpActionResult> GetFixtureDashboard(string type = "FATP", string line = "", string department = "", string fixtureSN = "")
        {
            try
            {
                var result = await _fixtureService.GetFixtureDashboardsAsync(type, line, department, fixtureSN);
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

        //[HttpPost]
        //[Route("daily/save")]
        //public async Task<IHttpActionResult> SaveFixtureDaily()
        //{
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        var fixture = BuildFixtureFromRequest(httpRequest);

        //        var type = GetFormValue(httpRequest, "type", "FATP");
        //        var line = GetFormValue(httpRequest, "line", "");
        //        var department = GetFormValue(httpRequest, "department", "");

        //        SaveImageFiles(httpRequest, fixture);

        //        var result = await _fixtureService.SaveFixtureDailyAsync(type, line, department, fixture);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new ApiResponse<FixtureDailyDto>
        //        {
        //            Success = false,
        //            Message = ex.GetBaseException().Message,
        //            Data = null
        //        });
        //    }
        //}

        [HttpPost]
        [Route("image/getInfo")]
        public async Task<IHttpActionResult> GetImageInfo()
        {
            try
            {
                var request = HttpContext.Current.Request;
                if (request.Files.Count == 0)
                {

                    return Content(HttpStatusCode.BadRequest,
                        new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Image is required",
                            Data = null
                        });

                }

                var file = request.Files[0];

                var result = await _fixtureService.GetImageInfoAsync(file);

                return Json(result);

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
        [Route("daily/submit")]
        public async Task<IHttpActionResult> SaveFixture()
        {
            try
            {
                var request = HttpContext.Current.Request;
                if (request == null || request.Form.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest,
                       new ApiResponse<object>
                       {
                           Success = false,
                           Message = "Form no data, please check again!",
                           Data = null
                       });
                }

                var funcType = request.Form["FuncType"];
                var type = request.Form["Type"];
                var department = request.Form["Department"];
                var line = request.Form["Line"];

                var model = new FixtureDailyDto
                {
                    Line = request.Form["Line"],
                    EQ_SN = request.Form["EQ_SN"],
                    EQ_Name = request.Form["EQ_Name"],
                    EQ_Model = request.Form["EQ_Model"],
                    Date = request.Form["Date"].ToDate(),
                    Shift = request.Form["Shift"],
                    IsApproverMode = string.Equals(request.Form["IsApproverMode"], "true", StringComparison.OrdinalIgnoreCase),
                };


                // Parse items
                model.CheckItems = ParseItems(request);

                if (model.CheckItems.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest,
                      new ApiResponse<object>
                      {
                          Success = false,
                          Message = "No items found",
                          Data = null
                      });
                }

                var rootFolder = ConfigurationManager.AppSettings["FixtureImageRoot"];

                // build folder theo ngày
                var today = DateTime.Now;
                var subFolder = Path.Combine(
                    today.Year.ToString(),
                    today.Month.ToString("D2"),
                    today.Day.ToString("D2")
                );

                // Physical path
                var fullFolder = Path.Combine(rootFolder, subFolder);

                // tạo folder nếu chưa có
                if (!Directory.Exists(fullFolder))
                {
                    Directory.CreateDirectory(fullFolder);
                }

                var imageResultBuilder = new StringBuilder();
                //var uploaderResultBuilder = new StringBuilder();
                //var approverResultBuilder = new StringBuilder();

                foreach (var item in model.CheckItems)
                {
                    var imagePaths = new List<string>();

                    // ảnh cũ
                    if (item.ExistingImagePaths != null)
                    {
                        imagePaths.AddRange(item.ExistingImagePaths);
                    }

                    // ảnh mới
                    if (item.NewFiles != null)
                    {
                        foreach (var file in item.NewFiles)
                        {
                            if (file == null || file.ContentLength == 0) continue;

                            var ext = Path.GetExtension(file.FileName);

                            var fileName = $"{Guid.NewGuid()}{ext}";

                            var physicalPath = Path.Combine(fullFolder, fileName);

                            file.SaveAs(physicalPath);

                            // Logical path trả về
                            var relativePath = $"Fixture/{subFolder.Replace("\\", "/")}/{fileName}";

                            imagePaths.Add(relativePath);

                        }
                    }

                    // build chuỗi imgage result
                    var jsonArray = "[" + string.Join(", ", imagePaths.Select(p => $"\"{p}\"")) + "]";
                    imageResultBuilder.Append($"{item.MT_No}={jsonArray};").ToString().TrimEnd(';');


                    //// build chuỗi uploader result
                    //var uploaderResult = item.Upload_Result ?? "NA";
                    //uploaderResultBuilder.Append($"{item.MT_No}={uploaderResult};");

                    //// build chuỗi approver result
                    //var approverResult = item.Approval_Result ?? "";
                    //approverResultBuilder.Append($"{item.MT_No}={approverResult};");
                }

                var imageResult = imageResultBuilder.ToString().TrimEnd(';');
                //var uploadResult = uploaderResultBuilder.ToString().TrimEnd(';');
                //var approvalResult = approverResultBuilder.ToString().TrimEnd(';');

                var result = await _fixtureService.SaveFixtureDailyAsync(type, line, department, funcType, imageResult, model);

                return Ok(new
                {
                    success = result.Success,
                    message = result.Success ? "Saved successfully" : "Save fail!",
                    data = ""
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private List<DailyItemInputDto> ParseItems(HttpRequest request)
        {
            var items = new List<DailyItemInputDto>();
            int index = 0;

            while (true)
            {
                var mtNo = request.Form[$"CheckItems[{index}].MT_No"];

                if (mtNo == null) break;

                var item = new DailyItemInputDto
                {
                    MT_No = mtNo,
                    Number_Order = request.Form[$"CheckItems[{index}].Number_Order"].ToInt(),
                    MT_Item = request.Form[$"CheckItems[{index}].MT_Item"],
                    MT_Desc = request.Form[$"CheckItems[{index}].MT_Desc"],

                    Upload_Result = request.Form[$"CheckItems[{index}].Upload_Result"],
                    Approval_Result = request.Form[$"CheckItems[{index}].Approval_Result"],
                    Remark = request.Form[$"CheckItems[{index}].Remark"],
                    Upload_ID = request.Form[$"CheckItems[{index}].Upload_ID"],
                    Approval_ID = request.Form[$"CheckItems[{index}].Approval_ID"],
                };

                // ExistingImagePaths
                item.ExistingImagePaths = ParseStringList(
                    request, $"CheckItems[{index}].ExistingImagePaths"
                );

                // NewFiles
                item.NewFiles = GetFilesByPrefix(
                    request, $"CheckItems[{index}].NewFiles"
                );

                items.Add(item);
                index++;
            }

            return items;
        }

        private List<string> ParseStringList(HttpRequest request, string prefix)
        {
            var list = new List<string>();
            int i = 0;

            while (true)
            {
                var value = request.Form[$"{prefix}[{i}]"];
                if (value == null) break;

                list.Add(value);
                i++;
            }

            return list;
        }

        private List<HttpPostedFile> GetFilesByPrefix(HttpRequest request, string prefix)
        {
            var files = new List<HttpPostedFile>();

            for (int i = 0; i < request.Files.Count; i++)
            {
                var key = request.Files.AllKeys[i];

                if (!string.IsNullOrEmpty(key) && key.StartsWith(prefix))
                {
                    files.Add(request.Files[i]);
                }
            }

            return files;
        }


        //private DateTime ParseDate(string date)
        //{
        //    if (string.IsNullOrWhiteSpace(date)) return DateTime.Today;

        //    DateTime result;
        //    if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        //        return result;

        //    if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        //        return result;

        //    return DateTime.TryParse(date, out result) ? result : DateTime.Today;
        //}

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

        //private string GetFormValue(HttpRequest request, string key, string defaultValue = "")
        //{
        //    var value = request.Form[key];
        //    return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        //}

        //private bool GetFormBool(HttpRequest request, string key)
        //{
        //    var value = GetFormValue(request, key, "false");
        //    return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
        //}

        //private int GetFormInt(HttpRequest request, string key)
        //{
        //    int result;
        //    return int.TryParse(GetFormValue(request, key, "0"), out result) ? result : 0;
        //}

        //private string UnescapeJsonString(string value)
        //{
        //    return (value ?? "")
        //        .Replace("\\n", "\n")
        //        .Replace("\\r", "\r")
        //        .Replace("\\\"", "\"")
        //        .Replace("\\\\", "\\");
        //}

        //private List<string> GetExistingImagePaths(HttpRequest request, int index)
        //{
        //    var raw = GetFormValue(request, "items[" + index + "].ExistingImagePaths", "[]");
        //    var result = new List<string>();

        //    if (string.IsNullOrWhiteSpace(raw)) return result;

        //    var matches = Regex.Matches(raw, "\"((?:\\\\.|[^\"\\\\])*)\"");
        //    foreach (Match match in matches)
        //    {
        //        var value = UnescapeJsonString(match.Groups[1].Value);
        //        if (!string.IsNullOrWhiteSpace(value))
        //            result.Add(value.Trim());
        //    }

        //    return result.Distinct().ToList();
        //}

        //private FixtureDailyDto BuildFixtureFromRequest(HttpRequest request)
        //{
        //    var itemCount = GetFormInt(request, "itemCount");
        //    var fixture = new FixtureDailyDto
        //    {
        //        EQ_SN = GetFormValue(request, "eqSn"),
        //        EQ_Name = GetFormValue(request, "eqName"),
        //        EQ_Model = GetFormValue(request, "eqModel"),
        //        Line = GetFormValue(request, "line"),
        //        Date = ParseDate(GetFormValue(request, "date")),
        //        Shift = GetFormValue(request, "shift", "Day"),
        //        IsApproverMode = GetFormBool(request, "isApproverMode")
        //    };

        //    for (var i = 0; i < itemCount; i++)
        //    {
        //        fixture.CheckItems.Add(new DailyItemInputDto
        //        {
        //            Number_Order = GetFormInt(request, "items[" + i + "].Number_Order"),
        //            MT_No = GetFormValue(request, "items[" + i + "].MT_No"),
        //            MT_Item = GetFormValue(request, "items[" + i + "].MT_Item"),
        //            MT_Desc = GetFormValue(request, "items[" + i + "].MT_Desc"),
        //            Upload_Result = GetFormValue(request, "items[" + i + "].Upload_Result"),
        //            Upload_ID = GetFormValue(request, "items[" + i + "].Upload_ID"),
        //            Approval_ID = GetFormValue(request, "items[" + i + "].Approval_ID"),
        //            Approval_Result = GetFormValue(request, "items[" + i + "].Approval_Result"),
        //            Remark = GetFormValue(request, "items[" + i + "].Remark"),
        //            ExistingImagePaths = GetExistingImagePaths(request, i)
        //        });
        //    }

        //    return fixture;
        //}

        //private void SaveImageFiles(HttpRequest request, FixtureDailyDto fixture)
        //{
        //    if (request.Files == null || request.Files.Count == 0 || fixture == null || fixture.CheckItems == null)
        //        return;

        //    var dateFolder = fixture.Date.ToString("yyyyMMdd");
        //    var eqSnFolder = MakeSafeFileName(fixture.EQ_SN);
        //    var relativeFolder = "Content/Images/Maintenance/" + dateFolder + "/" + eqSnFolder;
        //    var physicalFolder = HttpContext.Current.Server.MapPath("~/" + relativeFolder);

        //    if (!Directory.Exists(physicalFolder))
        //        Directory.CreateDirectory(physicalFolder);

        //    for (var i = 0; i < fixture.CheckItems.Count; i++)
        //    {
        //        var item = fixture.CheckItems[i];
        //        var fileKey = "imageFiles_" + i;

        //        for (var fileIndex = 0; fileIndex < request.Files.Count; fileIndex++)
        //        {
        //            if (!string.Equals(request.Files.GetKey(fileIndex), fileKey, StringComparison.OrdinalIgnoreCase))
        //                continue;

        //            var file = request.Files[fileIndex];
        //            if (file == null || file.ContentLength <= 0)
        //                continue;

        //            var extension = Path.GetExtension(file.FileName);
        //            if (!IsAllowedImageExtension(extension))
        //                continue;

        //            var fileName = MakeSafeFileName(item.MT_No) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + extension;
        //            var physicalPath = Path.Combine(physicalFolder, fileName);
        //            file.SaveAs(physicalPath);

        //            item.ExistingImagePaths.Add(relativeFolder + "/" + fileName);
        //        }

        //        item.ExistingImagePaths = item.ExistingImagePaths
        //            .Where(x => !string.IsNullOrWhiteSpace(x))
        //            .Distinct()
        //            .ToList();
        //    }
        //}

        //private bool IsAllowedImageExtension(string extension)
        //{
        //    extension = (extension ?? "").ToLowerInvariant();
        //    return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp" || extension == ".webp";
        //}

        //private string MakeSafeFileName(string value)
        //{
        //    value = string.IsNullOrWhiteSpace(value) ? "file" : value.Trim();
        //    foreach (var invalidChar in Path.GetInvalidFileNameChars())
        //    {
        //        value = value.Replace(invalidChar, '_');
        //    }

        //    return value.Replace(' ', '_');
        //}
    }
}
