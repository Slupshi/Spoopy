using System;

namespace Spoopy.Models
{
    public class SpoopyStatus
    {
        public TimeSpan Uptime { get; set; }
        public bool IsRunning { get; set; }
        public TimeSpan Runtime { get; set; }

        public SpoopyStatus(TimeSpan uptime, TimeSpan runtime, bool isConnected) 
        {
            Uptime = uptime;
            Runtime = runtime;
            IsRunning = isConnected;
        }
    }
}
