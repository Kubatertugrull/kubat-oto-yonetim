using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var data = await _dashboardService.GetDashboardSummaryAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpGet("aylik-satislar")]
    public async Task<IActionResult> GetAylikSatislar()
    {
        try
        {
            var data = await _dashboardService.GetAylikSatislarAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpGet("en-cok-satilan-urunler")]
    public async Task<IActionResult> GetEnCokSatilanUrunler()
    {
        try
        {
            var data = await _dashboardService.GetEnCokSatilanUrunlerAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpGet("en-cok-kar-eden-urunler")]
    public async Task<IActionResult> GetEnCokKarEdenUrunler()
    {
        try
        {
            var data = await _dashboardService.GetEnCokKarEdenUrunlerAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}

