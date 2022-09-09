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
        private HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> ScrapHtmlPageAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        public async Task<T> HttpGetAsync<T>(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseText = await response.Content.ReadAsStringAsync();
            T jsonResponse = JsonConvert.DeserializeObject<T>(responseText);

            return jsonResponse;
        }
    }
}
