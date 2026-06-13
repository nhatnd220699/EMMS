using System;
namespace Shared.Common
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) {}
    }
}
