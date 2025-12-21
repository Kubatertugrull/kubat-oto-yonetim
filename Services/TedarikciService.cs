using MySqlConnector;
using Services;

namespace Services;

public class TedarikciService
{
    private readonly DatabaseService _dbService;

    public TedarikciService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<List<object>> GetTedarikcilerAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                t.TedarikciID,
                t.TedarikciAdi,
                t.Telefon,
                t.Eposta,
                COUNT(DISTINCT ts.SiparisID) as SiparisSayisi,
                IFNULL(SUM(tsd.Miktar), 0) as ToplamUrunSayisi
            FROM tedarikciler t
            LEFT JOIN tedarik_siparisleri ts ON t.TedarikciID = ts.TedarikciID
            LEFT JOIN tedarik_siparis_detaylari tsd ON ts.SiparisID = tsd.SiparisID
            GROUP BY t.TedarikciID, t.TedarikciAdi, t.Telefon, t.Eposta
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
            int epostaOrd = reader.GetOrdinal("Eposta");
            
            var urunSayisi = reader.IsDBNull(urunSayisiOrd) 
                ? 0 
                : Convert.ToInt32(reader.GetDecimal(urunSayisiOrd));
            toplamUrunSayisi += urunSayisi;

            tempList.Add(new
            {
                tedarikciID = reader.GetInt32("TedarikciID"),
                tedarikciAdi = reader.GetString("TedarikciAdi"),
                telefon = reader.IsDBNull(telefonOrd) ? null : reader.GetString(telefonOrd),
                eposta = reader.IsDBNull(epostaOrd) ? null : reader.GetString(epostaOrd),
                siparisSayisi = reader.GetInt32("SiparisSayisi"),
                toplamUrunSayisi = urunSayisi
            });
        }

        // Yüzde hesaplamalarını yap
        foreach (var item in tempList)
        {
            var yuzde = toplamUrunSayisi > 0 ? (double)item.toplamUrunSayisi / toplamUrunSayisi * 100 : 0;

            result.Add(new
            {
                tedarikciID = item.tedarikciID,
                tedarikciAdi = item.tedarikciAdi,
                telefon = item.telefon,
                eposta = item.eposta,
                siparisSayisi = item.siparisSayisi,
                toplamUrunSayisi = item.toplamUrunSayisi,
                yuzde = Math.Round(yuzde, 2)
            });
        }

        return result;
    }

    public async Task<List<object>> GetTedarikSiparisleriAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand(@"
            SELECT 
                ts.SiparisID,
                ts.SiparisTarihi,
                t.TedarikciAdi,
                IFNULL(SUM(tsd.Miktar * tsd.BirimMaliyet), 0) as ToplamTutar
            FROM tedarik_siparisleri ts
            JOIN tedarikciler t ON ts.TedarikciID = t.TedarikciID
            LEFT JOIN tedarik_siparis_detaylari tsd ON ts.SiparisID = tsd.SiparisID
            GROUP BY ts.SiparisID, ts.SiparisTarihi, t.TedarikciAdi
            ORDER BY ts.SiparisTarihi DESC
            LIMIT 5
        ", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                siparisID = reader.GetInt32("SiparisID"),
                siparisTarihi = reader.GetDateTime("SiparisTarihi"),
                tedarikciAdi = reader.GetString("TedarikciAdi"),
                toplamTutar = reader.IsDBNull(reader.GetOrdinal("ToplamTutar")) ? 0m : reader.GetDecimal("ToplamTutar")
            });
        }
        return result;
    }
}

