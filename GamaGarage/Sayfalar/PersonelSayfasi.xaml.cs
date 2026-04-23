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
    public partial class PersonelSayfasi : UserControl
    {
        byte[] secilenResimBaytlari = null!;
        int seciliPersonelID = 0;
        int seciliOdemeID = 0; 

        public PersonelSayfasi()
        {
            InitializeComponent();
            PersonelListele();
        }

      

        private void PersonelListele()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Personeller ORDER BY Ad ASC", baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgPersonelListesi.ItemsSource = null;
                    dgPersonelListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Personel listesi yüklenirken hata oluştu: " + ex.Message, "SİSTEM HATASI");
            }
        }

        private bool BilgilerDogruMu()
        {
            if (string.IsNullOrWhiteSpace(txtAd.Text) || string.IsNullOrWhiteSpace(txtSoyad.Text))
            {
                GamaMesaj.Tamam("Ad ve Soyad alanları boş geçilemez!", "EKSİK BİLGİ");
                return false;
            }
            if (txtTC.Text.Length != 11)
            {
                GamaMesaj.Tamam("TC Kimlik numarası 11 hane olmalıdır!", "GEÇERSİZ TC");
                return false;
            }
            return true;
        }

     

        private void btnPersonelEkle_Click(object sender, RoutedEventArgs e)
        {
            if (!BilgilerDogruMu()) return;
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"INSERT INTO Personeller (Ad, Soyad, TC, BaslangicTarihi, Bolum, Gmail, Telefon, Durum, Foto) 
                                    VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9)";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", txtAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtSoyad.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", txtTC.Text.Trim());
                    komut.Parameters.AddWithValue("@p4", (object)dpBaslangicTarih.SelectedDate! ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p5", cmbBolum.Text ?? "");
                    komut.Parameters.AddWithValue("@p6", txtGmail.Text.Trim());
                    komut.Parameters.AddWithValue("@p7", txtTelefon.Text.Trim());
                    komut.Parameters.AddWithValue("@p8", rbCalisiyor.IsChecked == true);
                    komut.Parameters.Add("@p9", SqlDbType.VarBinary).Value = (object)secilenResimBaytlari ?? DBNull.Value;

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Yeni personel başarıyla kaydedildi.", "BAŞARILI");
                    PersonelListele();
                    Temizle();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ekleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnPersonelGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (seciliPersonelID == 0) { GamaMesaj.Tamam("Lütfen listeden bir personel seçin!", "UYARI"); return; }
            if (!BilgilerDogruMu()) return;
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"UPDATE Personeller SET Ad=@p1, Soyad=@p2, TC=@p3, BaslangicTarihi=@p4, 
                                    Bolum=@p5, Gmail=@p6, Telefon=@p7, Durum=@p8, Foto=@p9 WHERE ID=@id";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", txtAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtSoyad.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", txtTC.Text.Trim());
                    komut.Parameters.AddWithValue("@p4", (object)dpBaslangicTarih.SelectedDate! ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p5", cmbBolum.Text ?? "");
                    komut.Parameters.AddWithValue("@p6", txtGmail.Text.Trim());
                    komut.Parameters.AddWithValue("@p7", txtTelefon.Text.Trim());
                    komut.Parameters.AddWithValue("@p8", rbCalisiyor.IsChecked == true);
                    komut.Parameters.Add("@p9", SqlDbType.VarBinary).Value = (object)secilenResimBaytlari ?? DBNull.Value;
                    komut.Parameters.AddWithValue("@id", seciliPersonelID);

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Personel bilgileri güncellendi.", "BİLGİ");
                    PersonelListele();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Güncelleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnPersonelSil_Click(object sender, RoutedEventArgs e)
        {
            if (seciliPersonelID == 0) return;
            if (GamaMesaj.Onay("Bu personeli silmek istediğinize emin misiniz?", "SİLME ONAYI"))
            {
                try
                {
                    using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                    {
                        baglanti.Open();
                        SqlCommand komut = new SqlCommand("DELETE FROM Personeller WHERE ID=@id", baglanti);
                        komut.Parameters.AddWithValue("@id", seciliPersonelID);
                        komut.ExecuteNonQuery();
                        GamaMesaj.Tamam("Personel kaydı silindi.", "İŞLEM TAMAM");
                        PersonelListele();
                        Temizle();
                    }
                }
                catch (Exception ex) { GamaMesaj.Tamam("Silme Hatası: " + ex.Message, "HATA"); }
            }
        }

      

        private void OdemeleriListele(int pID)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM PersonelOdemeleri WHERE PersonelID = @pid ORDER BY Tarih DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@pid", pID);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgPersonelAlt.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void btnOdemeTamamla_Click(object sender, RoutedEventArgs e)
        {
            if (seciliPersonelID == 0) { GamaMesaj.Tamam("Önce listeden bir personel seçmelisiniz!", "UYARI"); return; }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"INSERT INTO PersonelOdemeleri (PersonelID, SaatlikUcret, GunlukUcret, Avans, Kesinti, Tarih, Ay, NetMaas, Aciklama) 
                                    VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", seciliPersonelID);
                    komut.Parameters.Add("@p2", SqlDbType.Decimal).Value = DecimalCevir(txtSaatlik.Text);
                    komut.Parameters.Add("@p3", SqlDbType.Decimal).Value = DecimalCevir(txtGunluk.Text);
                    komut.Parameters.Add("@p4", SqlDbType.Decimal).Value = DecimalCevir(txtAvans.Text);
                    komut.Parameters.Add("@p5", SqlDbType.Decimal).Value = DecimalCevir(txtKesinti.Text);
                    komut.Parameters.AddWithValue("@p6", (object)dpOdemeTarih.SelectedDate! ?? DateTime.Now);
                    komut.Parameters.AddWithValue("@p7", cmbAy.Text ?? "");
                    komut.Parameters.Add("@p8", SqlDbType.Decimal).Value = DecimalCevir(txtNetMaas.Text);
                    komut.Parameters.AddWithValue("@p9", txtAciklama.Text.Trim());

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Ödeme kaydı başarıyla oluşturuldu.", "BAŞARILI");
                    OdemeleriListele(seciliPersonelID);
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ödeme Kayıt Hatası: " + ex.Message, "HATA"); }
        }



        private void txtPersonelAra1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string arama = txtPersonelAra1.Text.Trim();
                    string sorgu = "SELECT * FROM Personeller WHERE Ad LIKE @search OR Soyad LIKE @search OR TC LIKE @search";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@search", "%" + arama + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgPersonelListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void btnfiltrele1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM Personeller WHERE 1=1"; 
                    if (dpBaslangicFiltre1.SelectedDate != null && dpBitisFiltre1.SelectedDate != null)
                    {
                        sorgu += " AND BaslangicTarihi BETWEEN @t1 AND @t2";
                    }

                    sorgu += " ORDER BY BaslangicTarihi DESC";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        if (dpBaslangicFiltre1.SelectedDate != null && dpBitisFiltre1.SelectedDate != null)
                        {
                            komut.Parameters.Add("@t1", SqlDbType.Date).Value = dpBaslangicFiltre1.SelectedDate.Value.Date;
                          
                            komut.Parameters.Add("@t2", SqlDbType.Date).Value = dpBitisFiltre1.SelectedDate.Value.Date;
                        }

                        SqlDataAdapter da = new SqlDataAdapter(komut);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgPersonelListesi.ItemsSource = null;
                        dgPersonelListesi.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Personel Filtreleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnfiltrele2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM PersonelOdemeleri WHERE 1=1";
                    if (dpBaslangicFiltre2.SelectedDate != null && dpBitisFiltre2.SelectedDate != null)
                    {
                        sorgu += " AND Tarih BETWEEN @t1 AND @t2";
                    }

                    sorgu += " ORDER BY Tarih DESC";

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

                        dgPersonelAlt.ItemsSource = null;
                        dgPersonelAlt.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Personel Ödemeleri Filtreleme Hatası: " + ex.Message, "HATA"); }
        }

        private void txtPersonelAra2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (seciliPersonelID == 0) return;
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string arama = txtPersonelAra2.Text.Trim();
                    string sorgu = "SELECT * FROM PersonelOdemeleri WHERE PersonelID=@pid AND (Ay LIKE @ara OR Aciklama LIKE @ara)";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@pid", seciliPersonelID);
                    da.SelectCommand.Parameters.AddWithValue("@ara", "%" + arama + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgPersonelAlt.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

      

        private void dgPersonelListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPersonelListesi.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliPersonelID = Convert.ToInt32(satir["ID"]);
                    txtAd.Text = satir["Ad"].ToString();
                    txtSoyad.Text = satir["Soyad"].ToString();
                    txtOAD.Text = satir["Ad"].ToString();
                    txtOSoyad.Text = satir["Soyad"].ToString();

                    txtTC.Text = satir["TC"].ToString();
                    txtTelefon.Text = satir["Telefon"].ToString();
                    txtGmail.Text = satir["Gmail"].ToString();
                    cmbBolum.Text = satir["Bolum"].ToString();

                    if (satir["BaslangicTarihi"] != DBNull.Value)
                        dpBaslangicTarih.SelectedDate = Convert.ToDateTime(satir["BaslangicTarihi"]);

                    bool durum = satir["Durum"] != DBNull.Value && Convert.ToBoolean(satir["Durum"]);
                    rbCalisiyor.IsChecked = durum;
                    rbCalismiyor.IsChecked = !durum;

                    if (satir["Foto"] != DBNull.Value)
                    {
                        secilenResimBaytlari = (byte[])satir["Foto"];
                        imgPersonelFoto.ImageSource = ByteToImage(secilenResimBaytlari);
                    }
                    else { imgPersonelFoto.ImageSource = null; secilenResimBaytlari = null!; }

                    OdemeleriListele(seciliPersonelID);
                }
                catch { }
            }
        }

        private void dgPersonelAlt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPersonelAlt.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliOdemeID = Convert.ToInt32(satir["OdemeID"]);
                    txtSaatlik.Text = satir["SaatlikUcret"].ToString();
                    txtGunluk.Text = satir["GunlukUcret"].ToString();
                    txtAvans.Text = satir["Avans"].ToString();
                    txtKesinti.Text = satir["Kesinti"].ToString();
                    txtNetMaas.Text = satir["NetMaas"].ToString();
                    txtAciklama.Text = satir["Aciklama"].ToString();
                    cmbAy.Text = satir["Ay"].ToString();

                    if (satir["Tarih"] != DBNull.Value)
                        dpOdemeTarih.SelectedDate = Convert.ToDateTime(satir["Tarih"]);
                }
                catch { }
            }
        }

        private void imgPersonelFoto_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Resimler|*.jpg;*.png;*.jpeg" };
            if (ofd.ShowDialog() == true)
            {
                secilenResimBaytlari = File.ReadAllBytes(ofd.FileName);
                imgPersonelFoto.ImageSource = new BitmapImage(new Uri(ofd.FileName));
            }
        }

        private void Temizle()
        {
            seciliPersonelID = 0;
            seciliOdemeID = 0;
            txtAd.Clear(); txtSoyad.Clear(); txtTC.Clear(); txtTelefon.Clear(); txtGmail.Clear();
            txtSaatlik.Clear(); txtGunluk.Clear(); txtAvans.Clear(); txtKesinti.Clear(); txtNetMaas.Clear(); txtAciklama.Clear();
            dpBaslangicTarih.SelectedDate = null;
            dpOdemeTarih.SelectedDate = null;
            cmbBolum.SelectedIndex = -1;
            cmbAy.SelectedIndex = -1;
            rbCalisiyor.IsChecked = true;
            imgPersonelFoto.ImageSource = null;
            secilenResimBaytlari = null!;
            dgPersonelAlt.ItemsSource = null;
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

        private void btnTemizle_Click(object sender, RoutedEventArgs e) => Temizle();

        private void dgPersonelListesi_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgPersonelListesi.Columns)
            {
                string header = col.Header?.ToString() ?? "";

                if (header == "ID" || header == "Foto" || header == "Durum")
                {
                    col.Visibility = Visibility.Collapsed;
                    continue;
                }

              
                switch (header)
                {
                    case "Ad": col.Header = "Adı"; break;
                    case "Soyad": col.Header = "Soyadı"; break;
                    case "TC": col.Header = "TC Kimlik No"; break;
                    case "BaslangicTarihi": col.Header = "İşe Giriş Tarihi"; break;
                    case "Bolum": col.Header = "Departman / Bölüm"; break;
                    case "Gmail": col.Header = "E-Posta"; break;
                    case "Telefon": col.Header = "İletişim"; break;
                }

            
                if (header == "BaslangicTarihi" && col is DataGridTextColumn textCol)
                {
                    ((System.Windows.Data.Binding)textCol.Binding).StringFormat = "dd.MM.yyyy";
                }
            }
        }

        private void dgPersonelAlt_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgPersonelAlt.Columns)
            {
                string header = col.Header?.ToString() ?? "";

                if (header == "OdemeID" || header == "PersonelID")
                {
                    col.Visibility = Visibility.Collapsed;
                    continue;
                }

               
                switch (header)
                {
                    case "SaatlikUcret": col.Header = "Saatlik Ücret"; break;
                    case "GunlukUcret": col.Header = "Günlük Ücret"; break;
                    case "NetMaas": col.Header = "Ödenen Net Maaş"; break;
                    case "Aciklama": col.Header = "Notlar / Açıklama"; break;
                    case "Ay": col.Header = "Dönem (Ay)"; break;
                }

                if (col is DataGridTextColumn textCol && textCol.Binding is System.Windows.Data.Binding binding)
                {
                  
                    if (header.Contains("Ucret") || header.Contains("NetMaas") || header == "Avans" || header == "Kesinti")
                    {
                        binding.StringFormat = "N2";
                        Style sagStyle = new Style(typeof(DataGridCell));
                        sagStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                        textCol.CellStyle = sagStyle;
                    }

                  
                    if (header == "Tarih")
                    {
                        binding.StringFormat = "dd.MM.yyyy";
                    }
                }
            }
        }

        private void btnYenile_Click(object sender, RoutedEventArgs e)
        {
            PersonelListele();
        }

        
    }
}