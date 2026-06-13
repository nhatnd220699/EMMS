namespace Shared.Common
{
    public class Result<T>
    {
        public bool Success { get; }
        public string Message { get; }
        public string ErrorCode { get; }
        public T Data { get; }

        private Result(bool success, string message, string errorCode, T data)
        {
            Success = success;
            Message = message;
            ErrorCode = errorCode;
            Data = data;        

        }

        public static Result<T> Ok(T data = default, string message = "Success")
        {
            return new Result<T>(true, message, null, data);
        }

        public static Result<T> Fail(string message = "Fail", string errorCode = null)
        {
            return new Result<T>(false, message, errorCode, default);
        }
    }
}
