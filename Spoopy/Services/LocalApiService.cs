using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Discord;
using Spoopy.Models;
using Spoopy.Variables;

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

        public static bool Ping()
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(hostname: "localhost", port: 7057);
                return true;
            }
            catch
            {
                return false;           
            }
        }

        #region Status

        public async Task<bool> PostSpoopyStatusAsync()
        {
            if (Properties.isLocalApiRunning)
            {
                try
                {
                    SpoopyStatus status = Utilities.GetSpoopyStatus();
                    Console.WriteLine("Posting Status");
                    return await _apiService.HttpPostAsync($"{_baseURL}status/status", status);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message.Contains("No connection could be made because the target machine actively refused it."))
                    {
                        Properties.isLocalApiRunning = false;
                        return false;
                    }
                    await Utilities.SpoopyLogAsync("Erreur dans PostSpoopyStatus", isError: true);
                    return false;
                }
            }
            else return false;
        }

        #endregion

        #region Logs

        public async Task<bool> PostSpoopyLogsAsync(SpoopyLogs logs)
        {
            if (Properties.isLocalApiRunning)
            {
                try
                {
                    Console.WriteLine("Posting Logs");
                    return await _apiService.HttpPostAsync($"{_baseURL}logs", logs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message.Contains("No connection could be made because the target machine actively refused it."))
                    {
                        Properties.isLocalApiRunning = false;
                        return false;
                    }
                    await Utilities.SpoopyLogAsync("Erreur dans PostSpoopyLogs", isError: true, isLogger: true);
                    return false;
                }
            }
            else return false;
            
        }

        #endregion

        #region Roles

        public async Task<IEnumerable<SpoopyRole>> GetBanquiseRolesAsync()
        {
            if (Properties.isLocalApiRunning)
            {
                try
                {
                    Console.WriteLine("Getting DBRoles");
                    return await _apiService.HttpGetAsync<IEnumerable<SpoopyRole>>($"{_baseURL}roles");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message.Contains("No connection could be made because the target machine actively refused it."))
                    {
                        Properties.isLocalApiRunning = false;
                        return null;
                    }
                    await Utilities.SpoopyLogAsync("Erreur dans GetBanquiseRoleByName", isError: true);
                    return null;
                }
            }
            else return null;
        }

        public async Task<SpoopyRole> GetBanquiseRoleByNameAsync(string name)
        {
            if (Properties.isLocalApiRunning)
            {
                try
                {
                    Console.WriteLine("Getting Role by name");
                    return await _apiService.HttpGetAsync<SpoopyRole>($"{_baseURL}roles/name/{name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message.Contains("No connection could be made because the target machine actively refused it."))
                    {
                        Properties.isLocalApiRunning = false;
                        return null;
                    }
                    await Utilities.SpoopyLogAsync("Erreur dans GetBanquiseRoleByName", isError: true);
                    return null;
                }
            }
            else return null;
        }

        public async Task<bool> PostBanquiseRoleAsync(SpoopyRole role)
        {
            if (Properties.isLocalApiRunning)
            {
                try
                {
                    Console.WriteLine($"Posting new Role : - {role.Name}");
                    return await _apiService.HttpPostAsync($"{_baseURL}roles", role);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message.Contains("No connection could be made because the target machine actively refused it."))
                    {
                        Properties.isLocalApiRunning = false;
                        return false;
                    }
                    await Utilities.SpoopyLogAsync("Erreur dans PostBanquiseRole", isError: true);
                    return false;
                }
            }
            else return false;            
        }

        public async Task<bool> PutBanquiseRoleAsync(SpoopyRole role)
        {
            if (Properties.isLocalApiRunning)
            {
                try
                {
                    Console.WriteLine($"Putting Role : #{role.Id} - {role.Name}");
                    return await _apiService.HttpPutAsync($"{_baseURL}roles/{role.Id}", role);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message.Contains("No connection could be made because the target machine actively refused it."))
                    {
                        Properties.isLocalApiRunning = false;
                        return false;
                    }
                    await Utilities.SpoopyLogAsync("Erreur dans PutBanquiseRole", isError: true);
                    return false;
                }
            }
            else return false;
        }

        public async Task<bool> DeleteBanquiseRoleAsync(SpoopyRole role)
        {
            if (Properties.isLocalApiRunning)
            {
                try
                {
                    Console.WriteLine($"Deleting Role : #{role.Id} - {role.Name}");
                    return await _apiService.HttpDeleteAsync($"{_baseURL}roles/{role.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message.Contains("No connection could be made because the target machine actively refused it."))
                    {
                        Properties.isLocalApiRunning = false;
                        return false;
                    }
                    await Utilities.SpoopyLogAsync("Erreur dans DeleteBanquiseRole", isError: true);
                    return false;
                }
            }
            else return false;
        }

        #endregion
    }
}
