using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Application.Services
{
    public  class FileService : IFileService
    {
        public List<string> SaveImageFiles(HttpFileCollection files, string eqSn, string mtNo, int itemIndex)
        {
            var result = new List<string>();

            var rootFolder = ConfigurationManager.AppSettings["FixtureImageRoot"];

            var today = DateTime.Now;

            var subFolder = Path.Combine(
                today.Year.ToString(),
                today.Month.ToString("D2"),
                today.Day.ToString("D2"));

            var fullFolder = Path.Combine(rootFolder, subFolder);

            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            int index = 1;

            foreach (string key in files.AllKeys)
            {
                if (string.IsNullOrEmpty(key) || !key.Contains($"CheckItems[{itemIndex}].NewFiles"))
                    continue;

                var file = files[key];

                if (file == null || file.ContentLength == 0) continue;

                var ext = Path.GetExtension(file.FileName);

                var fileName = $"{eqSn}_{mtNo}_{index}_{Guid.NewGuid():N}{ext}";

                var physicalPath = Path.Combine(fullFolder, fileName);
                file.SaveAs(physicalPath);

                var relativePath = $"/{subFolder.Replace("\\", "/")}/{fileName}";

                result.Add(relativePath);
                index++;
            }

            return result;
        }


        public List<string> NormalizeOldImagePaths(List<string> paths, string relativeRoot)
        {
            return paths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p =>
                    p.StartsWith(relativeRoot, StringComparison.OrdinalIgnoreCase)
                        ? p.Substring(relativeRoot.Length)
                        : p)
                .ToList();
        }
    }
}
