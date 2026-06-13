namespace Shared.Common
{
    public class ApiResponseNew<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public T Data { get; set; }

        public static ApiResponseNew<T> Ok(T data = default, string message = "Success")
        {
            return new ApiResponseNew<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponseNew<T> Fail(string message = "Fail", string errorCode = null)
        {
            return new ApiResponseNew<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }

    }
}
