using System;
using System.Collections.Generic;
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
            _baseURL = "https://localhost:7057/api/";
        }

        #region Status

        public async Task<bool> PostSpoopyStatusAsync(SpoopyStatus status)
        {
            try
            {
                Console.WriteLine("Posting Status");
                return await _apiService.HttpPostAsync($"{_baseURL}status/status", status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("No connection could be made because the target machine actively refused it.")) return false;
                await Program.ZLog("Erreur dans PostSpoopyStatus", isError: true);
                return false;
            }
            
        }

        #endregion

        #region Logs

        public async Task<bool> PostSpoopyLogsAsync(SpoopyLogs logs)
        {
            try
            {
                Console.WriteLine("Posting Logs");
                return await _apiService.HttpPostAsync($"{_baseURL}logs", logs);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("No connection could be made because the target machine actively refused it.")) return false;
                await Program.ZLog("Erreur dans PostSpoopyLogs", isError: true, isLogger: true);
                return false;
            }
        }

        #endregion

        #region Roles

        public async Task<IEnumerable<SpoopyRole>> GetBanquiseRolesAsync()
        {
            try
            {
                Console.WriteLine("Getting Roles");
                return await _apiService.HttpGetAsync<IEnumerable<SpoopyRole>>($"{_baseURL}roles");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("No connection could be made because the target machine actively refused it.")) return null;
                await Program.ZLog("Erreur dans GetBanquiseRoleByName", isError: true);
                return null;
            }
        }

        public async Task<SpoopyRole> GetBanquiseRoleByNameAsync(string name)
        {
            try
            {
                Console.WriteLine("Getting Role by name");
                return await _apiService.HttpGetAsync<SpoopyRole>($"{_baseURL}roles/name/{name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("No connection could be made because the target machine actively refused it.")) return null;
                await Program.ZLog("Erreur dans GetBanquiseRoleByName", isError: true);
                return null;
            }
        }

        public async Task<bool> PostBanquiseRoleAsync(SpoopyRole role)
        {
            try
            {
                Console.WriteLine("Posting Role");
                return await _apiService.HttpPostAsync($"{_baseURL}roles", role);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("No connection could be made because the target machine actively refused it.")) return false;
                await Program.ZLog("Erreur dans PostBanquiseRole", isError: true);
                return false;
            }
        }

        public async Task<bool> PutBanquiseRoleAsync(SpoopyRole role)
        {
            try
            {
                Console.WriteLine("Putting Role");
                return await _apiService.HttpPutAsync($"{_baseURL}roles/{role.Id}", role);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("No connection could be made because the target machine actively refused it.")) return false;
                await Program.ZLog("Erreur dans PutBanquiseRole", isError: true);
                return false;
            }
        }

        #endregion
    }
}
