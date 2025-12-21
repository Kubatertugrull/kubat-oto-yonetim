using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class SatisController : ControllerBase
{
    private readonly SatisService _satisService;

    public SatisController(SatisService satisService)
    {
        _satisService = satisService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSatislar()
    {
        try
        {
            var data = await _satisService.GetSatislarAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

