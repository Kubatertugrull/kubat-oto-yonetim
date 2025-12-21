using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class TedarikciController : ControllerBase
{
    private readonly TedarikciService _tedarikciService;

    public TedarikciController(TedarikciService tedarikciService)
    {
        _tedarikciService = tedarikciService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTedarikciler()
    {
        try
        {
            var data = await _tedarikciService.GetTedarikcilerAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("siparisler")]
    public async Task<IActionResult> GetTedarikSiparisleri()
    {
        try
        {
            var data = await _tedarikciService.GetTedarikSiparisleriAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

