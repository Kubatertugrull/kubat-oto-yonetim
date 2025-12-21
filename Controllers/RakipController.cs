using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text.Json;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class RakipController : ControllerBase
{
    private readonly RakipService _rakipService;

    public RakipController(RakipService rakipService)
    {
        _rakipService = rakipService;
    }

    [HttpGet("analiz")]
    public async Task<IActionResult> GetRakipAnalizi()
    {
        try
        {
            var data = await _rakipService.GetRakipAnaliziAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPut("urunler/{urunID}/fiyat")]
    public async Task<IActionResult> UpdateUrunFiyati(int urunID)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var jsonDoc = JsonDocument.Parse(body);
            
            if (!jsonDoc.RootElement.TryGetProperty("yeniFiyat", out var yeniFiyatElement))
            {
                return BadRequest(new { success = false, message = "yeniFiyat parametresi gerekli" });
            }

            if (!yeniFiyatElement.TryGetDecimal(out var yeniFiyat))
            {
                return BadRequest(new { success = false, message = "Geçersiz fiyat değeri" });
            }

            var result = await _rakipService.UpdateUrunFiyatiAsync(urunID, yeniFiyat);
            if (result)
            {
                return Ok(new { success = true, message = "Fiyat güncellendi" });
            }
            return NotFound(new { success = false, message = "Ürün bulunamadı" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

