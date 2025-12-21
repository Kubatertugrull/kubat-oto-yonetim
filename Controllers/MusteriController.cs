using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusteriController : ControllerBase
{
    private readonly MusteriService _musteriService;

    public MusteriController(MusteriService musteriService)
    {
        _musteriService = musteriService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMusteriler()
    {
        try
        {
            var data = await _musteriService.GetMusterilerAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("ilce-dagilimi")]
    public async Task<IActionResult> GetIlceDagilimi()
    {
        try
        {
            var data = await _musteriService.GetMusteriIlceDagilimiAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("ilce/{ilce}")]
    public async Task<IActionResult> GetMusterilerByIlce(string ilce)
    {
        try
        {
            var data = await _musteriService.GetMusterilerByIlceAsync(ilce);
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("en-cok-satis-yapan")]
    public async Task<IActionResult> GetEnCokSatisYapanMusteriler()
    {
        try
        {
            var data = await _musteriService.GetEnCokSatisYapanMusterilerAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("en-cok-kar-eden")]
    public async Task<IActionResult> GetEnCokKarEdenMusteriler()
    {
        try
        {
            var data = await _musteriService.GetEnCokKarEdenMusterilerAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

