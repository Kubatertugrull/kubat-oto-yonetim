# KUBAT OTO - Yedek Parça Yönetim Sistemi

İzmir Bölge Dashboard - Otomotiv Yedek Parça Yönetim ve Analiz Sistemi

## 🚀 Proje Özellikleri

Bu proje, otomotiv yedek parça firmaları için geliştirilmiş kapsamlı bir yönetim ve analiz sistemidir.

### 📊 Özellikler

- **Genel Bakış (Dashboard)**: Yıllık ciro, net kâr, iade oranı ve kritik stok takibi
- **Satışlar**: Satış geçmişi, en çok satılan ürünler ve en çok kâr edilen ürünler
- **Tedarikçiler**: Tedarikçi performans analizi ve sipariş geçmişi
- **Stok Durumu**: Ürün stok takibi ve kritik seviye uyarıları
- **Rakip Analizi**: Rakiplerin fiyat karşılaştırması ve fiyat güncelleme
- **İadeler**: İade analizi ve tedarikçi bazlı dağılım
- **Müşteriler**: Müşteri analizi ve İzmir ilçe bazlı harita görünümü

## 🛠️ Teknolojiler

- **Backend**: ASP.NET Core 8.0 (C#)
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **Veritabanı**: MySQL
- **Kütüphaneler**: 
  - Chart.js (Grafikler)
  - Leaflet.js (Harita görselleştirme)
  - Bootstrap 5
  - Font Awesome

## 📦 Kurulum

### Gereksinimler

- .NET 8.0 SDK
- MySQL Server
- Git

### Adımlar

1. Projeyi klonlayın:
```bash
git clone https://github.com/kullanici-adi/proje-adi.git
cd proje-adi
```

2. Veritabanı bağlantı bilgilerini `appsettings.json` dosyasında güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=veritabani_adi;User=kullanici;Password=sifre;"
  }
}
```

3. Bağımlılıkları yükleyin:
```bash
dotnet restore
```

4. Projeyi çalıştırın:
```bash
dotnet run
```

5. Tarayıcınızda açın: `http://localhost:5000`

## 📁 Proje Yapısı

```
projhe/
├── Controllers/          # API Controller'ları
├── Services/            # İş mantığı servisleri
├── Models/              # Veri modelleri
├── Middlewares/         # Middleware'ler
├── Utils/               # Yardımcı fonksiyonlar
├── wwwroot/             # Frontend dosyaları
│   ├── css/            # Stil dosyaları
│   ├── images/         # Resim dosyaları
│   └── *.html          # HTML sayfaları
├── Program.cs           # Uygulama giriş noktası
└── projhe.csproj       # Proje dosyası
```

## 🔧 Yapılandırma

### Veritabanı

Veritabanı şeması için SQL migration dosyaları veya şema dokümantasyonu eklenmelidir.

### API Endpoints

- `/api/dashboard/*` - Dashboard verileri
- `/api/satis/*` - Satış verileri
- `/api/tedarikci/*` - Tedarikçi verileri
- `/api/stok/*` - Stok verileri
- `/api/rakip/*` - Rakip analizi
- `/api/iade/*` - İade verileri
- `/api/musteri/*` - Müşteri verileri

## 📝 Notlar

- Veritabanında SQL Trigger'lar kullanılmaktadır (stok güncellemeleri için)
- Manuel stok güncellemesi yapılmamalıdır - trigger'lar otomatik hallediyor

## 👤 Geliştirici

KUBAT OTO - Yedek Parça Yönetim Sistemi

## 📄 Lisans

Bu proje özel bir projedir.

