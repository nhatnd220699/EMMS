namespace Shared.Common
{
    public class DataDto<T>
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}