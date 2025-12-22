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

    public async Task<List<object>> GetIlceBazliPazarPenetrasyonAnaliziAsync()
    {
        try
        {
            await using var connection = await _dbService.GetConnectionAsync();
            
            // Önce tablo var mı kontrol et
            var checkTableCmd = new MySqlCommand(@"
                SELECT COUNT(*) as TableExists
                FROM information_schema.tables 
                WHERE table_schema = DATABASE() 
                AND table_name = 'ilce_pazar_hedefleri'
            ", connection);
            
            var tableExists = false;
            await using (var checkReader = await checkTableCmd.ExecuteReaderAsync())
            {
                if (await checkReader.ReadAsync())
                {
                    tableExists = checkReader.GetInt32("TableExists") > 0;
                }
            }

            MySqlCommand cmd;
            
            if (tableExists)
            {
                // Tablo varsa, orijinal sorguyu kullan
                cmd = new MySqlCommand(@"
                    SELECT 
                        h.Ilce, 
                        h.ToplamPotansiyel, 
                        COUNT(m.MusteriID) as Mevcut
                    FROM ilce_pazar_hedefleri h
                    LEFT JOIN musteriler m ON h.Ilce = m.Ilce
                    GROUP BY h.Ilce, h.ToplamPotansiyel
                    ORDER BY (COUNT(m.MusteriID) / h.ToplamPotansiyel) ASC
                ", connection);
            }
            else
            {
                // Tablo yoksa, sadece mevcut müşteri sayılarını göster (varsayılan potansiyel: 100)
                cmd = new MySqlCommand(@"
                    SELECT 
                        m.Ilce, 
                        100 as ToplamPotansiyel, 
                        COUNT(m.MusteriID) as Mevcut
                    FROM musteriler m
                    WHERE m.Ilce IS NOT NULL AND m.Ilce != ''
                    GROUP BY m.Ilce
                    ORDER BY (COUNT(m.MusteriID) / 100) ASC
                ", connection);
            }

            var result = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var ilce = reader.GetString("Ilce");
                var toplamPotansiyel = reader.GetInt32("ToplamPotansiyel");
                var mevcut = reader.GetInt32("Mevcut");
                
                // Pazar payı yüzdesini hesapla
                var pazarPayiYuzdesi = toplamPotansiyel > 0 
                    ? Math.Round((double)mevcut / toplamPotansiyel * 100, 1) 
                    : 0.0;

                // Renk kategorisini belirle
                string renkKategori;
                if (pazarPayiYuzdesi >= 0 && pazarPayiYuzdesi <= 4)
                {
                    renkKategori = "red"; // Kritik/Fırsat
                }
                else if (pazarPayiYuzdesi >= 5 && pazarPayiYuzdesi <= 9)
                {
                    renkKategori = "yellow"; // Orta
                }
                else
                {
                    renkKategori = "green"; // Doygun (10+)
                }

                result.Add(new
                {
                    ilce = ilce,
                    toplamPotansiyel = toplamPotansiyel,
                    mevcut = mevcut,
                    pazarPayiYuzdesi = pazarPayiYuzdesi,
                    renkKategori = renkKategori
                });
            }
            return result;
        }
        catch (Exception ex)
        {
            // Hata durumunda boş liste döndür
            Console.WriteLine($"İlçe bazlı pazar penetrasyon analizi hatası: {ex.Message}");
            return new List<object>();
        }
    }
}

