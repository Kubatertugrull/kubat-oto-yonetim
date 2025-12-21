using MySqlConnector;
using Services;

namespace Services;

public class IadeService
{
    private readonly DatabaseService _dbService;

    public IadeService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<object> GetIadeIstatistikleriAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        
        // Önce tüm iade nedenlerini kontrol et (debug için)
        var debugCmd = new MySqlCommand(@"
            SELECT DISTINCT IadeNedeni, COUNT(*) as Sayi
            FROM iadeler
            WHERE YEAR(IadeTarihi) = YEAR(CURDATE())
            GROUP BY IadeNedeni
        ", connection);
        
        // Ana sorgu - IadeNedeni'ni LIKE ile kontrol et (büyük/küçük harf duyarsız)
        var cmd = new MySqlCommand(@"
            SELECT 
                COUNT(*) as ToplamIade,
                SUM(CASE WHEN LOWER(IadeNedeni) LIKE '%bozuk%' THEN 1 ELSE 0 END) as BozukUrunSayisi,
                SUM(CASE WHEN LOWER(IadeNedeni) LIKE '%hatalı%' OR LOWER(IadeNedeni) LIKE '%hatali%' THEN 1 ELSE 0 END) as HataliUrunSayisi,
                SUM(CASE WHEN LOWER(IadeNedeni) LIKE '%yanlış%' OR LOWER(IadeNedeni) LIKE '%yanlis%' OR LOWER(IadeNedeni) LIKE '%yanlıs%' THEN 1 ELSE 0 END) as YanlisUrunSayisi
            FROM iadeler
            WHERE YEAR(IadeTarihi) = YEAR(CURDATE())
        ", connection);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new
            {
                toplamIade = reader.GetInt32("ToplamIade"),
                bozukUrunSayisi = reader.GetInt32("BozukUrunSayisi"),
                hataliUrunSayisi = reader.GetInt32("HataliUrunSayisi"),
                yanlisUrunSayisi = reader.GetInt32("YanlisUrunSayisi")
            };
        }
        return new { toplamIade = 0, bozukUrunSayisi = 0, hataliUrunSayisi = 0, yanlisUrunSayisi = 0 };
    }

    public async Task<List<object>> GetBozukUrunTedarikciDagilimiAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        
        // Sorun: JOIN'ler nedeniyle aynı iade kaydı birden fazla kez sayılıyor.
        // Çözüm: Her iade kaydı için sadece bir ürün ve bir tedarikçi eşleştirmesi yapmalıyız.
        // Bir satışta birden fazla ürün olabilir, ama her iade kaydı tek bir IadeID.
        // Her ürün için en son sipariş verilen tedarikçiyi kullanacağız.
        var cmd = new MySqlCommand(@"
            SELECT 
                t.TedarikciID,
                t.TedarikciAdi,
                t.Telefon,
                t.Eposta,
                COUNT(DISTINCT iade_tedarikci.IadeID) as BozukUrunSayisi
            FROM (
                -- Her iade için bir ürün ve tedarikçi eşleştirmesi
                SELECT DISTINCT
                    i.IadeID,
                    i.SatisID,
                    (
                        SELECT ts.TedarikciID
                        FROM satis_detaylar sd2
                        JOIN urunler u2 ON sd2.UrunID = u2.UrunID
                        JOIN tedarik_siparis_detaylari tsd2 ON u2.UrunID = tsd2.UrunID
                        JOIN tedarik_siparisleri ts ON tsd2.SiparisID = ts.SiparisID
                        WHERE sd2.SatisID = i.SatisID
                        ORDER BY ts.SiparisTarihi DESC
                        LIMIT 1
                    ) as TedarikciID
                FROM iadeler i
                WHERE (i.IadeNedeni LIKE '%Bozuk%' OR i.IadeNedeni LIKE '%bozuk%' OR i.IadeNedeni = 'Bozuk')
                  AND YEAR(i.IadeTarihi) = YEAR(CURDATE())
            ) iade_tedarikci
            JOIN tedarikciler t ON iade_tedarikci.TedarikciID = t.TedarikciID
            WHERE iade_tedarikci.TedarikciID IS NOT NULL
            GROUP BY t.TedarikciID, t.TedarikciAdi, t.Telefon, t.Eposta
            ORDER BY BozukUrunSayisi DESC
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            int telefonOrd = reader.GetOrdinal("Telefon");
            int epostaOrd = reader.GetOrdinal("Eposta");
            
            result.Add(new
            {
                tedarikciID = reader.GetInt32("TedarikciID"),
                tedarikciAdi = reader.GetString("TedarikciAdi"),
                telefon = reader.IsDBNull(telefonOrd) ? null : reader.GetString(telefonOrd),
                eposta = reader.IsDBNull(epostaOrd) ? null : reader.GetString(epostaOrd),
                bozukUrunSayisi = reader.GetInt32("BozukUrunSayisi")
            });
        }
        return result;
    }

    public async Task<List<object>> GetIadelerAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        
        // iadeler.UrunID NULL olduğu için, iadeler.SatisID üzerinden satis_detaylar'a gidip UrunID'yi çekmeliyiz
        // Bir satışta birden fazla ürün olabilir, bu yüzden GROUP_CONCAT kullanarak tüm ürünleri birleştirebiliriz
        // veya ilk ürünü alabiliriz. Burada ilk ürünü alıyoruz.
        var cmd = new MySqlCommand(@"
            SELECT 
                i.IadeID,
                i.IadeTarihi,
                i.IadeNedeni,
                s.FaturaNo,
                COALESCE(
                    (SELECT u.UrunAdi 
                     FROM satis_detaylar sd 
                     JOIN urunler u ON sd.UrunID = u.UrunID 
                     WHERE sd.SatisID = i.SatisID 
                     LIMIT 1),
                    'Bilinmeyen Ürün'
                ) as UrunAdi
            FROM iadeler i
            JOIN satislar s ON i.SatisID = s.SatisID
            ORDER BY i.IadeTarihi DESC
            LIMIT 5
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                iadeID = reader.GetInt32("IadeID"),
                iadeTarihi = reader.GetDateTime("IadeTarihi"),
                iadeNedeni = reader.GetString("IadeNedeni"),
                faturaNo = reader.GetString("FaturaNo"),
                urunAdi = reader.GetString("UrunAdi")
            });
        }
        return result;
    }
}

