using System.Net.Http;
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

        public async Task<bool> HttpPostAsync(string url, object model)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, model);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> HttpPutAsync(string url, object model)
        {
            HttpResponseMessage responseMessage = await _httpClient.PutAsJsonAsync(url, model);
            responseMessage.EnsureSuccessStatusCode();
            return responseMessage.IsSuccessStatusCode;
        }

        public async Task<bool> HttpDeleteAsync(string url)
        {
            HttpResponseMessage responseMessage = await _httpClient.DeleteAsync(url);
            responseMessage.EnsureSuccessStatusCode();
            return responseMessage.IsSuccessStatusCode;
        }

    }
}
