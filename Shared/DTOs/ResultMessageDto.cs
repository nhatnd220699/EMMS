using System;

namespace Shared.DTOs
{
    public class ResultMessageDto
    {
        public string Result { get; set; }
        public string Message { get; set; }

        // Some legacy CATCH blocks return iResult/iMessage instead of Result/Message.
        public string IResult { get; set; }
        public string IMessage { get; set; }

        public string ResultValue
        {
            get { return string.IsNullOrWhiteSpace(Result) ? IResult : Result; }
        }

        public string MessageValue
        {
            get { return string.IsNullOrWhiteSpace(Message) ? IMessage : Message; }
        }

        public bool IsPass
        {
            get { return string.Equals(ResultValue, "PASS", StringComparison.OrdinalIgnoreCase); }
        }
    }
}
