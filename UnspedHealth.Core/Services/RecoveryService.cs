using Microsoft.Extensions.Options;
using Serilog;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using UnspedHealth.Core.Interfaces;
using UnspedHealth.Core.Models;

namespace UnspedHealth.Core.Services
{
    public class RecoveryService : IRecoveryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public RecoveryService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(bool IsSuccess, string Message)> RestartRemoteProcessAsync(string serverIP, string processName, string executionPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serverIP)) throw new ArgumentException("[RestartRemoteProcessAsync] Sunucu IP adresi boş olamaz.");
                if (string.IsNullOrWhiteSpace(processName)) throw new ArgumentException("[RestartRemoteProcessAsync] İşlem adı boş olamaz.");
                if (string.IsNullOrWhiteSpace(executionPath)) throw new ArgumentException("[RestartRemoteProcessAsync] Çalıştırma yolu boş olamaz.");

                Log.Warning("[RestartRemoteProcessAsync] | {IP} üzerindeki Agent'a restart talebi gönderiliyor: {ProcessName}", serverIP, processName);

                using var client = _httpClientFactory.CreateClient();

                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("X-Agent-Key", AgentSettings.ApiKey);

                var requestUrl = $"http://{serverIP.Trim()}:{AgentSettings.DefaultPort}/process/restart";
                var restartRequest = new RestartRequest(processName.Trim(), executionPath.Trim());

                var response = await client.PostAsJsonAsync(requestUrl, restartRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AgentResponse>();

                    Log.Information("[RECOVERY_SUCCESS] | {IP} sunucusunda Agent işlemi onayladı: {Message}", serverIP, result?.Message);

                    return (true, result?.Message ?? "Başarılı");
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("[RECOVERY_ERROR] | Agent hata döndürdü. IP: {IP}, Kod: {Status}, Detay: {Detail}", serverIP, response.StatusCode, errorContent);

                return (false, $"Agent Hatası: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[RECOVERY_ERROR] | {IP} sunucusundaki Agent'a ulaşılamadı.", serverIP);
                return (false, $"Bağlantı Hatası: {ex.Message}");
            }
        }
        public async Task<(bool IsSuccess, string Message)> RestartLocalProcessAsync(string processName, string executionPath)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var p in processes)
                {
                    Log.Warning("[RECOVERY] | {ProcessName} sonlandırılıyor...", processName);
                    p.Kill();
                    await p.WaitForExitAsync();
                }

                Log.Information("[RECOVERY] | {ProcessName} başlatılıyor: {Path}", processName, executionPath);

                var startInfo = new ProcessStartInfo
                {
                    FileName = executionPath,
                    WorkingDirectory = Path.GetDirectoryName(executionPath),
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                return (true, "");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[RECOVERY_ERROR] | Müdahale başarısız.");
                return (false, ex.Message);
            }
        }
        public async Task<(bool IsSuccess, string Message)> RestartWindowsServiceAsync(string serverIP, string serviceName, string executionPath)
        {
            try
            {
                Log.Information("Restarting Windows Service: {ServiceName} on Server: {ServerIP}", serviceName, serverIP);
                // Simulate some asynchronous operation
                await Task.Delay(1000);
                return (true, $"Service {serviceName} restarted successfully on server {serverIP}.");
            }
            catch (Exception)
            {
                Log.Error("Failed to restart Windows Service: {ServiceName} on Server: {ServerIP}", serviceName, serverIP);
                return (false, $"Failed to restart service {serviceName} on server {serverIP}.");
            }
        }
    }
}
