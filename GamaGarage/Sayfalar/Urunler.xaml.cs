using GamaGarage.Veriler;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using GamaGarage.mesagekutu; 

namespace GamaGarage.Sayfalar
{
    public partial class Urunler : UserControl
    {
        int seciliUrunID = 0;

        public Urunler()
        {
            InitializeComponent();
            UrunListele();
           
            VeriIslemleri.ComboBoxFirmaDoldur(cmbUFirma);
        }

      

        private void UrunListele()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                   
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Urunler ORDER BY UrunID DESC", baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgUrun.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Ürün listesi yüklenemedi: " + ex.Message, "HATA");
            }
        }

     

        private void btnUrunler_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUrunAd.Text.Trim()))
            {
                GamaMesaj.Tamam("Lütfen ürün adını boş bırakmayınız!", "EKSİK BİLGİ");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"INSERT INTO Urunler (UrunAd, Kategori, Marka, StokAdedi, Fiyat, KayitTarihi, FirmaID, Aciklama) 
                                    VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", txtUrunAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtKatagori.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", txtUrunMarka.Text.Trim());
                    komut.Parameters.AddWithValue("@p4", string.IsNullOrEmpty(txtStokadedi.Text) ? 0 : int.Parse(txtStokadedi.Text));
                    komut.Parameters.Add("@p5", SqlDbType.Decimal).Value = DecimalCevir(txtBirimFiyati.Text);
                    komut.Parameters.AddWithValue("@p6", dpUrunTarihi.SelectedDate ?? DateTime.Now);
                    komut.Parameters.AddWithValue("@p7", cmbUFirma.SelectedValue ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p8", txtUAciklama.Text.Trim());

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Yeni ürün başarıyla sisteme kaydedildi.", "BAŞARILI");

                    UrunListele();
                    Temizle();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ekleme Hatası: " + ex.Message, "HATA"); }
        }

       

        private void btnUrunGuncelle_Click_1(object sender, RoutedEventArgs e)
        {
            if (seciliUrunID == 0)
            {
                GamaMesaj.Tamam("Lütfen güncellemek istediğiniz ürünü listeden seçin!", "UYARI");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"UPDATE Urunler SET UrunAd=@p1, Kategori=@p2, Marka=@p3, StokAdedi=@p4, 
                                    Fiyat=@p5, KayitTarihi=@p6, FirmaID=@p7, Aciklama=@p8 WHERE UrunID=@id";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", txtUrunAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtKatagori.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", txtUrunMarka.Text.Trim());
                    komut.Parameters.AddWithValue("@p4", string.IsNullOrEmpty(txtStokadedi.Text) ? 0 : int.Parse(txtStokadedi.Text));
                    komut.Parameters.Add("@p5", SqlDbType.Decimal).Value = DecimalCevir(txtBirimFiyati.Text);
                    komut.Parameters.AddWithValue("@p6", dpUrunTarihi.SelectedDate ?? DateTime.Now);
                    komut.Parameters.AddWithValue("@p7", cmbUFirma.SelectedValue ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p8", txtUAciklama.Text.Trim());
                    komut.Parameters.AddWithValue("@id", seciliUrunID);

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("Ürün bilgileri başarıyla güncellendi.", "BAŞARILI");
                    UrunListele();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Güncelleme Hatası: " + ex.Message, "HATA"); }
        }

      
        private void btnUrunSil_Click(object sender, RoutedEventArgs e)
        {
            if (seciliUrunID == 0)
            {
                GamaMesaj.Tamam("Silmek istediğiniz ürünü seçmediniz!", "UYARI");
                return;
            }

            if (GamaMesaj.Onay("Seçilen ürünü silmek istediğinize emin misiniz?", "SİLME ONAYI"))
            {
                try
                {
                    using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                    {
                        baglanti.Open();
                        SqlCommand komut = new SqlCommand("DELETE FROM Urunler WHERE UrunID=@id", baglanti);
                        komut.Parameters.AddWithValue("@id", seciliUrunID);
                        komut.ExecuteNonQuery();

                        GamaMesaj.Tamam("Ürün sistemden silindi.", "BİLGİ");
                        UrunListele();
                        Temizle();
                    }
                }
                catch (Exception ex) { GamaMesaj.Tamam("Silme Hatası: " + ex.Message, "HATA"); }
            }
        }

      

        private void txtUrunAra_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string ara = txtUrunAra.Text.Trim();
                    string sorgu = "SELECT * FROM Urunler WHERE UrunAd LIKE @ara OR Marka LIKE @ara OR Kategori LIKE @ara";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@ara", "%" + ara + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgUrun.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void btnFiltrele_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM Urunler WHERE 1=1";
                    if (dpUBaslangic.SelectedDate != null && dpUBitis.SelectedDate != null)
                    {
                        sorgu += " AND KayitTarihi BETWEEN @t1 AND @t2";
                    }

                    sorgu += " ORDER BY KayitTarihi DESC";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        if (dpUBaslangic.SelectedDate != null && dpUBitis.SelectedDate != null)
                        {
                            komut.Parameters.Add("@t1", SqlDbType.Date).Value = dpUBaslangic.SelectedDate.Value.Date;

                            komut.Parameters.Add("@t2", SqlDbType.Date).Value = dpUBitis.SelectedDate.Value.Date;
                        }

                        SqlDataAdapter da = new SqlDataAdapter(komut);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgUrun.ItemsSource = null;
                        dgUrun.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Ürün Filtreleme Hatası: " + ex.Message, "HATA"); }
        }



        private void dgUrun_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUrun.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliUrunID = Convert.ToInt32(satir["UrunID"]);
                    txtUrunAd.Text = satir["UrunAd"]?.ToString();
                    txtKatagori.Text = satir["Kategori"]?.ToString();
                    txtUrunMarka.Text = satir["Marka"]?.ToString();
                    txtStokadedi.Text = satir["StokAdedi"]?.ToString();
                    txtBirimFiyati.Text = satir["Fiyat"]?.ToString();
                    cmbUFirma.SelectedValue = satir["FirmaID"];
                    txtUAciklama.Text = satir["Aciklama"]?.ToString();

                    if (satir["KayitTarihi"] != DBNull.Value)
                        dpUrunTarihi.SelectedDate = Convert.ToDateTime(satir["KayitTarihi"]);
                }
                catch { }
            }
        }

        private void Temizle()
        {
            seciliUrunID = 0;
            txtUrunAd.Clear();
            txtKatagori.Clear();
            txtUrunMarka.Clear();
            txtStokadedi.Clear();
            txtBirimFiyati.Clear();
            txtUAciklama.Clear();
            cmbUFirma.SelectedIndex = -1;
            dpUrunTarihi.SelectedDate = null;
            dgUrun.SelectedIndex = -1;
        }

        private decimal DecimalCevir(string metin)
        {
            if (string.IsNullOrEmpty(metin)) return 0;
           
            decimal.TryParse(metin.Replace(".", ","), out decimal sonuc);
            return sonuc;
        }

        private void dgUrun_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgUrun.Columns)
            {
                if (col.Header == null) continue;
                string header = col.Header.ToString()!;

                
                if (header == "UrunID" || header == "FirmaID")
                {
                    col.Visibility = Visibility.Collapsed;
                }

              
                switch (header)
                {
                    case "UrunAd": col.Header = "Ürün Adı"; break;
                    case "Kategori": col.Header = "Kategori"; break; 
                    case "Marka": col.Header = "Marka"; break;
                    case "StokAdedi": col.Header = "Stok Miktarı"; break;
                    case "Fiyat": col.Header = "Birim Fiyat (₺)"; break;
                    case "KayitTarihi": col.Header = "Kayıt Tarihi"; break;
                    case "Aciklama": col.Header = "Açıklama"; break;
                    case "FirmaAdi": col.Header = "Tedarikçi Firma"; break; 
                }

                if (header == "KayitTarihi" || header == "Fiyat")
                {
                    var textColumn = col as System.Windows.Controls.DataGridTextColumn;
                    if (textColumn != null && textColumn.Binding is System.Windows.Data.Binding binding)
                    {
                        if (header == "Fiyat")
                            binding.StringFormat = "N2"; 
                        else
                            binding.StringFormat = "dd.MM.yyyy"; 
                    }
                }
            }
        }

        private void btnUrunTemizle_Click(object sender, RoutedEventArgs e) => Temizle();

      
    }
}