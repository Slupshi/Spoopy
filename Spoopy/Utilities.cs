using System;

namespace Spoopy
{
    public class Utilities
    {
        public static string FormatToCode(string message) => $"```{message}```";

        public static string GetCustomTimestamp() => $"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Properties.Culture)} ";
    }
}
