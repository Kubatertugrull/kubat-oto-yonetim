using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class StokController : ControllerBase
{
    private readonly StokService _stokService;

    public StokController(StokService stokService)
    {
        _stokService = stokService;
    }

    [HttpGet("durumu")]
    public async Task<IActionResult> GetStokDurumu()
    {
        try
        {
            var data = await _stokService.GetStokDurumuAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

