using MySqlConnector;
using Services;

namespace Services;

public class MusteriService
{
    private readonly DatabaseService _dbService;

    public MusteriService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<List<object>> GetMusterilerAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                m.MusteriID,
                m.AdSoyad,
                m.Ilce,
                m.MusteriTipi,
                m.Telefon,
                m.Email,
                IFNULL(SUM(sd.Adet), 0) as ToplamUrunSayisi
            FROM musteriler m
            LEFT JOIN satislar s ON m.MusteriID = s.MusteriID
            LEFT JOIN satis_detaylar sd ON s.SatisID = sd.SatisID
            GROUP BY m.MusteriID, m.AdSoyad, m.Ilce, m.MusteriTipi, m.Telefon, m.Email
            ORDER BY ToplamUrunSayisi DESC
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        
        // İlk önce tüm verileri oku ve toplamı hesapla
        var tempList = new List<dynamic>();
        int toplamUrunSayisi = 0;

        while (await reader.ReadAsync())
        {
            int urunSayisiOrd = reader.GetOrdinal("ToplamUrunSayisi");
            int telefonOrd = reader.GetOrdinal("Telefon");
            int emailOrd = reader.GetOrdinal("Email");
            int ilceOrd = reader.GetOrdinal("Ilce");
            int musteriTipiOrd = reader.GetOrdinal("MusteriTipi");
            
            var urunSayisi = reader.IsDBNull(urunSayisiOrd) 
                ? 0 
                : Convert.ToInt32(reader.GetDecimal(urunSayisiOrd));
            toplamUrunSayisi += urunSayisi;

            tempList.Add(new
            {
                musteriID = reader.GetInt32("MusteriID"),
                adSoyad = reader.GetString("AdSoyad"),
                ilce = reader.IsDBNull(ilceOrd) ? null : reader.GetString(ilceOrd),
                musteriTipi = reader.IsDBNull(musteriTipiOrd) ? null : reader.GetString(musteriTipiOrd),
                telefon = reader.IsDBNull(telefonOrd) ? null : reader.GetString(telefonOrd),
                email = reader.IsDBNull(emailOrd) ? null : reader.GetString(emailOrd),
                toplamUrunSayisi = urunSayisi
            });
        }

        // Yüzde hesaplamalarını yap
        foreach (var item in tempList)
        {
            var yuzde = toplamUrunSayisi > 0 ? (double)item.toplamUrunSayisi / toplamUrunSayisi * 100 : 0;

            result.Add(new
            {
                musteriID = item.musteriID,
                adSoyad = item.adSoyad,
                ilce = item.ilce,
                musteriTipi = item.musteriTipi,
                telefon = item.telefon,
                email = item.email,
                toplamUrunSayisi = item.toplamUrunSayisi,
                yuzde = Math.Round(yuzde, 2)
            });
        }

        return result;
    }

    public async Task<List<object>> GetMusteriIlceDagilimiAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                m.Ilce,
                COUNT(DISTINCT m.MusteriID) as MusteriSayisi
            FROM musteriler m
            WHERE m.Ilce IS NOT NULL AND m.Ilce != ''
            GROUP BY m.Ilce
            ORDER BY MusteriSayisi DESC
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                ilce = reader.GetString("Ilce"),
                musteriSayisi = reader.GetInt32("MusteriSayisi")
            });
        }
        return result;
    }

    public async Task<List<object>> GetMusterilerByIlceAsync(string ilce)
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                m.MusteriID,
                m.AdSoyad,
                m.Ilce,
                m.MusteriTipi,
                m.Telefon,
                m.Email
            FROM musteriler m
            WHERE m.Ilce = @Ilce
            ORDER BY m.AdSoyad
        ", connection);

        cmd.Parameters.AddWithValue("@Ilce", ilce);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            int telefonOrd = reader.GetOrdinal("Telefon");
            int emailOrd = reader.GetOrdinal("Email");
            int musteriTipiOrd = reader.GetOrdinal("MusteriTipi");

            result.Add(new
            {
                musteriID = reader.GetInt32("MusteriID"),
                adSoyad = reader.GetString("AdSoyad"),
                ilce = reader.GetString("Ilce"),
                musteriTipi = reader.IsDBNull(musteriTipiOrd) ? null : reader.GetString(musteriTipiOrd),
                telefon = reader.IsDBNull(telefonOrd) ? null : reader.GetString(telefonOrd),
                email = reader.IsDBNull(emailOrd) ? null : reader.GetString(emailOrd)
            });
        }
        return result;
    }

    public async Task<List<object>> GetEnCokSatisYapanMusterilerAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                m.MusteriID,
                m.AdSoyad,
                m.Ilce,
                m.MusteriTipi,
                IFNULL(SUM(sd.Adet), 0) as ToplamUrunSayisi,
                IFNULL(SUM(sd.BirimSatisFiyati * sd.Adet), 0) as ToplamCiro
            FROM musteriler m
            LEFT JOIN satislar s ON m.MusteriID = s.MusteriID
            LEFT JOIN satis_detaylar sd ON s.SatisID = sd.SatisID
            WHERE sd.Adet IS NOT NULL
            GROUP BY m.MusteriID, m.AdSoyad, m.Ilce, m.MusteriTipi
            HAVING ToplamUrunSayisi > 0
            ORDER BY ToplamUrunSayisi DESC
            LIMIT 3
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            int musteriTipiOrd = reader.GetOrdinal("MusteriTipi");
            int ilceOrd = reader.GetOrdinal("Ilce");
            int urunSayisiOrd = reader.GetOrdinal("ToplamUrunSayisi");
            
            result.Add(new
            {
                musteriID = reader.GetInt32("MusteriID"),
                adSoyad = reader.GetString("AdSoyad"),
                ilce = reader.IsDBNull(ilceOrd) ? null : reader.GetString(ilceOrd),
                musteriTipi = reader.IsDBNull(musteriTipiOrd) ? null : reader.GetString(musteriTipiOrd),
                toplamUrunSayisi = Convert.ToInt32(reader.GetDecimal(urunSayisiOrd)),
                toplamCiro = reader.GetDecimal("ToplamCiro")
            });
        }
        return result;
    }

    public async Task<List<object>> GetEnCokKarEdenMusterilerAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                m.MusteriID,
                m.AdSoyad,
                m.Ilce,
                m.MusteriTipi,
                IFNULL(SUM((sd.BirimSatisFiyati - sd.BirimMaliyet) * sd.Adet), 0) as ToplamKar,
                IFNULL(SUM(sd.BirimSatisFiyati * sd.Adet), 0) as ToplamCiro
            FROM musteriler m
            LEFT JOIN satislar s ON m.MusteriID = s.MusteriID
            LEFT JOIN satis_detaylar sd ON s.SatisID = sd.SatisID
            WHERE sd.BirimSatisFiyati IS NOT NULL AND sd.BirimMaliyet IS NOT NULL
            GROUP BY m.MusteriID, m.AdSoyad, m.Ilce, m.MusteriTipi
            HAVING ToplamKar > 0
            ORDER BY ToplamKar DESC
            LIMIT 3
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            int musteriTipiOrd = reader.GetOrdinal("MusteriTipi");
            int ilceOrd = reader.GetOrdinal("Ilce");
            
            result.Add(new
            {
                musteriID = reader.GetInt32("MusteriID"),
                adSoyad = reader.GetString("AdSoyad"),
                ilce = reader.IsDBNull(ilceOrd) ? null : reader.GetString(ilceOrd),
                musteriTipi = reader.IsDBNull(musteriTipiOrd) ? null : reader.GetString(musteriTipiOrd),
                toplamKar = reader.GetDecimal("ToplamKar"),
                toplamCiro = reader.GetDecimal("ToplamCiro")
            });
        }
        return result;
    }
}

