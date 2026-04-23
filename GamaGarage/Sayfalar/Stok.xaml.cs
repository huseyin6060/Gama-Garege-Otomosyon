using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GamaGarage.mesagekutu;

namespace GamaGarage.Sayfalar
{
    public partial class Stok : UserControl
    {
        int seciliUrunID = 0;

        public Stok()
        {
            InitializeComponent();
            UrunleriGetir();
            StokListele();
            txtUrunAd.IsReadOnly = true; 
        }



        private void UrunleriGetir()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    
                    string sorgu = "SELECT UrunID, UrunAd, StokAdedi, Fiyat FROM URUNLER ORDER BY UrunAd ASC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgUrunleriListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ürün listesi yüklenemedi: " + ex.Message, "HATA"); }
        }

        private void StokListele()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = @"SELECT S.ID, S.URUNLER as UrunID, U.UrunAd, S.ADET, S.BİRİMFİYATİ, 
                           (S.ADET * S.BİRİMFİYATİ) as ToplamTutar, S.İSLEMTARİHİ, S.ACİKLAMA 
                           FROM STOKLAR S 
                           INNER JOIN URUNLER U ON S.URUNLER = U.UrunID 
                           ORDER BY S.İSLEMTARİHİ DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgSatısTablo.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Stok geçmişi yüklenemedi: " + ex.Message, "HATA"); }
        }






        private void dgUrunleriListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUrunleriListesi.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliUrunID = Convert.ToInt32(satir["UrunID"]);
                    txtUrunAd.Text = satir["UrunAd"].ToString();
                 
                    txtBirimfiyat.Text = satir["Fiyat"]?.ToString();
                    txtAdet.Focus();
                }
                catch (Exception ex) { GamaMesaj.Tamam("Seçim Hatası: " + ex.Message, "UYARI"); }
            }
        }

        private void txtUrunAra1_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                string ara = txtUrunAra1.Text.Trim();    
                string sorgu = "SELECT * FROM URUNLER WHERE UrunAd LIKE @ara";
                SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                da.SelectCommand.Parameters.AddWithValue("@ara", "%" + ara + "%");
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgUrunleriListesi.ItemsSource = dt.DefaultView;
            }
        }



        private void btnStokEkle_Click(object sender, RoutedEventArgs e)
        {
            if (seciliUrunID == 0 || string.IsNullOrWhiteSpace(txtAdet.Text)) return;

            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                baglanti.Open();
                SqlTransaction islem = baglanti.BeginTransaction();
                try
                {
               
                    string stokSorgu = "INSERT INTO STOKLAR (BİRİMFİYATİ, İSLEMTARİHİ, ACİKLAMA, URUNLER, ADET) VALUES (@p1, @p2, @p3, @p4, @p5)";
                    SqlCommand stokKomut = new SqlCommand(stokSorgu, baglanti, islem);
                    stokKomut.Parameters.Add("@p1", SqlDbType.Decimal).Value = decimal.Parse(txtBirimfiyat.Text.Replace(".", ","));
                    stokKomut.Parameters.AddWithValue("@p2", dpUrunCikis.SelectedDate ?? DateTime.Now);
                    stokKomut.Parameters.AddWithValue("@p3", txtSAciklama.Text.Trim());
                    stokKomut.Parameters.AddWithValue("@p4", seciliUrunID);
                    stokKomut.Parameters.AddWithValue("@p5", int.Parse(txtAdet.Text));
                    stokKomut.ExecuteNonQuery();

                  
                    string urunGuncelleSorgu = "UPDATE URUNLER SET StokAdedi = StokAdedi - @dusulecekAdet WHERE UrunID = @id";
                    SqlCommand urunKomut = new SqlCommand(urunGuncelleSorgu, baglanti, islem);
                    urunKomut.Parameters.AddWithValue("@dusulecekAdet", int.Parse(txtAdet.Text));
                    urunKomut.Parameters.AddWithValue("@id", seciliUrunID);
                    urunKomut.ExecuteNonQuery();

                    islem.Commit();
                    GamaMesaj.Tamam("Satış kaydedildi.", "BAŞARILI");
                    StokListele(); UrunleriGetir(); StokTemizle();
                }
                catch (Exception ex) { islem.Rollback(); GamaMesaj.Tamam("Hata: " + ex.Message, "KRİTİK HATA"); }
            }
        }

        private void btnStokSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgSatısTablo.SelectedItem is DataRowView satir)
            {
                if (!GamaMesaj.Onay("İşlem iptal edilsin mi?", "ONAY")) return;

                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    SqlTransaction islem = baglanti.BeginTransaction();
                    try
                    {
                   
                        SqlCommand silKomut = new SqlCommand("DELETE FROM STOKLAR WHERE ID = @id", baglanti, islem);
                        silKomut.Parameters.AddWithValue("@id", satir["ID"]);
                        silKomut.ExecuteNonQuery();

                       
                        SqlCommand iadeKomut = new SqlCommand("UPDATE URUNLER SET StokAdedi = StokAdedi + @miktar WHERE UrunID = @uid", baglanti, islem);
                        iadeKomut.Parameters.AddWithValue("@miktar", satir["ADET"]);
                        iadeKomut.Parameters.AddWithValue("@uid", satir["UrunID"]);
                        iadeKomut.ExecuteNonQuery();

                        islem.Commit();
                        StokListele(); UrunleriGetir();
                    }
                    catch { islem.Rollback(); }
                }
            }
        }


        private void StokTemizle()
        {
            seciliUrunID = 0;
            txtBirimfiyat.Clear();
            txtSAciklama.Clear();
            txtUrunAd.Clear();
            txtAdet.Clear();
            dpUrunCikis.SelectedDate = null;
            dgUrunleriListesi.SelectedIndex = -1;
            dgSatısTablo.SelectedIndex = -1;
        }

        private void btnSTemizle_Click(object sender, RoutedEventArgs e) { StokTemizle(); }

        private void dgUrunleriListesi_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgUrunleriListesi.Columns)
            {
                if (col.Header == null) continue;
                string header = col.Header.ToString()!;

               
                if (header == "UrunID" || header == "KategoriID")
                    col.Visibility = Visibility.Collapsed;

         
                switch (header)
                {
                    case "UrunAd": col.Header = "Ürün Adı"; break;
                    case "StokAdedi": col.Header = "Mevcut Stok"; break;
                    case "Fiyat": col.Header = "Satış Fiyatı (₺)"; break;
                }

               
                if (header == "Fiyat")
                {
                    var textColumn = col as System.Windows.Controls.DataGridTextColumn;
                    if (textColumn != null && textColumn.Binding is System.Windows.Data.Binding binding)
                        binding.StringFormat = "N2";
                }
            }
        }

        private void dgSatısTablo_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgSatısTablo.Columns)
            {
                if (col.Header == null) continue;
                string header = col.Header.ToString()!;

           
                if (header == "ID" || header == "UrunID")
                    col.Visibility = Visibility.Collapsed;

          
                switch (header)
                {
                    case "UrunAd": col.Header = "Satılan Ürün"; break;
                    case "ADET": col.Header = "Miktar"; break;
                    case "BİRİMFİYATİ": col.Header = "Birim Fiyat"; break;
                    case "ToplamTutar": col.Header = "Toplam Tutar (₺)"; break;
                    case "İSLEMTARİHİ": col.Header = "İşlem Tarihi"; break;
                    case "ACİKLAMA": col.Header = "Not/Açıklama"; break;
                }

               
                if (header == "İSLEMTARİHİ" || header == "BİRİMFİYATİ" || header == "ToplamTutar")
                {
                    var textColumn = col as System.Windows.Controls.DataGridTextColumn;
                    if (textColumn != null && textColumn.Binding is System.Windows.Data.Binding binding)
                    {
                        if (header == "İSLEMTARİHİ")
                            binding.StringFormat = "dd.MM.yyyy HH:mm";
                        else
                            binding.StringFormat = "N2";
                    }
                }
            }
        }

        private void btnUrunFiltrele1_Click(object sender, RoutedEventArgs e)
        {
            if (dpBaslangicFiltre1.SelectedDate == null || dpBitisFiltre1.SelectedDate == null)
            {
                GamaMesaj.Tamam("Lütfen tarih aralığı seçin.", "UYARI");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = @"SELECT S.ID, S.URUNLER as UrunID, U.UrunAd, S.ADET, S.BİRİMFİYATİ, 
                                   (S.ADET * S.BİRİMFİYATİ) as ToplamTutar, S.İSLEMTARİHİ, S.ACİKLAMA 
                                   FROM STOKLAR S 
                                   INNER JOIN URUNLER U ON S.URUNLER = U.UrunID 
                                   WHERE S.İSLEMTARİHİ BETWEEN @bas AND @bit
                                   ORDER BY S.İSLEMTARİHİ DESC";

                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@bas", dpBaslangicFiltre1.SelectedDate.Value);
                    da.SelectCommand.Parameters.AddWithValue("@bit", dpBitisFiltre1.SelectedDate.Value.AddDays(1));

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgSatısTablo.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Filtreleme hatası: " + ex.Message, "HATA"); }
        }

        
        private void dgSatısTablo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSatısTablo.SelectedItem is DataRowView satir)
            {
                try
                {
                    
                    seciliUrunID = Convert.ToInt32(satir["UrunID"]);
                    txtUrunAd.Text = satir["UrunAd"].ToString();
                    txtAdet.Text = satir["ADET"].ToString();
                    txtBirimfiyat.Text = satir["BİRİMFİYATİ"].ToString();
                    txtSAciklama.Text = satir["ACİKLAMA"].ToString();

                    if (satir["İSLEMTARİHİ"] != DBNull.Value)
                        dpUrunCikis.SelectedDate = Convert.ToDateTime(satir["İSLEMTARİHİ"]);
                }
                catch (Exception ex)
                {
                    GamaMesaj.Tamam("Seçim işlenemedi: " + ex.Message, "UYARI");
                }
            }
        }

        
        private void btnStokGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgSatısTablo.SelectedItem == null) { GamaMesaj.Tamam("Lütfen güncellemek istediğiniz işlemi alt tablodan seçin!", "SEÇİM YOK"); return; }

            DataRowView satir = (DataRowView)dgSatısTablo.SelectedItem;
            int eskiAdet = Convert.ToInt32(satir["ADET"]);
            int yeniAdet = int.Parse(txtAdet.Text);
            int stokID = Convert.ToInt32(satir["ID"]);
            int urunID = Convert.ToInt32(satir["UrunID"]);
            decimal yeniFiyat = decimal.Parse(txtBirimfiyat.Text.Replace(".", ","));

            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                try
                {
                    baglanti.Open();
                    SqlTransaction islem = baglanti.BeginTransaction();
                    try
                    {
                        
                        string guncelleSorgu = "UPDATE STOKLAR SET ADET=@p1, BİRİMFİYATİ=@p2, ACİKLAMA=@p3 WHERE ID=@id";
                        SqlCommand kmt = new SqlCommand(guncelleSorgu, baglanti, islem);
                        kmt.Parameters.AddWithValue("@p1", yeniAdet);
                        kmt.Parameters.AddWithValue("@p2", yeniFiyat);
                        kmt.Parameters.AddWithValue("@p3", txtSAciklama.Text.Trim());
                        kmt.Parameters.AddWithValue("@id", stokID);
                        kmt.ExecuteNonQuery();

                      
                        int fark = yeniAdet - eskiAdet;
                        string stokDuzelt = "UPDATE URUNLER SET StokAdedi = StokAdedi - @fark WHERE UrunID = @uid";
                        SqlCommand kmt2 = new SqlCommand(stokDuzelt, baglanti, islem);
                        kmt2.Parameters.AddWithValue("@fark", fark);
                        kmt2.Parameters.AddWithValue("@uid", urunID);
                        kmt2.ExecuteNonQuery();

                        islem.Commit();
                        GamaMesaj.Tamam("İşlem başarıyla güncellendi.", "BAŞARILI");
                        StokListele();
                        UrunleriGetir();
                    }
                    catch (Exception) 
                    {
                        islem.Rollback();
                        throw;
                    }
                }
                catch (Exception ex) { GamaMesaj.Tamam("Hata: " + ex.Message, "GÜNCELLEME HATASI"); }
            }
        }

     
        private void btnStokGerial_Click(object sender, RoutedEventArgs e)
        {
            
            btnStokSil_Click(sender, e);
        }

        private void btnYenile_Click(object sender, RoutedEventArgs e)
        {
            UrunleriGetir();
            StokListele();
        }

      
    }
}