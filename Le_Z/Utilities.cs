using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Le_Z
{
    public class Utilities
    {
        public static string FormatToCode(string message) => $"```{message}```";

        public static string GetCustomTimestamp() => $"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Properties.Culture)} ";
    }
}
