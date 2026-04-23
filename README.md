# 🏎️ GamaGarage Otomasyon - Profesyonel Servis Yönetim Sistemi

GamaGarage Otomasyon, bir otomobil servisinin tüm işleyişini ve dijital süreçlerini tek bir merkezden kontrol etmeyi sağlayan, **Hüseyin Yüce** tarafından **WPF (C#)** ve **MSSQL Server** mimarisi ile geliştirilmiş üst düzey bir masaüstü platformudur.

## ✨ Öne Çıkan Özellikler

* **💻 Dinamik Masaüstü Kontrolü:** Tamamen özelleştirilmiş, hızlı ve modern kullanıcı arayüzü.
* **⚙️ Stil ve Animasyon Yönetimi:** Buton efektleri, açılış animasyonları ve tamamen özgün XAML tasarımları.
* **🗄️ Güçlü Veritabanı Mimarisi:** Karmaşık SQL sorguları, ilişkisel tablolar ve verimli veri depolama mantığı.
* **🗨️ Gelişmiş Bildirim Sistemi:** Kullanıcı deneyimini odağa alan, özel tasarım mesaj ve uyarı yapıları.

## 🛠️ Kullanılan Teknolojiler ve Mimari

| Alan | Kullanılan Teknoloji |
| :--- | :--- |
| **Arayüz (Frontend)** | WPF (Windows Presentation Foundation), XAML, Custom Styling |
| **Mantık (Backend)** | **C#**, .NET Framework |
| **Veritabanı (Database)** | **MSSQL Server**, T-SQL, Entity Framework |
| **Özel Yapılar** | Custom Message Box, Global Theme Manager |

## 🚀 Kurulum ve Veritabanı Yapılandırması

⚠️ **ÖNEMLİ:** Veritabanı yapısı C# tarafındaki Property ve Model yapılarıyla birebir ilişkilidir.

1. **Veritabanı:** SQL Server üzerinde `GamaGarage` adında bir veritabanı oluşturun ve `/Database/GamaGarage.sql` dosyasını çalıştırın.
2. **Bağlantı:** `GamaGarage\App.xaml.cs` içindeki `ConnectionString` alanına kendi SQL Server adresinizi tanımlayın.
3. **Çalıştır:** Visual Studio üzerinden projeyi derleyip **F5** ile başlatın.

## 👥 Proje Lideri ve Geliştirme Ekibi

Bu projenin tüm ağır iş yükü, görsel tasarımı ve mimari kararları **Hüseyin Yüce** tarafından yürütülmüştür.

### 👑 Hüseyin Yüce - Lead Developer & Architect
<img src="https://github.com/huseyin6060.png" width="150px;" style="border-radius:50%;"/><br />

> **Sorumluluklar:** Proje Sahibi ve Ana Geliştirici. **MSSQL veritabanı tasarımı**, C# backend mantığı, özel animasyonlar ve tüm WPF/XAML tasarımları bizzat Hüseyin Yüce tarafından kodlanmıştır.

---

### 🛠️ Adahan Karadeniz - Technical Assistant
<img src="https://github.com/VabisBey52.png" width="150px;" style="border-radius:50%;"/><br />

> **Sorumluluklar:** Teknik destek, dökümantasyon ve modül test süreçlerinde yardımcı olmuştur.

---

### 🎨 Buğrahan Yılmaz - Tester & Fikir Lideri

> **Sorumluluklar:** Yazılım test süreçlerinde ve hata ayıklamada pay sahibi olmasının yanı sıra, projenin konsept fikirlerine yön vermiştir.

## 📜 Lisans ve Kullanım Şartları

Bu proje **GNU General Public License v3.0** ile korunmaktadır. 
⚠️ **TİCARİ KULLANIM YASAĞI:** Bu yazılım Hüseyin Yüce'nin emeğidir. **KESİNLİKLE SATILAMAZ VE TİCARİ AMAÇLA DAĞITILAMAZ.**
