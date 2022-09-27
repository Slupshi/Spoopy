using System;

namespace Spoopy.Models
{
    public class SpoopyLogs
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public DateTime Time { get; set; }

        public SpoopyLogs(string message, bool isError, DateTime time) 
        { 
            Message = message;
            IsError = isError;
            Time = time;            
        }
    }
}
