using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnspedHealth.API.Persistence;
using UnspedHealth.Core.Entities;
using UnspedHealth.Core.Interfaces;

namespace UnspedHealth.Core.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;
        public ReportService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<CustomResponse<List<Server>>> GetAllServerHealthsAsync()
        {
            try
            {
                var servers = await _db.Servers
                    .AsNoTracking()
                    .OrderByDescending(s => s.LastPingTime)
                    .ToListAsync();

                return CustomResponse<List<Server>>.Success(servers, new List<string> { "Server healths retrieved successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ReportService] Sunucu verileri çekilirken hata oluştu.");
                return CustomResponse<List<Server>>.Fail(new List<string> { ex.Message });
            }
        }

        public async Task<CustomResponse<List<ServiceHealthCheck>>> GetAllServiceHealthsAsync()
        {
            try
            {
                var services = await _db.ServiceHealthChecks
                    .AsNoTracking()
                    .OrderByDescending(s => s.LastHeartbeat)
                    .ToListAsync();

                return CustomResponse<List<ServiceHealthCheck>>.Success(services, new List<string> { "Service healths retrieved successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ReportService] Servis verileri çekilirken hata oluştu.");
                return CustomResponse<List<ServiceHealthCheck>>.Fail(new List<string> { ex.Message });
            }
        }
    }
}
