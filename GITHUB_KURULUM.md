# GitHub'a Yükleme Adımları

## 1. GitHub'da Yeni Repository Oluşturun

1. GitHub'a giriş yapın: https://github.com
2. Sağ üstteki "+" işaretine tıklayın
3. "New repository" seçeneğini seçin
4. Repository adını girin (örn: `kubat-oto-yonetim`)
5. "Public" veya "Private" seçin
6. **README, .gitignore veya license eklemeyin** (zaten var)
7. "Create repository" butonuna tıklayın

## 2. Projeyi GitHub'a Bağlayın

Aşağıdaki komutları terminalde çalıştırın (GitHub'dan aldığınız URL'i kullanın):

```bash
# Eğer remote origin zaten varsa, önce kaldırın:
git remote remove origin

# GitHub repository URL'inizi buraya ekleyin
git remote add origin https://github.com/KULLANICI-ADI/REPOSITORY-ADI.git

# Değişiklikleri GitHub'a gönderin
git push -u origin main
```

**Not:** Eğer "remote origin already exists" hatası alırsanız, önce `git remote remove origin` komutunu çalıştırın.

## 3. Alternatif: SSH Kullanıyorsanız

Eğer SSH kullanıyorsanız:

```bash
git remote add origin git@github.com:KULLANICI-ADI/REPOSITORY-ADI.git
git push -u origin main
```

## 4. Sonraki Yükleme İşlemleri

İleride değişiklik yaptığınızda:

```bash
# Değişiklikleri ekle
git add .

# Commit yap
git commit -m "Açıklama mesajı"

# GitHub'a gönder
git push
```

## Notlar

- `.gitignore` dosyası oluşturuldu, build dosyaları ve node_modules yüklenecek
- `README.md` dosyası oluşturuldu, projeniz hakkında bilgiler içeriyor
- Veritabanı şifrelerini ve hassas bilgileri `appsettings.json`'da kontrol edin

