# Stok Güncellemesi Notları

## ÖNEMLİ: SQL Trigger Kullanımı

Veritabanında SQL Trigger'lar eklendi. Artık stok güncellemeleri otomatik olarak yapılıyor:

- **Satış yapıldığında**: Trigger otomatik olarak stok düşüşünü yapıyor
- **İade alındığında**: Trigger otomatik olarak stok artışını yapıyor  
- **Tedarik siparişi oluşturulduğunda**: Trigger otomatik olarak stok girişini yapıyor

## DİKKAT: Kod Tarafında Stok Güncellemesi YAPMAYIN!

Eğer gelecekte aşağıdaki endpoint'ler eklenirse, **KESINLIKLE** manuel stok güncellemesi yapmayın:

### ❌ YAPILMAMASI GEREKENLER:

1. **Satış oluşturma endpoint'i** (örnek: `POST /api/satis`)
   - ❌ `UPDATE urunler SET StokMiktari = StokMiktari - @adet WHERE UrunID = @urunID` gibi sorgular YAZMAYIN
   - ✅ Sadece `INSERT INTO satislar` ve `INSERT INTO satis_detaylar` yapın
   - ✅ Trigger otomatik olarak stok düşüşünü yapacak

2. **İade oluşturma endpoint'i** (örnek: `POST /api/iade`)
   - ❌ `UPDATE urunler SET StokMiktari = StokMiktari + @adet WHERE UrunID = @urunID` gibi sorgular YAZMAYIN
   - ✅ Sadece `INSERT INTO iadeler` yapın
   - ✅ Trigger otomatik olarak stok artışını yapacak

3. **Tedarik siparişi oluşturma endpoint'i** (örnek: `POST /api/tedarikci/siparis`)
   - ❌ `UPDATE urunler SET StokMiktari = StokMiktari + @miktar WHERE UrunID = @urunID` gibi sorgular YAZMAYIN
   - ✅ Sadece `INSERT INTO tedarik_siparisleri` ve `INSERT INTO tedarik_siparis_detaylari` yapın
   - ✅ Trigger otomatik olarak stok artışını yapacak

### ✅ Doğru Yaklaşım:

```csharp
// Sadece satış kaydı ekle, stok güncellemesi trigger tarafından yapılacak
public async Task<bool> CreateSatisAsync(SatisModel model)
{
    await using var connection = await _dbService.GetConnectionAsync();
    
    // Sadece INSERT işlemleri yap
    var satisCmd = new MySqlCommand(@"
        INSERT INTO satislar (MusteriID, SatisTarihi, FaturaNo, ToplamTutar)
        VALUES (@musteriID, @tarih, @faturaNo, @tutar)
    ", connection);
    // ... parametreler
    
    // Stok güncellemesi YAPMAYIN - Trigger otomatik yapacak!
    // ❌ UPDATE urunler SET StokMiktari = ... YAZMAYIN!
    
    return await satisCmd.ExecuteNonQueryAsync() > 0;
}
```

## Mevcut Durum

Şu anda kodda manuel stok güncellemesi yapan bir kod bulunmamaktadır. Tüm servisler sadece SELECT sorguları yapmaktadır.

