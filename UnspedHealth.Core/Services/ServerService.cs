using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnspedHealth.API.Persistence;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Entities;
using UnspedHealth.Core.Interfaces;
using static UnspedHealth.Core.Enums;

namespace UnspedHealth.Core.Services
{
    public class ServerService : IServerService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ServerService> _logger;

        public ServerService(AppDbContext db, ILogger<ServerService> logger)
        {
            _db = db;
            _logger = logger;
        }
        public async Task<List<Server>> CheckAllServersAsync()
        {
            var downedServers = new List<Server>();
            var servers = await _db.Servers.Where(s => s.IsActive).ToListAsync();

            foreach (var server in servers)
            {
                bool isAlive = await PingServerAsync(server.IpAddress);

                if (!isAlive && server.Status == 1)
                {
                    server.Status = 0;
                    server.LastDownTime = DateTime.Now;
                    downedServers.Add(server);

                    _logger.LogWarning("[SERVER_OFFLINE] {Name} - ({IP}) erişilemez durumda.", server.ServerName, server.IpAddress);
                }

                else if (isAlive && server.Status == 0)
                {
                    server.Status = 1;
                    server.LastUpTime = DateTime.Now;
                    _logger.LogInformation("[SERVER_ONLINE] {Name} - ({IP}) tekrar erişilebilir.", server.ServerName, server.IpAddress);
                }

                server.LastPingTime = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            return downedServers;
        }
        public async Task<bool> AddAsync(AddServerRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.IpAddress))
                    return false;

                if (await _db.Servers.AnyAsync(x => x.IpAddress == req.IpAddress))
                    return false;

                var server = new Server
                {
                    IpAddress = req.IpAddress.Trim(),
                    ServerName = req.ServerName?.Trim() ?? req.IpAddress.Trim(),
                    IsActive = req.IsActive,
                    Status = 1,
                    CreatedAt = DateTime.Now
                };

                await _db.Servers.AddAsync(server);
                return await _db.SaveChangesAsync() > 0;
            }

            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> DeleteAsync(Guid ID)
        {
            try
            {
                int affectedRows = await _db.Servers.Where(x => x.ID == ID).ExecuteDeleteAsync();

                if (affectedRows > 0)
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> AddApplicationAsync(AddServiceRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.ServicePath))
                    return false;

                if (string.IsNullOrWhiteSpace(req.ServerIP))
                    return false;

                if (await _db.ServiceHealthChecks.AnyAsync(x => x.ServicePath == req.ServicePath))
                    return false;

                var app = new ServiceHealthCheck
                {
                    ApplicationName = req.ApplicationName.Trim(),
                    ServicePath = req.ServicePath.Trim(),
                    ServerIP = req.ServerIP.Trim(),
                    IsActive = req.IsActive,
                    Status = 1,
                    CheckInterval = req.CheckInterval ?? 300,
                    IsNotifyEnabled = req.IsNotifyEnabled,
                    Description = req.Description?.Trim(),
                    WindowsServiceName = req.WindowsServiceName?.Trim(),
                    IsWindowsService = req.IsWindowsService,
                    AutoRestartEnabled = req.AutoRestartEnabled,
                    MonitoringMode = req.MonitoringMode,
                    ApiPort = req.ApiPort,
                    ContainerName = req.ContainerName?.Trim(),
                    Type = req.Type?.Trim(),
                };

                await _db.ServiceHealthChecks.AddAsync(app);
                return await _db.SaveChangesAsync() > 0;
            }

            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> DeleteApplicationAsync(Guid ID)
        {
            try
            {
                int affectedRows = await _db.ServiceHealthChecks.Where(x => x.ID == ID).ExecuteDeleteAsync();

                if (affectedRows > 0)
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Belirtilen IP adresine ICMP Ping paketi gönderir.
        /// </summary>
        /// <param name="ipAddress">Hedef IP veya Hostname</param>
        /// <returns>Başarılı ise true, aksi halde false.</returns>
        private async Task<bool> PingServerAsync(string ipAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ipAddress, 3000); // 3 sn timeout
                    return reply.Status == IPStatus.Success;
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Alternatif kontrol: Belirli bir portun (Örn: 1433, 80) açık olup olmadığını kontrol eder.
        /// </summary>
        private async Task<bool> CheckPortAsync(string ipAddress, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var task = client.ConnectAsync(ipAddress, port);
                    if (await Task.WhenAny(task, Task.Delay(3000)) == task) { return client.Connected; }
                    return false;
                }
            }
            catch { return false; }
        }
    }
}
