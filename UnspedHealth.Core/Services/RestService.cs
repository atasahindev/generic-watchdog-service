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

namespace UnspedHealth.Core.Services
{
    public class RestService : IRestService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RestService> _logger;

        public RestService(AppDbContext db, ILogger<RestService> logger)
        {
            _db = db;
            _logger = logger;
        }
        public async Task<bool> AddServerAsync(AddServerRequest req)
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
        public async Task<bool> DeleteServerAsync(Guid ID)
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
                    IsWindowsService = req.IsWindowsService ?? false,
                    AutoRestartEnabled = req.AutoRestartEnabled ?? false,
                    MonitoringMode = req.MonitoringMode,
                    ApiPort = req.ApiPort,
                    ContainerName = req.ContainerName?.Trim(),
                    BatchFilePath = req.BatchFilePath?.Trim(),
                    Type = req.Type?.Trim(),
                    HealthEndpoint = req.HealthEndpoint?.Trim(),
                    Contacts = req.Contacts?.Trim(),
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
    }
}
