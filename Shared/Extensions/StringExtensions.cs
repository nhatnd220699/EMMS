using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extensions
{
    public static class StringExtensions
    {
        public static bool IsPass(this string result)
        {
            return string.Equals(result, "PASS", StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizeFilter(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            value = value.Trim();

            return value.StartsWith("All ", StringComparison.OrdinalIgnoreCase) ? "" : value;
        }
    }
}
