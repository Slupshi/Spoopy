using System;
using System.Threading.Tasks;
using Spoopy.Models;

namespace Spoopy.Services
{
    public class LocalApiService
    {
        private readonly ApiService _apiService;
        private string _baseURL;

        public LocalApiService(ApiService apiService)
        {
            _apiService = apiService;
            _baseURL = "https://localhost:7057/";
        }

        public async Task<bool> PostSpoopyStatus(SpoopyStatus status)
        {
            try
            {
                return await _apiService.HttpPostAsync($"{_baseURL}status", status);
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }


    }
}
