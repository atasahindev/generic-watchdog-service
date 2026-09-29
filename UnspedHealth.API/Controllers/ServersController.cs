using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Interfaces;
using UnspedHealth.Core.Services;

namespace UnspedHealth.API.Controllers
{
    [Route("servers")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly IRestService _restService;
        public ServersController(IRestService restService)
        {
            _restService = restService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] AddServerRequest request)
        {
            var result = await _restService.AddServerAsync(request);
            if (result) return Ok();

            return BadRequest();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(Guid ID)
        {
            var result = await _restService.DeleteServerAsync(ID);
            if (result) return Ok();

            return BadRequest();
        }
    }
}
