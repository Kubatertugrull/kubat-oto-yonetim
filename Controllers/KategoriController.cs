using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class KategoriController : ControllerBase
{
    private readonly KategoriService _kategoriService;

    public KategoriController(KategoriService kategoriService)
    {
        _kategoriService = kategoriService;
    }

    [HttpGet]
    public async Task<IActionResult> GetKategoriler()
    {
        try
        {
            var data = await _kategoriService.GetKategorilerAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

