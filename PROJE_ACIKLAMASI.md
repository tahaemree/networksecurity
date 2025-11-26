# PKI Güvenli İletişim Sistemi Projesi

Bu proje, C# kullanılarak geliştirilmiş küçük ölçekli bir **PKI (Public Key Infrastructure - Açık Anahtar Altyapısı)** simülasyonudur. Üç farklı konsol uygulaması (CA, Client1, Client2) ve bir ortak kütüphane (Shared) üzerinden güvenli bir iletişim protokolünü uygular.

## 📂 Proje Klasör Yapısı

Proje, Visual Studio Solution (`.sln`) yapısı altında organize edilmiştir:

```
PKI_System/
├── ag_guvenligi_bitirme.sln  # Ana çözüm dosyası
│
├── CA/                       # Certificate Authority (Sertifika Otoritesi) Uygulaması
│   ├── CA.csproj
│   └── Program.cs            # CA'nın ana kodları (Anahtar üretimi, dinleme, imzalama)
│
├── Client1/                  # İstemci 1 Uygulaması
│   ├── Client1.csproj
│   └── Program.cs            # Client 1'in protokol akışı (Sertifika alma, Client 2 ile görüşme)
│
├── Client2/                  # İstemci 2 Uygulaması
│   ├── Client2.csproj
│   └── Program.cs            # Client 2'nin protokol akışı (Client 1'e bağlanma)
│
└── Shared/                   # Ortak Kütüphane (Tüm projelerin kullandığı sınıflar)
    ├── Shared.csproj
    ├── Certificate.cs        # Sertifika veri yapısı
    └── CryptoHelper.cs       # Şifreleme, imzalama ve doğrulama yardımcı fonksiyonları
```

---

## 🛠 Bileşenler ve Görevleri

### 1. Shared (Ortak Kütüphane)
Tüm uygulamaların ortak kullandığı veri tiplerini ve kriptografik fonksiyonları içerir.
*   **`Certificate.cs`**: Dijital sertifikanın yapısını tanımlar (SubjectID, Public Key, İmza vb.).
*   **`CryptoHelper.cs`**:
    *   RSA Anahtar Çifti Üretimi (`GenerateRSAKeys`)
    *   Dijital İmzalama ve Doğrulama (`SignData`, `VerifyData`)
    *   RSA ile Şifreleme/Çözme (Anahtar değişimi için)
    *   AES ile Şifreleme/Çözme (Mesajlaşma için)
    *   SHA256 ile Hashleme (Anahtar türetme için)

### 2. CA (Certificate Authority)
Güven otoritesidir.
*   Kendi RSA anahtarlarını üretir.
*   5000 portunu dinler.
*   Client'lardan gelen sertifika isteklerini (CSR) alır.
*   İsteği kendi **Private Key**'i ile imzalar ve sertifikayı Client'a geri gönderir.
*   Client'ların birbirini doğrulayabilmesi için kendi **Public Key**'ini dağıtır.

### 3. Client 1 ve Client 2
Birbiriyle güvenli iletişim kurmak isteyen iki taraftır. 3 aşamalı bir protokol izlerler:

#### **Aşama 1: Sertifika Alma (Güven İnşası)**
1.  Kendi RSA anahtarlarını üretirler.
2.  CA'ya bağlanıp Public Key'lerini gönderirler.
3.  CA'dan imzalı sertifikalarını alırlar.

#### **Aşama 2: Master Key ($K_m$) Oluşturma**
1.  Client 1 sunucu moduna geçer (Port 6001), Client 2 ona bağlanır.
2.  Sertifikalarını birbirlerine gönderirler.
3.  CA'nın Public Key'i ile karşı tarafın sertifikasını doğrularlar.
4.  **Kimlik Doğrulama ve Anahtar Değişimi:**
    *   Client 1, `$ID_{C1} | N_1$` (Kimlik + Rastgele Sayı) verisini Client 2'nin Public Key'i ile şifreleyip gönderir.
    *   Client 2, `$ID_{C2} | N_2$` verisini Client 1'in Public Key'i ile şifreleyip gönderir.
5.  İki taraf da $N_1$ ve $N_2$ sayılarını elde eder.
6.  Bu sayılar birleştirilip hashlenerek **Master Key ($K_m$)** üretilir.

#### **Aşama 3: Session Key ($K_s$) Oluşturma**
1.  Master Key doğrudan kullanılmaz.
2.  Client 1, yeni bir rastgele sayı ($N_3$) üretir ve **Master Key ile şifreleyip** gönderir.
3.  Client 2, yeni bir rastgele sayı ($N_4$) üretir ve **Master Key ile şifreleyip** gönderir.
4.  Bu sayılar ($N_3 + N_4$) hashlenerek geçici **Session Key ($K_s$)** üretilir.
5.  Son olarak, asıl mesajlaşma bu Session Key kullanılarak AES algoritması ile yapılır.

---

## 🚀 Nasıl Çalıştırılır?

Projeyi çalıştırmak için 3 ayrı terminal açın ve sırasıyla şu komutları girin:

**Terminal 1 (CA):**
```bash
dotnet run --project CA/CA.csproj
```

**Terminal 2 (Client 1):**
```bash
dotnet run --project Client1/Client1.csproj
```

**Terminal 3 (Client 2):**
```bash
dotnet run --project Client2/Client2.csproj
```
