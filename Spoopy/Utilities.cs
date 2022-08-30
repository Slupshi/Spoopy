﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom.Events;
using TwitterSharp.Response.RTweet;
using AngleSharp.Html.Dom;

namespace Spoopy
{
    public class Utilities
    {
        public static string FormatToCode(string message) => $"```{message}```";

        public static string GetCustomTimestamp() => $"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Properties.Culture)} ";

        public static string DeleteUrlFromText(string text)
        {
            List<string> words = text.Split("http").ToList();
            words.RemoveAll(i => i.Contains("://"));
            string textToReturn = string.Join(" ", words);
            return textToReturn;
        }

        public static async Task<int> GetParisTimeZoneAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string path = "http://worldtimeapi.org/api/timezone/Europe/Paris";
                HttpResponseMessage response = await httpClient.GetAsync(path);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();
                TimeZoneModel timeZone = JsonConvert.DeserializeObject<TimeZoneModel>(responseText);

                if (timeZone.UTC_offset == "+01:00") return 1;
                else return 2;
            }
        }

        public static string DeFactory(string nextWord, bool isUpperCase = false)
        {
            string response = isUpperCase ? "De " : "de ";
            char firstLetter = char.ToUpperInvariant(nextWord.First());
            if(Properties.Vowels.Any(x=> x == firstLetter))
            {
                if(!(firstLetter == 'Y' && Properties.Vowels.Any(x => x == char.ToUpperInvariant(nextWord.ElementAt(1)))))
                {
                    response = isUpperCase ? "D'" : "d'";
                }
            }
            return response;
        }
    }

    public class TimeZoneModel
    {
        public string UTC_offset { get; set; }
    }
}
