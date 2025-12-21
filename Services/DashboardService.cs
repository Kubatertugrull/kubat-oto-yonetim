using MySqlConnector;
using Services;

namespace Services;

public class DashboardService
{
    private readonly DatabaseService _dbService;

    public DashboardService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<object> GetDashboardSummaryAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        
        // Satış istatistikleri
        var satisCmd = new MySqlCommand(@"
            SELECT 
                COUNT(DISTINCT s.SatisID) as ToplamSatis,
                IFNULL(SUM(sd.BirimSatisFiyati * sd.Adet), 0) as ToplamCiro,
                IFNULL(SUM((sd.BirimSatisFiyati - sd.BirimMaliyet) * sd.Adet), 0) as ToplamKar
            FROM satislar s
            LEFT JOIN satis_detaylar sd ON s.SatisID = sd.SatisID
            WHERE YEAR(s.SatisTarihi) = YEAR(CURDATE())
        ", connection);

        int toplamSatis = 0;
        decimal toplamCiro = 0;
        decimal toplamKar = 0;

        await using (var reader = await satisCmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                toplamSatis = reader.GetInt32("ToplamSatis");
                toplamCiro = reader.IsDBNull(reader.GetOrdinal("ToplamCiro")) ? 0 : reader.GetDecimal("ToplamCiro");
                toplamKar = reader.IsDBNull(reader.GetOrdinal("ToplamKar")) ? 0 : reader.GetDecimal("ToplamKar");
            }
        }

        // Kritik stok sayısı
        var kritikStokCmd = new MySqlCommand(@"
            SELECT COUNT(*) as KritikStok
            FROM urunler
            WHERE KritikStokSeviyesi IS NOT NULL 
              AND StokMiktari <= KritikStokSeviyesi
        ", connection);

        int kritikStok = 0;
        await using (var reader = await kritikStokCmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                kritikStok = reader.GetInt32("KritikStok");
            }
        }

        return new
        {
            toplamSatis,
            toplamCiro,
            toplamKar,
            kritikStok
        };
    }

    public async Task<List<object>> GetAylikSatislarAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        
        // Önce tüm aylar için 0 değerli bir dictionary oluştur
        var aylikData = new Dictionary<int, (decimal ciro, decimal kar)>();
        for (int ay = 1; ay <= 12; ay++)
        {
            aylikData[ay] = (0, 0);
        }
        
        // Veritabanından gelen verileri doldur
        var cmd = new MySqlCommand(@"
            SELECT 
                MONTH(s.SatisTarihi) as Ay,
                IFNULL(SUM(sd.BirimSatisFiyati * sd.Adet), 0) as Ciro,
                IFNULL(SUM((sd.BirimSatisFiyati - sd.BirimMaliyet) * sd.Adet), 0) as Kar
            FROM satislar s
            LEFT JOIN satis_detaylar sd ON s.SatisID = sd.SatisID
            WHERE YEAR(s.SatisTarihi) = YEAR(CURDATE())
              AND sd.BirimSatisFiyati IS NOT NULL
              AND sd.Adet IS NOT NULL
              AND sd.BirimMaliyet IS NOT NULL
            GROUP BY MONTH(s.SatisTarihi)
            ORDER BY Ay
        ", connection);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            int ay = reader.GetInt32("Ay");
            decimal ciro = reader.IsDBNull(reader.GetOrdinal("Ciro")) 
                ? 0 
                : reader.GetDecimal("Ciro");
            decimal kar = reader.IsDBNull(reader.GetOrdinal("Kar")) 
                ? 0 
                : reader.GetDecimal("Kar");
            
            aylikData[ay] = (ciro, kar);
        }
        
        // Tüm ayları sıralı olarak döndür
        var result = new List<object>();
        for (int ay = 1; ay <= 12; ay++)
        {
            result.Add(new
            {
                ay = ay,
                ciro = aylikData[ay].ciro,
                kar = aylikData[ay].kar
            });
        }
        
        return result;
    }

    public async Task<List<object>> GetEnCokSatilanUrunlerAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                u.UrunAdi,
                SUM(sd.Adet) as ToplamMiktar,
                SUM(sd.BirimSatisFiyati * sd.Adet) as ToplamTutar
            FROM satis_detaylar sd
            JOIN urunler u ON sd.UrunID = u.UrunID
            JOIN satislar s ON sd.SatisID = s.SatisID
            WHERE YEAR(s.SatisTarihi) = YEAR(CURDATE())
            GROUP BY u.UrunID, u.UrunAdi
            ORDER BY ToplamMiktar DESC
            LIMIT 3
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                urunAdi = reader.GetString("UrunAdi"),
                toplamMiktar = reader.GetInt32("ToplamMiktar"),
                toplamTutar = reader.GetDecimal("ToplamTutar")
            });
        }
        return result;
    }

    public async Task<List<object>> GetEnCokKarEdenUrunlerAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                u.UrunAdi,
                SUM((sd.BirimSatisFiyati - sd.BirimMaliyet) * sd.Adet) as ToplamKar
            FROM satis_detaylar sd
            JOIN urunler u ON sd.UrunID = u.UrunID
            JOIN satislar s ON sd.SatisID = s.SatisID
            WHERE YEAR(s.SatisTarihi) = YEAR(CURDATE())
            GROUP BY u.UrunID, u.UrunAdi
            ORDER BY ToplamKar DESC
            LIMIT 3
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                urunAdi = reader.GetString("UrunAdi"),
                toplamKar = reader.GetDecimal("ToplamKar")
            });
        }
        return result;
    }
}

