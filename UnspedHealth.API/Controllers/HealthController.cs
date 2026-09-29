using Microsoft.AspNetCore.Mvc;
using Serilog;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Interfaces;

namespace UnspedHealth.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly IHealthService _healthService;
        public HealthController(IHealthService healthService)
        {
            _healthService = healthService;
        }

        // 1. Basit Sinyal (Arka plan döngüsü için)
        [HttpPost("heartbeat/{serviceId}")]
        public async Task<IActionResult> Heartbeat([FromRoute] Guid serviceId)
        {
            if (serviceId == Guid.Empty) return BadRequest();

            var result = await _healthService.ReceiveHeartbeatAsync(serviceId);
            if (result) return Ok();

            return NotFound();
        }

        // 2. Detaylı Rapor (Manuel Hata/Bilgi Bildirimleri)
        [HttpPost("report")]
        public async Task<IActionResult> Report([FromBody] HeartbeatRequest request)
        {
            var result = await _healthService.ReceiveHeartbeatAsync(request);
            if (!result) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Message))
            {
                if (request.IsError)
                {
                    Log.Error("[SERVICE_FAULT] | ServiceID: {ServiceID} | Error: {ErrorMessage} | OccurredAt: {Timestamp}",
                        request.ServiceID,
                        request.Message,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    Log.Information("[SERVICE_INFO] | ServiceID: {ServiceID} | Status: {StatusMessage} | ReceivedAt: {Timestamp}",
                        request.ServiceID,
                        request.Message,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }

              return Ok();
        }
    }
}
