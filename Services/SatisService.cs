using MySqlConnector;
using Services;

namespace Services;

public class SatisService
{
    private readonly DatabaseService _dbService;

    public SatisService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<List<object>> GetSatislarAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                s.SatisID,
                s.SatisTarihi,
                s.FaturaNo,
                s.ToplamTutar as Tutar,
                SUM((sd.BirimSatisFiyati - sd.BirimMaliyet) * sd.Adet) as Kar,
                COUNT(sd.SatisDetayID) as UrunSayisi
            FROM satislar s
            LEFT JOIN satis_detaylar sd ON s.SatisID = sd.SatisID
            GROUP BY s.SatisID, s.SatisTarihi, s.FaturaNo, s.ToplamTutar
            ORDER BY s.SatisTarihi DESC
            LIMIT 5
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                satisID = reader.GetInt32("SatisID"),
                satisTarihi = reader.GetDateTime("SatisTarihi"),
                faturaNo = reader.GetString("FaturaNo"),
                tutar = reader.GetDecimal("Tutar"),
                kar = reader.GetDecimal("Kar"),
                urunSayisi = reader.GetInt32("UrunSayisi")
            });
        }
        return result;
    }
}

