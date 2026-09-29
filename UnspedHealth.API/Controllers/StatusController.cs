using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnspedHealth.Core.Interfaces;
using UnspedHealth.Core.Services;

namespace UnspedHealth.API.Controllers
{
    [ApiController]
    [Route("report")]
    public class StatusController : ControllerBase
    {
        private readonly IReportService _reportService;

        public StatusController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("services")]
        public async Task<IActionResult> Services()
        {

            var result = await _reportService.GetAllServiceHealthsAsync();
            if (result.Successful) return Ok(result?.Data);

            return NotFound();
        }

        [HttpGet("servers")]
        public async Task<IActionResult> Servers()
        {
            var result = await _reportService.GetAllServerHealthsAsync();
            if (result.Successful) return Ok(result?.Data);

            return NotFound();
        }
    }
}
