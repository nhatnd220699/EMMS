using System.Collections.Generic;
using System.Web;

namespace Application.Interfaces
{
    public interface IFileService
    {
        List<string> SaveImageFiles(HttpFileCollection files, string eqSn, string mtNo, int itemIndex);
        List<string> NormalizeOldImagePaths(List<string> paths, string relativeRoot);
    }
}
