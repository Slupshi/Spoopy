using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Spoopy.Services
{
    public class ApiService
    {
        public static async Task<object> HttpGet<T>(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();
                T jsonResponse = JsonConvert.DeserializeObject<T>(responseText);

                return jsonResponse;               
            }
        }
    }
}
