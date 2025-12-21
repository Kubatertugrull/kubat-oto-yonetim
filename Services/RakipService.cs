using MySqlConnector;
using Services;

namespace Services;

public class RakipService
{
    private readonly DatabaseService _dbService;

    public RakipService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<List<object>> GetRakipAnaliziAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        
        // Önce rakip isimlerini çek
        var rakipIsimleriCmd = new MySqlCommand("SELECT DISTINCT RakipAdi FROM rakipler ORDER BY RakipAdi", connection);
        var rakipIsimleri = new List<string>();
        await using (var rakipReader = await rakipIsimleriCmd.ExecuteReaderAsync())
        {
            while (await rakipReader.ReadAsync())
            {
                rakipIsimleri.Add(rakipReader.GetString("RakipAdi"));
            }
        }

        // Tüm ürün ve rakip fiyatlarını bir kerede çek
        var cmd = new MySqlCommand(@"
            SELECT 
                u.UrunID,
                u.UrunAdi,
                u.SatisFiyati as BizimFiyat,
                r.RakipAdi,
                r.RakipFiyati
            FROM urunler u
            INNER JOIN rakipler r ON u.UrunID = r.UrunID
            ORDER BY u.UrunAdi, r.RakipAdi
        ", connection);

        var urunlerDict = new Dictionary<int, Dictionary<string, object>>();
        
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var urunID = reader.GetInt32("UrunID");
            
            if (!urunlerDict.ContainsKey(urunID))
            {
                urunlerDict[urunID] = new Dictionary<string, object>
                {
                    { "urunID", urunID },
                    { "urunAdi", reader.GetString("UrunAdi") },
                    { "bizimFiyat", reader.GetDecimal("BizimFiyat") },
                    { "rakipFiyatlariDict", new Dictionary<string, decimal?>() }
                };
            }
            
            var rakipAdi = reader.GetString("RakipAdi");
            var rakipFiyati = reader.GetDecimal("RakipFiyati");
            var rakipFiyatlariDict = (Dictionary<string, decimal?>)urunlerDict[urunID]["rakipFiyatlariDict"];
            
            // Her rakip için en son fiyatı sakla (Tarih DESC ile sıralandığı için ilk gelen en son)
            if (!rakipFiyatlariDict.ContainsKey(rakipAdi))
            {
                rakipFiyatlariDict[rakipAdi] = rakipFiyati;
            }
        }

        // Dictionary'yi list'e çevir
        var result = new List<object>();
        foreach (var urun in urunlerDict.Values)
        {
            var rakipFiyatlariDict = (Dictionary<string, decimal?>)urun["rakipFiyatlariDict"];
            var rakipFiyatlariList = new List<decimal?>();
            
            foreach (var rakipAdi in rakipIsimleri)
            {
                rakipFiyatlariList.Add(rakipFiyatlariDict.ContainsKey(rakipAdi) ? rakipFiyatlariDict[rakipAdi] : null);
            }
            
            result.Add(new
            {
                urunID = urun["urunID"],
                urunAdi = urun["urunAdi"],
                bizimFiyat = urun["bizimFiyat"],
                rakipIsimleri = rakipIsimleri,
                rakipFiyatlari = rakipFiyatlariList
            });
        }
        
        return result;
    }

    public async Task<bool> UpdateUrunFiyatiAsync(int urunID, decimal yeniFiyat)
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand("UPDATE urunler SET SatisFiyati = @yeniFiyat WHERE UrunID = @urunID", connection);
        cmd.Parameters.AddWithValue("@yeniFiyat", yeniFiyat);
        cmd.Parameters.AddWithValue("@urunID", urunID);
        int affectedRows = await cmd.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }
}

