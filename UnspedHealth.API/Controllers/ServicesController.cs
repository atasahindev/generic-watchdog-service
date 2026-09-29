using Microsoft.AspNetCore.Mvc;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Interfaces;

namespace UnspedHealth.API.Controllers
{
    [Route("services")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IRestService _restService;
        public ServicesController(IRestService restService)
        {
            _restService = restService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] AddServiceRequest request)
        {
            var result = await _restService.AddApplicationAsync(request);
            if (result) return Created();

            return BadRequest();
        }

        [HttpDelete("delete/{ID}")]
        public async Task<IActionResult> Delete([FromRoute] Guid ID)
        {
            var result = await _restService.DeleteApplicationAsync(ID);
            if (result) return Ok();

            return BadRequest();
        }
    }
}
