using Application.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Common;
using Shared.DTOs;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Application.Services
{
    public class FixtureService : IFixtureService
    {
        private readonly IFixtureRepository _fixtureRepository;
        private readonly IFileService _fileService;

        public FixtureService(IFixtureRepository fixtureRepository, IFileService fileService)
        {
            _fixtureRepository = fixtureRepository;
            _fileService = fileService;
        }

        private string UnescapeJsonString(string value)
        {
            return value.ToText()
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }

        private List<string> ParseImagePaths(string dbImageRaw)
        {
            var relativeImgPath = ConfigurationManager.AppSettings["RelativeImgPath"];

            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(dbImageRaw)) return result;

            dbImageRaw = dbImageRaw.Trim();

            if (dbImageRaw.StartsWith("[") && dbImageRaw.EndsWith("]"))
            {
                var matches = Regex.Matches(dbImageRaw, "\"((?:\\\\.|[^\"\\\\])*)\"");
                foreach (Match match in matches)
                {
                    var value = UnescapeJsonString(match.Groups[1].Value);
                    if (!string.IsNullOrWhiteSpace(value))
                        result.Add(relativeImgPath+value.Trim());
                }

                return result.Distinct().ToList();
            }

            return dbImageRaw
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        public async Task<Result<FixtureFilterOptionDto>> GetFilterOptionsAsync()
        {
            try
            {
                var types = await _fixtureRepository.GetTypesAsync();
                var lines = await _fixtureRepository.GetLinesAsync();
                var departments = await _fixtureRepository.GetDepartmentsAsync();

                var result = new FixtureFilterOptionDto
                {
                    Types = types,
                    Lines = lines,
                    Departments = departments
                };

                return Result<FixtureFilterOptionDto>.Ok(result);
            }
            catch (SqlException)
            {
                return Result<FixtureFilterOptionDto>.Fail("Database error", "DB_ERROR");
            }
            catch (BusinessException ex)
            {
                return Result<FixtureFilterOptionDto>.Fail(ex.Message, "BUSINESS_ERROR");
            }
            catch (Exception)
            {
                return Result<FixtureFilterOptionDto>.Fail("System error", "SYSTEM_ERROR");
            }
        }

        public async Task<Result<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string type, string line, string department, string sqSn)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureDashboardsAsync(type, line, department, sqSn);

                var data = repoResult
                            .OrderBy(x => x.Number_Order)
                            .ThenBy(x => x.EQ_SN)
                            .ToList();

                return Result<List<FixtureDashboardDto>>.Ok(data);
            }
            catch (SqlException)
            {
                return Result<List<FixtureDashboardDto>>.Fail("Database error", "DB_ERROR");
            }
            catch (BusinessException ex)
            {
                return Result<List<FixtureDashboardDto>>.Fail(ex.Message, "BUSINESS_ERROR");
            }
            catch (Exception)
            {
                return Result<List<FixtureDashboardDto>>.Fail("System error", "SYSTEM_ERROR");
            }
        }

        public async Task<Result<FixtureMonthlyDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn, string chkDay)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureMonthlyAsync(type, line, department, eqSn, chkDay);

                var header = repoResult.Header;
                var rows = repoResult.Rows;

                var data = new FixtureMonthlyDto
                {
                    EQ_SN = header.EQ_SN,
                    EQ_Name = header.EQ_Name,
                    EQ_Model = header.EQ_Model,
                    Line = header.Line,
                    Shift = header.Shift,
                    Year = header.Year.ToInt(),
                    Month = header.Month.ToInt(),
                    MaintenanceItems = rows
                        .GroupBy(x => new { MT_No = x.MT_No.ToText(), x.MT_Item, x.MT_Desc })
                        .Where(g => !string.IsNullOrWhiteSpace(g.Key.MT_No) && !string.IsNullOrWhiteSpace(g.Key.MT_Desc))
                        .Select(g => new MonthlyItemDto
                        {
                            Number_Order = g.Min(x => x.Number_Order.ToInt()),
                            MT_No = g.Key.MT_No,
                            MT_Item = g.Key.MT_Item,
                            MT_Desc = g.Key.MT_Desc,
                            DailyResults = g
                                .Where(x => x.Day.ToInt() > 0)
                                .GroupBy(x => x.Day.ToInt())
                                .ToDictionary(x => x.Key, x => x.Last().Status ?? "NA")
                        })
                        .OrderBy(x => x.Number_Order)
                        .ToList()

                };

                return Result<FixtureMonthlyDto>.Ok(data);
            }
            catch (SqlException)
            {
                return Result<FixtureMonthlyDto>.Fail("Database error", "DB_ERROR");
            }
            catch (BusinessException ex)
            {
                return Result<FixtureMonthlyDto>.Fail(ex.Message, "BUSINESS_ERROR");
            }
            catch (Exception)
            {
                return Result<FixtureMonthlyDto>.Fail("System error", "SYSTEM_ERROR");
            }
        }

        public async Task<Result<FixtureDailyDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureDailyAsync(type, line, department, eqSn, chkDay, shift, uploadId, isApproverMode);

                var header = repoResult.Header;
                var rows = repoResult.Rows;

                var data = new FixtureDailyDto
                {
                    EQ_SN = header.EQ_SN,
                    EQ_Name = header.EQ_Name,
                    EQ_Model = header.EQ_Model,
                    Line = header.Line,
                    Date = header.Date.ToDate(),
                    Shift = header.Shift,
                    IsApproverMode = isApproverMode,
                    CheckItems = rows
                        .Select(x => new DailyItemDto
                        {
                            Number_Order = x.Number_Order.ToInt(),
                            MT_No = x.MT_No.ToText(),
                            MT_Item = x.MT_Item,
                            MT_Desc = x.MT_Desc,

                            ExistingImagePaths = ParseImagePaths(x.Upload_PathImage.ToText()),
                            Upload_Result = x.Upload_Result.ToText(),
                            Upload_ID = x.Upload_ID.ToText(),

                            Approval_ID = x.Approval_ID.ToText(),
                            Approval_Result = x.Approval_Result.ToText(),

                            Remark = x.Remark.ToText()
                        })
                        .OrderBy(x => x.Number_Order)
                        .ThenBy(x => x.MT_No.ToInt())
                        .ToList()
                };

                return Result<FixtureDailyDto>.Ok(data);
            }
            catch (SqlException)
            {
                return Result<FixtureDailyDto>.Fail("Database error", "DB_ERROR");
            }
            catch (BusinessException ex)
            {
                return Result<FixtureDailyDto>.Fail(ex.Message, "BUSINESS_ERROR");
            }
            catch (Exception)
            {
                return Result<FixtureDailyDto>.Fail("System error", "SYSTEM_ERROR");
            }
        }

        public async Task<Result<string>> SaveFixtureDailyAsync(FixtureDailyDto model, HttpFileCollection files, string funcType, string type, string department)
        {
            try
            {
                if (model == null)
                {
                    throw new BusinessException("Fixture data is required");
                }

                if (string.IsNullOrWhiteSpace(model.EQ_SN))
                {
                    throw new BusinessException("Fixture SN is required");
                }

                var checkItems = model.CheckItems ?? new List<DailyItemDto>();
                if (!checkItems.Any())
                {
                   throw new BusinessException("Check items data is required");

                }

                var relativeRoot = ConfigurationManager.AppSettings["RelativeImgPath"] ?? "";

                var imageResultBuilder = new StringBuilder();

                //foreach (var item in model.CheckItems)
                for (int i = 0; i < checkItems.Count; i++)
                {
                    var item = checkItems[i];

                    var imagePaths = new List<string>();

                    // ảnh cũ
                    imagePaths.AddRange(
                        _fileService.NormalizeOldImagePaths(item.ExistingImagePaths, relativeRoot) //loại bỏ relative path của ảnh cũ trước khi lưu
                    );


                    // lưu ảnh và trả về path của ảnh mới
                    var newPaths = _fileService.SaveImageFiles(files, model.EQ_SN, item.MT_No, i);
                    item.NewImagePaths = newPaths;

                    imagePaths.AddRange(newPaths);


                    // build string business
                    var jsonArray = "[" + string.Join(", ", imagePaths.Select(p => $"\"{p}\"")) + "]";

                    imageResultBuilder.Append($"{item.MT_No}={jsonArray};");
                }

                var imageResult = imageResultBuilder.ToString().Trim(';');

                var uploadResult = string.Join(";",model.CheckItems
                    .Where(x => !string.IsNullOrWhiteSpace(x.MT_No))
                    .Select(x => x.MT_No + "=" + x.Upload_Result))?.Trim();

                var approveResult = string.Join(";", model.CheckItems
                    .Where(x => !string.IsNullOrWhiteSpace(x.MT_No))
                    .Select(x => x.MT_No + "=" + x.Approval_Result))?.Trim();

                var remarkResult = string.Join(";", model.CheckItems
                    .Where(x => !string.IsNullOrWhiteSpace(x.MT_No))
                    .Select(x => x.MT_No + "=" + x.Remark?.Trim()))?.Trim();

                var shift = string.IsNullOrWhiteSpace(model.Shift) ? "Day" : model.Shift;

                var uploadId = model.CheckItems.Select(x => x.Upload_ID).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                var approvalId = model.CheckItems.Select(x => x.Approval_ID).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                var extendedPara = BuildExtendedPara(new Dictionary<string, string>
                {
                    { "Upload_ID", uploadId },
                    { "Approval_ID", approvalId },
                    { "EQ_Name", model.EQ_Name },
                    { "EQ_Model", model.EQ_Model }
                });


                var param = new SaveFixtureParam
                {
                    FuncType = funcType,
                    Type = type,
                    Department = department,
                    Shift = shift,
                    ExtendedPara = extendedPara,
                    ImageResult = imageResult,
                    UploadResult = uploadResult,
                    ApproveResult = approveResult,
                    Remark = remarkResult,
                };

                var message = await _fixtureRepository.SaveFixtureDailyAsync(model, param);

                return Result<string>.Ok(string.Empty, message);
            }
            catch (SqlException)
            {
                return Result<string>.Fail("Database error", "DB_ERROR");
            }
            catch (BusinessException ex)
            {
                return Result<string>.Fail(ex.Message, "BUSINESS_ERROR");
            }
            catch (Exception)
            {
                return Result<string>.Fail("System error", "SYSTEM_ERROR");
            }
        }

        private string BuildExtendedPara(Dictionary<string, string> values)
        {
            if (values == null) return "";

            return string.Join(";", values
                .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => x.Key + "=" + EscapeSplitValue(x.Value)));
        }

        private string EscapeSplitValue(string value)
        {
            return value.ToText()
                .Replace(";", "；")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        public async Task<JObject> GetImageInfoAsync(HttpPostedFile file)
        {

            using (var ms = new MemoryStream())
            {
                file.InputStream.CopyTo(ms);
                string url = "https://10.36.8.55:5000/api/timestamp";

                byte[] bytes = ms.ToArray();
                string base64 = Convert.ToBase64String(bytes);
                string json = "{\"image_base64\":\"" + base64 + "\"}";

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,

                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls11
                };

                using (HttpClient client = new HttpClient(handler))
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    // 建议使用异步方式，避免死锁；
                    var response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return (JObject)JsonConvert.DeserializeObject(result);
                }
            }
        }
    }
}
