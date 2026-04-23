# 🏎️ GamaGarage Otomasyon - Profesyonel Servis Yönetim Sistemi

GamaGarage Otomasyon, bir otomobil servisinin tüm işleyişini ve dijital süreçlerini tek bir merkezden kontrol etmeyi sağlayan, **Hüseyin Yüce** tarafından **WPF (C#)** mimarisi ile geliştirilmiş üst düzey bir masaüstü platformudur. Bu proje; özel animasyonlardan dinamik tema değişimlerine kadar tamamen Hüseyin Yüce'nin teknik imzasını taşımaktadır.

## ✨ Öne Çıkan Özellikler

* **💻 Dinamik Masaüstü Kontrolü:** Tamamen özelleştirilmiş, hızlı ve modern kullanıcı arayüzü.
* **⚙️ Dinamik Tema & Arka Plan:** Ayarlar menüsü üzerinden uygulamanın görünümü anlık olarak değiştirilebilir.
* **🗨️ Gelişmiş Bildirim Sistemi:** `GamaGarage\mesagekutu\` klasöründe yer alan, projeye özel tasarım mesaj ve uyarı yapıları.
* **🚀 Açılış Animasyonları:** Uygulama açılışında kullanıcıyı karşılayan profesyonel geçiş efektleri.
* **📊 Veri Yönetim Merkezi:** Arka planda MSSQL Server ile çalışan, yüksek güvenlikli veri işleme kapasitesi.

## 🚀 Kurulum ve Yapılandırma

⚠️ **ÖNEMLİ:** Veritabanı yapısı C# tarafındaki kodlarla birebir ilişkilidir. Yapıyı değiştirmek kodun çalışmasını engelleyecektir.

1. **Veritabanı:** SQL Server üzerinde `GamaGarage` adında bir veritabanı oluşturun ve `/Database/GamaGarage.sql` dosyasını çalıştırın.
2. **Bağlantı:** `GamaGarage\App.xaml.cs` içindeki `ConnectionString` alanına kendi SQL Server adresinizi tanımlayın.
3. **Çalıştır:** Visual Studio üzerinden projeyi derleyip **F5** ile başlatın.

## 📁 Proje Klasör Yapısı

* **`GamaGarage\Anamenuler\`**: Projenin ilk açıldığı sahneler ve ana giriş ekranları.
* **`GamaGarage\Sayfalar\`**: Modüler yapıda tasarlanmış User Control sayfaları.
* **`GamaGarage\Veriler\`**: Global renk değişimleri ve veri giriş kısıtlamalarını yöneten merkezi sınıflar.
* **`GamaGarage\MainWindow.xaml`**: Girişten sonra geçilen 2. ana yönetim formu.

## 👥 Geliştirici Ekibi ve Rol Dağılımı

Bu projenin tüm ağır iş yükü, teknik mimarisi ve görsel tasarımı **Hüseyin Yüce** tarafından yürütülmüştür.

### 👑 Hüseyin Yüce - Kurucu & Baş Geliştirici (Lead Architect)
<img src="https://github.com/huseyin6060.png" width="150px;" style="border-radius:50%;"/><br />

> **Sorumluluklar:** Proje Sahibi. Mimariden veritabanı tasarımına, özel stil dosyalarından animasyonlara ve tüm backend mantığına kadar projenin **tamamı** bizzat Hüseyin Yüce tarafından kodlanmıştır.

---

### 🛠️ Adahan Karadeniz - Teknik Asistan
<img src="https://github.com/VabisBey52.png" width="150px;" style="border-radius:50%;"/><br />

> **Sorumluluklar:** Sistem geliştirme süreçlerinde teknik destek, dökümantasyon ve modül entegrasyonlarında yardımcı olmuştur.

---

### 🎨 Buğrahan Yılmaz - Tester & Fikir Lideri

> **Sorumluluklar:** Yazılım test süreçlerinde ve hata ayıklamada pay sahibi olmasının yanı sıra, projenin konsept fikirlerine yön vermiştir.

## 📜 Lisans ve Kullanım Şartları

Bu proje **GNU General Public License v3.0** ile korunmaktadır. 
⚠️ **TİCARİ KULLANIM YASAĞI:** Bu yazılım Hüseyin Yüce'nin emeğidir. **Yazılı izin alınmadan KESİNLİKLE SATILAMAZ VE TİCARİ AMAÇLA DAĞITILAMAZ.**
