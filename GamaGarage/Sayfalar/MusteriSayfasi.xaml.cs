using GamaGarage.mesagekutu; 
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GamaGarage.Sayfalar
{
    public partial class MusteriSayfasi : UserControl
    {
        byte[] secilenResimBaytlari = null!;
        int seciliMusteriID = 0;
        int seciliIslemID = 0; 

        public MusteriSayfasi()
        {
            InitializeComponent();
            MusteriListele();
        }


        private void MusteriListele()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                   
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Musteriler ORDER BY Ad ASC", baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgMusteriListesi.ItemsSource = null;
                    dgMusteriListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Müşteri listesi yüklenirken hata oluştu: " + ex.Message, "SİSTEM HATASI");
            }
        }

        private void OdemeleriListele(int mID)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM MusteriOdemeleri WHERE MusteriID = @mid ORDER BY IslemTarihi DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@mid", mID);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgMusteriOdeme.ItemsSource = dt.DefaultView;
                }
            }
            catch {  }
        }

       

        private void btnMusteriEkle_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMAd.Text) || string.IsNullOrWhiteSpace(plaka.Text))
            {
                GamaMesaj.Tamam("Lütfen en azından Müşteri Adı ve Araç Plakası bilgilerini girin.", "EKSİK BİLGİ");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"INSERT INTO Musteriler (Ad, Soyad, AracPlaka, AlisTarihi, TeslimTarihi, Gmail, Telefon, Foto) 
                                    VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", txtMAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtMSoyad.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", plaka.Text.Trim());
                    komut.Parameters.AddWithValue("@p4", (object)dpAlisTarihi.SelectedDate! ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p5", (object)dpTeslimTarihi.SelectedDate! ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p6", txtMGmail.Text.Trim());
                    komut.Parameters.AddWithValue("@p7", txtMTelefon.Text.Trim());
                    komut.Parameters.Add("@p8", SqlDbType.VarBinary).Value = (object)secilenResimBaytlari ?? DBNull.Value;

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Müşteri kaydı başarıyla oluşturuldu.", "BAŞARILI");
                    MusteriListele();
                    Temizle();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ekleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnMusteriGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (seciliMusteriID == 0)
            {
                GamaMesaj.Tamam("Lütfen güncellemek istediğiniz müşteriyi listeden seçin.", "SEÇİM YAPILMADI");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"UPDATE Musteriler SET Ad=@p1, Soyad=@p2, AracPlaka=@p3, AlisTarihi=@p4, 
                                    TeslimTarihi=@p5, Gmail=@p6, Telefon=@p7, Foto=@p8 WHERE MusteriID=@id";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", txtMAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtMSoyad.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", plaka.Text.Trim());
                    komut.Parameters.AddWithValue("@p4", (object)dpAlisTarihi.SelectedDate! ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p5", (object)dpTeslimTarihi.SelectedDate! ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p6", txtMGmail.Text.Trim());
                    komut.Parameters.AddWithValue("@p7", txtMTelefon.Text.Trim());
                    komut.Parameters.AddWithValue("@id", seciliMusteriID);
                    komut.Parameters.Add("@p8", SqlDbType.VarBinary).Value = (object)secilenResimBaytlari ?? DBNull.Value;

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Müşteri bilgileri güncellendi.", "GÜNCELLENDİ");
                    MusteriListele();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Güncelleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnMusteriSil_Click(object sender, RoutedEventArgs e)
        {
            if (seciliMusteriID == 0) return;

            if (GamaMesaj.Onay("Bu müşteriyi silmek istediğinize emin misiniz? (Bu işlem geri alınamaz)", "SİLME ONAYI"))
            {
                try
                {
                    using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                    {
                        baglanti.Open();
                        SqlCommand komut = new SqlCommand("DELETE FROM Musteriler WHERE MusteriID=@id", baglanti);
                        komut.Parameters.AddWithValue("@id", seciliMusteriID);
                        komut.ExecuteNonQuery();

                        GamaMesaj.Tamam("Müşteri kaydı silindi.", "BİLGİ");
                        MusteriListele();
                        Temizle();
                    }
                }
                catch (Exception ex) { GamaMesaj.Tamam("Silme Hatası: " + ex.Message, "HATA"); }
            }
        }

       

        private void btnMOdemeTamamla_Click(object sender, RoutedEventArgs e)
        {
            if (seciliMusteriID == 0)
            {
                GamaMesaj.Tamam("Ödeme eklemek için bir müşteri seçmelisiniz.", "UYARI");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"INSERT INTO MusteriOdemeleri (MusteriID, ToplamTutar, Odenen, IslemTarihi, Aciklama) 
                                    VALUES (@mid, @toplam, @odenen, @tarih, @aciklama)";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@mid", seciliMusteriID);
                    komut.Parameters.Add("@toplam", SqlDbType.Decimal).Value = DecimalCevir(txtTutar.Text);
                    komut.Parameters.Add("@odenen", SqlDbType.Decimal).Value = DecimalCevir(txtÖdenen.Text);
                    komut.Parameters.AddWithValue("@tarih", dpMOdemeTarih.SelectedDate ?? DateTime.Now);
                    komut.Parameters.AddWithValue("@aciklama", txtMAciklama.Text.Trim());

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Ödeme kaydı başarıyla eklendi.", "ÖDEME ALINDI");
                    OdemeleriListele(seciliMusteriID);
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ödeme Hatası: " + ex.Message, "HATA"); }
        }

       

        private void txtMusteriAra1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string ara = txtMusteriAra1.Text.Trim();
                    string sorgu = "SELECT * FROM Musteriler WHERE Ad LIKE @ara OR AracPlaka LIKE @ara OR Soyad LIKE @ara";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@ara", "%" + ara + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgMusteriListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void btnFiltrele1_Click(object sender, RoutedEventArgs e)
        {
            if (dpBaslangicFiltre1.SelectedDate == null || dpBitisFiltre1.SelectedDate == null)
            {
                GamaMesaj.Tamam("Lütfen tarih aralığı seçin.", "BİLGİ");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM Musteriler WHERE AlisTarihi BETWEEN @t1 AND @t2 ORDER BY AlisTarihi DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@t1", dpBaslangicFiltre1.SelectedDate.Value.Date);
                    da.SelectCommand.Parameters.AddWithValue("@t2", dpBitisFiltre1.SelectedDate.Value.Date.AddDays(1).AddTicks(-1));

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgMusteriListesi.ItemsSource = dt.DefaultView;
                    if (dt.Rows.Count == 0) GamaMesaj.Tamam("Bu tarihler arasında müşteri bulunamadı.", "SONUÇ YOK");
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Filtreleme Hatası: " + ex.Message, "HATA"); }
        }



        private void btnFiltrele2_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM MusteriOdemeleri WHERE 1=1";
                    if (dpBaslangicFiltre2.SelectedDate != null && dpBitisFiltre2.SelectedDate != null)
                    {
                        sorgu += " AND IslemTarihi BETWEEN @t1 AND @t2";
                    }

                    sorgu += " ORDER BY IslemTarihi DESC";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        if (dpBaslangicFiltre2.SelectedDate != null && dpBitisFiltre2.SelectedDate != null)
                        {
                            komut.Parameters.Add("@t1", SqlDbType.Date).Value = dpBaslangicFiltre2.SelectedDate.Value.Date;

                            komut.Parameters.Add("@t2", SqlDbType.Date).Value = dpBitisFiltre2.SelectedDate.Value.Date;
                        }

                        SqlDataAdapter da = new SqlDataAdapter(komut);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgMusteriOdeme.ItemsSource = null;
                        dgMusteriOdeme.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Müşteri Ödemeleri Filtreleme Hatası: " + ex.Message, "HATA"); }
        }








        private void txtMusteriAra2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (seciliMusteriID == 0) return;
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string ara = txtMusteriAra2.Text.Trim();
                    string sorgu = "SELECT * FROM MusteriOdemeleri WHERE MusteriID=@mid AND Aciklama LIKE @ara";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@mid", seciliMusteriID);
                    da.SelectCommand.Parameters.AddWithValue("@ara", "%" + ara + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgMusteriOdeme.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

    


        private void dgMusteriListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMusteriListesi.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliMusteriID = Convert.ToInt32(satir["MusteriID"]);
                    txtMAd.Text = satir["Ad"].ToString();
                    txtMSoyad.Text = satir["Soyad"].ToString();

                    txtMOAD.Text = satir["Ad"].ToString();
                    txtMOSoyad.Text = satir["Soyad"].ToString();
                    plaka.Text = satir["AracPlaka"].ToString();
                    txtMGmail.Text = satir["Gmail"].ToString();
                    txtMTelefon.Text = satir["Telefon"].ToString();

                    dpAlisTarihi.SelectedDate = satir["AlisTarihi"] != DBNull.Value ? Convert.ToDateTime(satir["AlisTarihi"]) : null;
                    dpTeslimTarihi.SelectedDate = satir["TeslimTarihi"] != DBNull.Value ? Convert.ToDateTime(satir["TeslimTarihi"]) : null;

                    if (satir["Foto"] != DBNull.Value)
                    {
                        secilenResimBaytlari = (byte[])satir["Foto"];
                        imgMusteriFoto.ImageSource = ByteToImage(secilenResimBaytlari);
                    }
                    else
                    {
                        imgMusteriFoto.ImageSource = null;
                        secilenResimBaytlari = null!;
                    }

                    OdemeleriListele(seciliMusteriID);
                }
                catch { }
            }
        }

        private void dgMusteriOdeme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMusteriOdeme.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliIslemID = Convert.ToInt32(satir["IslemID"]);
                    txtTutar.Text = satir["ToplamTutar"].ToString();
                    txtÖdenen.Text = satir["Odenen"].ToString();
                    txtMAciklama.Text = satir["Aciklama"].ToString();

                    if (satir["IslemTarihi"] != DBNull.Value)
                        dpMOdemeTarih.SelectedDate = Convert.ToDateTime(satir["IslemTarihi"]);

                    decimal toplam = DecimalCevir(satir["ToplamTutar"].ToString()!);
                    decimal odenen = DecimalCevir(satir["Odenen"].ToString()!);
                    txtKalan.Text = (toplam - odenen).ToString("N2");
                }
                catch { }
            }
        }

        private void imgMusteriFoto_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Resim Dosyaları|*.jpg;*.png;*.jpeg" };
            if (ofd.ShowDialog() == true)
            {
                secilenResimBaytlari = File.ReadAllBytes(ofd.FileName);
                imgMusteriFoto.ImageSource = new BitmapImage(new Uri(ofd.FileName));
            }
        }

        private void Temizle()
        {
            seciliMusteriID = 0;
            seciliIslemID = 0;
            txtMAd.Clear(); txtMSoyad.Clear(); plaka.Clear(); txtMGmail.Clear(); txtMTelefon.Clear();
            txtTutar.Clear(); txtÖdenen.Clear(); txtKalan.Clear(); txtMAciklama.Clear();
            dpAlisTarihi.SelectedDate = null; dpTeslimTarihi.SelectedDate = null;
            dpMOdemeTarih.SelectedDate = null;
            imgMusteriFoto.ImageSource = null; secilenResimBaytlari = null!;
            dgMusteriOdeme.ItemsSource = null;
        }

        private decimal DecimalCevir(string metin)
        {
            if (string.IsNullOrEmpty(metin)) return 0;
            decimal.TryParse(metin.Replace(".", ","), out decimal sonuc);
            return sonuc;
        }

        private BitmapImage ByteToImage(byte[] array)
        {
            if (array == null || array.Length == 0) return null!;
            using (MemoryStream ms = new MemoryStream(array))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        private void btnMTemizle_Click(object sender, RoutedEventArgs e) => Temizle();

        private void dgMusteriListesi_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgMusteriListesi.Columns)
            {
                if (col.Header == null) continue;
                string header = col.Header.ToString()!;

            
                if (header == "MusteriID" || header == "Foto")
                    col.Visibility = Visibility.Collapsed;

                switch (header)
                {
                    case "Ad": col.Header = "Müşteri Adı"; break;
                    case "Soyad": col.Header = "Soyadı"; break;
                    case "AracPlaka": col.Header = "Araç Plakası"; break;
                    case "AlisTarihi": col.Header = "Alış Tarihi"; break;
                    case "TeslimTarihi": col.Header = "Teslim Tarihi"; break;
                    case "Gmail": col.Header = "E-Posta"; break;
                    case "Telefon": col.Header = "Telefon No"; break;
                }

                
                if (header == "AlisTarihi" || header == "TeslimTarihi")
                {
                    var textColumn = col as System.Windows.Controls.DataGridTextColumn;
                    if (textColumn != null && textColumn.Binding is System.Windows.Data.Binding binding)
                        binding.StringFormat = "dd.MM.yyyy";
                }
            }
        }

        private void dgMusteriOdeme_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgMusteriOdeme.Columns)
            {
                if (col.Header == null) continue;
                string header = col.Header.ToString()!;

             
                if (header == "IslemID" || header == "MusteriID")
                    col.Visibility = Visibility.Collapsed;

                
                switch (header)
                {
                    case "ToplamTutar": col.Header = "Toplam Tutar (₺)"; break;
                    case "Odenen": col.Header = "Ödenen Miktar (₺)"; break;
                    case "IslemTarihi": col.Header = "İşlem Tarihi"; break;
                    case "Aciklama": col.Header = "Açıklama / Not"; break;
                }

               
                var textColumn = col as System.Windows.Controls.DataGridTextColumn;
                if (textColumn != null && textColumn.Binding is System.Windows.Data.Binding binding)
                {
                    if (header == "ToplamTutar" || header == "Odenen")
                        binding.StringFormat = "N2"; 

                    if (header == "IslemTarihi")
                        binding.StringFormat = "dd.MM.yyyy HH:mm"; 
                }
            }
        }

        private void btnYenile_Click(object sender, RoutedEventArgs e)
        {
            MusteriListele();
        }

        private void btnDetay_Click(object sender, RoutedEventArgs e)
        {
            MusteriDetay detaySayfasi = new MusteriDetay();


            this.SayfaMDetay.Content = detaySayfasi;
        }
    }
}