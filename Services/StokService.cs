using MySqlConnector;
using Services;

namespace Services;

public class StokService
{
    private readonly DatabaseService _dbService;

    public StokService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<List<object>> GetStokDurumuAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                u.UrunID,
                u.UrunAdi,
                u.StokMiktari,
                u.AlisFiyati,
                u.SatisFiyati,
                CASE 
                    WHEN u.KritikStokSeviyesi IS NOT NULL AND u.StokMiktari <= u.KritikStokSeviyesi THEN 'Kritik'
                    ELSE 'Normal'
                END as StokDurumu
            FROM urunler u
            ORDER BY 
                CASE 
                    WHEN u.KritikStokSeviyesi IS NOT NULL AND u.StokMiktari <= u.KritikStokSeviyesi THEN 1
                    ELSE 2
                END,
                u.UrunAdi
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                urunID = reader.GetInt32("UrunID"),
                urunAdi = reader.GetString("UrunAdi"),
                stokMiktari = reader.GetInt32("StokMiktari"),
                alisFiyati = reader.GetDecimal("AlisFiyati"),
                satisFiyati = reader.GetDecimal("SatisFiyati"),
                stokDurumu = reader.GetString("StokDurumu")
            });
        }
        return result;
    }
}

