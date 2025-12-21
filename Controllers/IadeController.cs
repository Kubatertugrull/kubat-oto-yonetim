using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class IadeController : ControllerBase
{
    private readonly IadeService _iadeService;

    public IadeController(IadeService iadeService)
    {
        _iadeService = iadeService;
    }

    [HttpGet("istatistikler")]
    public async Task<IActionResult> GetIadeIstatistikleri()
    {
        try
        {
            var data = await _iadeService.GetIadeIstatistikleriAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("bozuk-urun-tedarikci-dagilimi")]
    public async Task<IActionResult> GetBozukUrunTedarikciDagilimi()
    {
        try
        {
            var data = await _iadeService.GetBozukUrunTedarikciDagilimiAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetIadeler()
    {
        try
        {
            var data = await _iadeService.GetIadelerAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

