using GamaGarage.mesagekutu; 
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace GamaGarage.Sayfalar
{
    public partial class Firmalar : UserControl
    {
        int seciliFirmaID = 0;
        int seciliOdemeID = 0;

        public Firmalar()
        {
            InitializeComponent();
            FirmaListele();
        }

        private void FirmaListele()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Firmalar ORDER BY FirmaID DESC", baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgFirmaListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Firma listesi yüklenemedi: " + ex.Message, "LİSTE HATASI"); }
        }

        private void FirmaOdemeListele(int fId)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM FirmaOdemeleri WHERE FirmaID = @fId ORDER BY IslemTarihi DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@fId", fId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgFirmaAltTablo.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ödeme listesi yüklenemedi: " + ex.Message, "LİSTE HATASI"); }
        }

        private void txtFirmaAra1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string ara = txtFirmaAra1.Text.Trim();
                    string sorgu = "SELECT * FROM Firmalar WHERE FirmaAd LIKE @ara OR FirmaYetkilisi LIKE @ara";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@ara", "%" + ara + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgFirmaListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void dgFirmaListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgFirmaListesi.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliFirmaID = Convert.ToInt32(satir["FirmaID"]);
                    txtFAd.Text = satir["FirmaAd"].ToString();
                    txtFYetkilisiAd.Text = satir["FirmaYetkilisi"].ToString(); 
                    txtFOAD.Text = satir["FirmaAd"].ToString();
                    txtFOYetkilisi.Text = satir["FirmaYetkilisi"].ToString();
                    Telefon.Text = satir["Telefon"].ToString();
                    txtFAdres.Text = satir["Adres"].ToString();
                    txtFGmail.Text = satir["Gmail"].ToString();
                    txtFAciklama.Text = satir["Aciklama"].ToString();

                    if (satir["FirmaKayitTarihi"] != DBNull.Value)
                        dpFirmaTarihi.SelectedDate = Convert.ToDateTime(satir["FirmaKayitTarihi"]);

                    FirmaOdemeListele(seciliFirmaID);
                }
                catch { }
            }
        }

        private void btnFirmaEkle_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFAd.Text)) { GamaMesaj.Tamam("Lütfen firma adını girin!", "EKSİK BİLGİ"); return; }
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"INSERT INTO Firmalar (FirmaAd, FirmaYetkilisi, Telefon, Adres, Gmail, FirmaKayitTarihi, Aciklama) 
                                    VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7)";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", txtFAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtFYetkilisiAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", Telefon.Text.Trim());
                    komut.Parameters.AddWithValue("@p4", txtFAdres.Text.Trim());
                    komut.Parameters.AddWithValue("@p5", txtFGmail.Text.Trim());
                    komut.Parameters.AddWithValue("@p6", dpFirmaTarihi.SelectedDate ?? DateTime.Now);
                    komut.Parameters.AddWithValue("@p7", txtFAciklama.Text.Trim());

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Firma başarıyla kaydedildi.", "BAŞARILI");
                    FirmaListele();
                    Temizle();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ekleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnFirmaGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (seciliFirmaID == 0) { GamaMesaj.Tamam("Lütfen listeden güncellenecek firmayı seçin!", "SEÇİM YAP"); return; }
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"UPDATE Firmalar SET FirmaAd=@p1, FirmaYetkilisi=@p2, Telefon=@p3, Adres=@p4, 
                                    Gmail=@p5, FirmaKayitTarihi=@p6, Aciklama=@p7 WHERE FirmaID=@id";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", txtFAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtFYetkilisiAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", Telefon.Text.Trim());
                    komut.Parameters.AddWithValue("@p4", txtFAdres.Text.Trim());
                    komut.Parameters.AddWithValue("@p5", txtFGmail.Text.Trim());
                    komut.Parameters.AddWithValue("@p6", dpFirmaTarihi.SelectedDate ?? DateTime.Now);
                    komut.Parameters.AddWithValue("@p7", txtFAciklama.Text.Trim());
                    komut.Parameters.AddWithValue("@id", seciliFirmaID);

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Firma bilgileri güncellendi.", "GÜNCELLENDİ");
                    FirmaListele();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Güncelleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnFirmaSil_Click(object sender, RoutedEventArgs e)
        {
            if (seciliFirmaID == 0) { GamaMesaj.Tamam("Silinecek firmayı seçin!", "SEÇİM EKSİK"); return; }

            if (GamaMesaj.Onay("Seçili firmayı silmek istediğinize emin misiniz?", "ONAY"))
            {
                try
                {
                    using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                    {
                        baglanti.Open();
                        SqlCommand komut = new SqlCommand("DELETE FROM Firmalar WHERE FirmaID=@id", baglanti);
                        komut.Parameters.AddWithValue("@id", seciliFirmaID);
                        komut.ExecuteNonQuery();
                        GamaMesaj.Tamam("Firma başarıyla silindi.", "BİLGİ");
                        FirmaListele();
                        Temizle();
                    }
                }
                catch (Exception ex) { GamaMesaj.Tamam("Silme Hatası: " + ex.Message, "HATA"); }
            }
        }

        private void btnTemizle_Click(object sender, RoutedEventArgs e)
        {
            Temizle();
        }

        private void Temizle()
        {
            seciliFirmaID = 0;
            seciliOdemeID = 0;
            txtFAd.Clear(); txtFYetkilisiAd.Clear(); Telefon.Clear();
            txtFAdres.Clear(); txtFGmail.Clear(); txtFAciklama.Clear();
            txtFBorc.Clear(); txtFÖdenen.Clear(); txtFOdemeAciklama.Clear();
            dpFirmaTarihi.SelectedDate = null; dpFOdemeTarih.SelectedDate = null;
            dgFirmaListesi.SelectedItem = null; dgFirmaAltTablo.ItemsSource = null;
        }

        private void btnFOdemeTamamla_Click(object sender, RoutedEventArgs e)
        {
           
            if (seciliFirmaID == 0) { GamaMesaj.Tamam("Lütfen önce listeden bir firma seçin!", "UYARI"); return; }
            if (string.IsNullOrWhiteSpace(txtFÖdenen.Text)) { GamaMesaj.Tamam("Lütfen ödenecek tutarı girin.", "EKSİK BİLGİ"); return; }

           
            string girilenSeriNo = txtFSerino?.Text?.Trim() ?? "";
            string firmaAdi = txtFAd.Text.Trim(); 
            string firmaYetkilisi = txtFYetkilisiAd.Text.Trim(); 

            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                try
                {
                    baglanti.Open();
                    SqlTransaction islem = baglanti.BeginTransaction();

                    try
                    {
                        
                        string evrakTuruMetin = "Nakit";
                        if (cmbFTuru.SelectedItem is ComboBoxItem item)
                        {
                            evrakTuruMetin = item.Content.ToString()!;
                        }

                        int yeniEvrakID = 0;

                        
                        if (evrakTuruMetin == "Çek" || evrakTuruMetin == "Senet")
                        {
                            
                            string csSorgu = @"INSERT INTO CekSenetIslemleri 
                                       (EvrakTuru, SeriNo, VadeTarihi, VerisTarihi, Tutar, MuhatapCariAd, Firma, Aciklama, IslemYonu) 
                                       OUTPUT INSERTED.EvrakID 
                                       VALUES (@tur, @seri, @vade, @veris, @tutar, @cari, @firmaID, @acik, 'Çıkış')";

                            using (SqlCommand cmdCS = new SqlCommand(csSorgu, baglanti, islem))
                            {
                                cmdCS.Parameters.AddWithValue("@tur", evrakTuruMetin);
                                cmdCS.Parameters.AddWithValue("@seri", girilenSeriNo);
                                cmdCS.Parameters.AddWithValue("@vade", dpFVadeTarih.SelectedDate ?? DateTime.Now.AddMonths(1));
                                cmdCS.Parameters.AddWithValue("@veris", dpFOdemeTarih.SelectedDate ?? DateTime.Now);
                                cmdCS.Parameters.Add("@tutar", SqlDbType.Decimal).Value = DecimalCevir(txtFÖdenen.Text);

                             
                                cmdCS.Parameters.AddWithValue("@cari", firmaYetkilisi); 
                                cmdCS.Parameters.AddWithValue("@firmaID", seciliFirmaID); 
                                cmdCS.Parameters.AddWithValue("@acik", txtFOdemeAciklama.Text.Trim()); 

                                var sonuc = cmdCS.ExecuteScalar();
                                if (sonuc != null) yeniEvrakID = (int)sonuc;
                            }
                        }

                        string foSorgu = @"INSERT INTO FirmaOdemeleri (FirmaID, ToplamBorc, Odenen, IslemTarihi, EvrakTuru, OdemeDurumu, Aciklama) 
                                   VALUES (@fId, @borc, @odenen, @tarih, @evrak, @durum, @aciklama)";

                        using (SqlCommand cmdFO = new SqlCommand(foSorgu, baglanti, islem))
                        {
                            cmdFO.Parameters.AddWithValue("@fId", seciliFirmaID);
                            cmdFO.Parameters.Add("@borc", SqlDbType.Decimal).Value = DecimalCevir(txtFBorc.Text);
                            cmdFO.Parameters.Add("@odenen", SqlDbType.Decimal).Value = DecimalCevir(txtFÖdenen.Text);
                            cmdFO.Parameters.AddWithValue("@tarih", dpFOdemeTarih.SelectedDate ?? DateTime.Now);
                            cmdFO.Parameters.AddWithValue("@evrak", (yeniEvrakID > 0) ? yeniEvrakID.ToString() : evrakTuruMetin);
                            cmdFO.Parameters.AddWithValue("@durum", rbÖdendi.IsChecked == true ? "Ödendi" : "Ödenmedi");
                            cmdFO.Parameters.AddWithValue("@aciklama", txtFOdemeAciklama.Text.Trim());

                            cmdFO.ExecuteNonQuery();
                        }

                        islem.Commit();
                        GamaMesaj.Tamam($"Ödeme başarıyla tamamlandı.\nKayıt: {firmaAdi}", "BAŞARILI");

                        FirmaOdemeListele(seciliFirmaID);
                        Temizle();
                    }
                    catch (Exception ex)
                    {
                        islem.Rollback();
                        throw new Exception("İşlem Hatası (Geri Alındı): " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    GamaMesaj.Tamam("Sistem Hatası: " + ex.Message, "HATA");
                }
            }
        }
        private void btnFBorcTamamla_Click(object sender, RoutedEventArgs e)
        {
            if (seciliFirmaID == 0) { GamaMesaj.Tamam("Lütfen önce firma seçin!", "UYARI"); return; }
            if (string.IsNullOrWhiteSpace(txtFBorc.Text)) { GamaMesaj.Tamam("Lütfen borç miktarını girin!", "EKSİK BİLGİ"); return; }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();

              
                    string sorgu = @"INSERT INTO FirmaOdemeleri 
                            (FirmaID, ToplamBorc, Odenen, IslemTarihi, EvrakTuru, OdemeDurumu, Aciklama) 
                            VALUES 
                            (@fId, @borc, 0, @tarih, NULL, 'Ödenmedi', @aciklama)";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        komut.Parameters.AddWithValue("@fId", seciliFirmaID);
                        komut.Parameters.Add("@borc", SqlDbType.Decimal).Value = DecimalCevir(txtFBorc.Text);
                        komut.Parameters.AddWithValue("@tarih", dpFOdemeTarih.SelectedDate ?? DateTime.Now);
                        komut.Parameters.AddWithValue("@aciklama", txtFOdemeAciklama.Text.Trim() + " (Borç Kaydı)");

          

                        komut.ExecuteNonQuery();
                        GamaMesaj.Tamam("Borç kaydı başarıyla oluşturuldu.", "BAŞARILI");

                        FirmaOdemeListele(seciliFirmaID);

                        txtFBorc.Clear();
                        txtFOdemeAciklama.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Borç Yazma Hatası: " + ex.Message, "HATA");
            }
        }

        private void dgFirmaAltTablo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgFirmaAltTablo.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliOdemeID = Convert.ToInt32(satir["FirmaOdemeID"]);
                    txtFBorc.Text = satir["ToplamBorc"].ToString();
                    txtFÖdenen.Text = satir["Odenen"].ToString();
                    txtFKalan.Text = satir["Kalan"].ToString();
                    txtFOdemeAciklama.Text = satir["Aciklama"].ToString();

                    if (satir["IslemTarihi"] != DBNull.Value)
                        dpFOdemeTarih.SelectedDate = Convert.ToDateTime(satir["IslemTarihi"]);

                    string odemeDurumu = satir["OdemeDurumu"].ToString()!;
                    rbÖdendi.IsChecked = (odemeDurumu == "Ödendi");
                    rbÖdenmedi.IsChecked = (odemeDurumu == "Ödenmedi");

                   
                    if (satir["EvrakTuru"] != DBNull.Value)
                    {
                        string gelenTur = satir["EvrakTuru"].ToString()!;
                        foreach (ComboBoxItem item in cmbFTuru.Items)
                        {
                            if (item.Content.ToString() == gelenTur)
                            {
                                cmbFTuru.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    GamaMesaj.Tamam("Seçim işlenirken hata oluştu: " + ex.Message, "HATA");
                }
            }
        }
        private decimal DecimalCevir(string metin)
        {
            if (string.IsNullOrWhiteSpace(metin)) return 0;

            

            string temizMetin = metin.Replace(" ", ""); 

           
            if (decimal.TryParse(temizMetin.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal sonuc))
            {
                return sonuc;
            }

            return 0;
        }

        private void dgFirmaListesi_AutoGeneratedColumns(object sender, EventArgs e)
        {
            if (!(sender is DataGrid dg)) return;

            foreach (var col in dg.Columns)
            {
                string header = col.Header?.ToString() ?? "";

                
                if (header == "FirmaID" || header == "FirmaKayitTarihi")
                {
                    col.Visibility = Visibility.Collapsed;
                    continue;
                }

                
                switch (header)
                {
                    case "FirmaAd": col.Header = "Firma Adı"; break;
                    case "FirmaYetkilisi": col.Header = "Yetkili Kişi"; break;
                    case "Telefon": col.Header = "İletişim No"; break;
                    case "Adres": col.Header = "Firma Adresi"; break;
                    case "Gmail": col.Header = "E-Posta"; break;
                    case "Aciklama": col.Header = "Genel Notlar"; break;
                }
            }
        }

        private void dgFirmaAltTablo_AutoGeneratedColumns(object sender, EventArgs e)
        {
            if (!(sender is DataGrid dg)) return;

            foreach (var col in dg.Columns)
            {
                string header = col.Header?.ToString() ?? "";

               
                if (header == "FirmaOdemeID" || header == "FirmaID" || header == "EvrakTuru")
                {
                    col.Visibility = Visibility.Collapsed;
                    continue;
                }

              
                switch (header)
                {
                    case "ToplamBorc": col.Header = "Toplam Borç (₺)"; break;
                    case "Odenen": col.Header = "Ödenen (₺)"; break;
                    case "Kalan": col.Header = "Kalan Borç (₺)"; break;
                    case "IslemTarihi": col.Header = "İşlem Tarihi"; break;
                    case "GosterilecekEvrakTuru": col.Header = "Ödeme Tipi / Evrak"; break;
                    case "OdemeDurumu": col.Header = "Durum"; break;
                    case "Aciklama": col.Header = "Açıklama"; break;
                }

                
                if (col is DataGridTextColumn textCol && textCol.Binding is System.Windows.Data.Binding binding)
                {
                    if (header == "IslemTarihi")
                        binding.StringFormat = "dd.MM.yyyy HH:mm";
                    else if (header == "ToplamBorc" || header == "Odenen" || header == "Kalan")
                        binding.StringFormat = "N2"; 
                }
            }
        }

        private void txtFirmaAra2_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string ara = txtFirmaAra2.Text.Trim();
                    string sorgu = "SELECT * FROM FirmaOdemeleri WHERE EvrakTuru LIKE @ara OR Aciklama LIKE @ara";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@ara", "%" + ara + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgFirmaAltTablo.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void btnFFiltrele1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    
                    string sorgu = "SELECT * FROM Firmalar WHERE 1=1";

                    
                    if (dpFBaslangicFiltre1.SelectedDate != null && dpFBitisFiltre1.SelectedDate != null)
                    {
                        sorgu += " AND FirmaKayitTarihi BETWEEN @t1 AND @t2";
                    }

                    sorgu += " ORDER BY FirmaKayitTarihi DESC";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        if (dpFBaslangicFiltre1.SelectedDate != null && dpFBitisFiltre1.SelectedDate != null)
                        {
                            komut.Parameters.Add("@t1", SqlDbType.Date).Value = dpFBaslangicFiltre1.SelectedDate.Value.Date;
                            komut.Parameters.Add("@t2", SqlDbType.Date).Value = dpFBitisFiltre1.SelectedDate.Value.Date;
                        }

                        SqlDataAdapter da = new SqlDataAdapter(komut);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgFirmaListesi.ItemsSource = null;
                        dgFirmaListesi.ItemsSource = dt.DefaultView;

                        if (dt.Rows.Count == 0) GamaMesaj.Tamam("Seçili tarihlerde firma kaydı bulunamadı.", "BİLGİ");
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Firma Filtreleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnFFiltrele2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    
                    string sorgu = "SELECT * FROM FirmaOdemeleri WHERE 1=1";

                  
                    if (dpFBaslangicFiltre2.SelectedDate != null && dpFBitisFiltre2.SelectedDate != null)
                    {
                        sorgu += " AND IslemTarihi BETWEEN @t1 AND @t2";
                    }

                    sorgu += " ORDER BY IslemTarihi DESC";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        if (dpFBaslangicFiltre2.SelectedDate != null && dpFBitisFiltre2.SelectedDate != null)
                        {
                            komut.Parameters.Add("@t1", SqlDbType.Date).Value = dpFBaslangicFiltre2.SelectedDate.Value.Date;
                            komut.Parameters.Add("@t2", SqlDbType.Date).Value = dpFBitisFiltre2.SelectedDate.Value.Date;
                        }

                        SqlDataAdapter da = new SqlDataAdapter(komut);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgFirmaAltTablo.ItemsSource = null;
                        dgFirmaAltTablo.ItemsSource = dt.DefaultView;

                        if (dt.Rows.Count == 0) GamaMesaj.Tamam("Seçili tarihlerde firma ödeme kaydı bulunamadı.", "BİLGİ");
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Firma Ödeme Filtreleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnYenile_Click(object sender, RoutedEventArgs e)
        {
            FirmaListele();
        }

        private void btnDetay_Click(object sender, RoutedEventArgs e)
        {
          
            Detay detaySayfasi = new Detay();

       
            this.SayfaIcerikKonteynir.Content = detaySayfasi;
        }

       
    }
}