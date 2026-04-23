using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using GamaGarage.mesagekutu;
namespace GamaGarage.Sayfalar
{
    public partial class Detay : UserControl
    {
      
        int seciliFirmaID = 0;

        public Detay()
        {
            InitializeComponent();
            FirmalariListele();
        }

        private void FirmalariListele()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                   
                    string sorgu = "SELECT FirmaID, FirmaAd, FirmaYetkilisi, Telefon, FirmaKayitTarihi FROM Firmalar ORDER BY FirmaAd ASC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgDetay.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Firmalar listelenirken hata oluştu: " + ex.Message);
            }
        }
        private void FirmaDetaylariniGetir()
        {
            if (seciliFirmaID == 0) return;

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = @"
            -- Ürünler Tablosu
            SELECT 
                KayitTarihi AS Tarih,  
                UrunAd AS Islem,
                'ALINAN ÜRÜN' AS Tur, 
                Fiyat AS Tutar 
            FROM Urunler 
            WHERE FirmaID = @firmaID

            UNION ALL 

            -- Çek ve Senetler Tablosu
            SELECT 
                VadeTarihi AS Tarih, 
                EvrakTuru + ' (Seri: ' + ISNULL(SeriNo, '') + ')' AS Islem, 
                'ÇEK-SENET' AS Tur, 
                Tutar AS Tutar 
            FROM CekSenetIslemleri 
            WHERE Firma = @firmaID

            UNION ALL 

            -- Nakit/Banka Ödemeleri
            SELECT 
                IslemTarihi AS Tarih, 
                -- Hata Buradaydı: EvrakTuru INT olduğu için önce metne çeviriyoruz (CAST)
                CASE 
                    WHEN EvrakTuru IS NULL THEN 'Nakit' 
                    ELSE 'Evrak No: ' + CAST(EvrakTuru AS VARCHAR(10)) 
                END + ' Ödemesi' AS Islem, 
                'NAKİT ÖDEME' AS Tur, 
                Odenen AS Tutar 
            FROM FirmaOdemeleri 
            WHERE FirmaID = @firmaID

            ORDER BY Tarih DESC";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.Add("@firmaID", SqlDbType.Int).Value = seciliFirmaID;

                    SqlDataAdapter da = new SqlDataAdapter(komut);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgGelir.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Detaylar listelenirken hata oluştu: " + ex.Message);
            }
        }
        private void dgDetay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgDetay.SelectedItem is DataRowView satir)
            {
                
                seciliFirmaID = Convert.ToInt32(satir["FirmaID"]);
                FirmaDetaylariniGetir();
            }
        }

        private void txtdetayAra_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgDetay.ItemsSource == null) return;
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT FirmaID, FirmaAd, FirmaYetkilisi, Telefon, FirmaKayitTarihi FROM Firmalar WHERE FirmaAd LIKE @p1 OR FirmaYetkilisi LIKE @p1";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@p1", "%" + txtdetayAra.Text + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgDetay.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void txtDetayAra2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgGelir.ItemsSource == null) return;
            DataView dv = (DataView)dgGelir.ItemsSource;
            dv.RowFilter = string.Format("Islem LIKE '%{0}%' OR Tur LIKE '%{0}%'", txtDetayAra2.Text);
        }

        private void btnDetayFiltre_Click(object sender, RoutedEventArgs e)
        {
            DateTime baslangic = dpDetayBaslangic.SelectedDate ?? DateTime.MinValue;
            DateTime bitis = dpDetayBitis.SelectedDate ?? DateTime.MaxValue;

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM Firmalar WHERE FirmaKayitTarihi BETWEEN @d1 AND @d2";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@d1", baslangic);
                    da.SelectCommand.Parameters.AddWithValue("@d2", bitis);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgDetay.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void btnDetayFiltre2_Click(object sender, RoutedEventArgs e)
        {
            if (seciliFirmaID == 0 || dgGelir.ItemsSource == null) return;

            DateTime baslangic = dpDetayBaslangic2.SelectedDate ?? DateTime.MinValue;
            DateTime bitis = dpGelirBitis2.SelectedDate ?? DateTime.MaxValue;

            DataView dv = (DataView)dgGelir.ItemsSource;
            dv.RowFilter = string.Format("Tarih >= #{0:MM/dd/yyyy}# AND Tarih <= #{1:MM/dd/yyyy}#", baslangic, bitis);
        }
        private void dgDetay_AutoGeneratedColumns(object sender, EventArgs e)
        {
            if (!(sender is DataGrid dg)) return;

            foreach (var col in dg.Columns)
            {
                string header = col.Header?.ToString() ?? "";

                
                if (header.Contains("ID"))
                {
                    col.Visibility = Visibility.Collapsed;
                    continue;
                }

                
                switch (header)
                {
                    case "FirmaAd": col.Header = "Firma Adı"; break;
                    case "FirmaYetkilisi": col.Header = "Yetkili"; break;
                    case "Telefon": col.Header = "Telefon"; break;
                    case "FirmaKayitTarihi": col.Header = "Kayıt Tarihi"; break;
                }

                
                if (header == "FirmaKayitTarihi" && col is DataGridTextColumn dateCol)
                {
                    (dateCol.Binding as System.Windows.Data.Binding)!.StringFormat = "dd.MM.yyyy";
                }
            }
        }


        private void dgKontrol_AutoGeneratedColumns(object sender, EventArgs e)
        {
            if (!(sender is DataGrid dg)) return;

            foreach (var col in dg.Columns)
            {
                string header = col.Header?.ToString() ?? "";

              
                switch (header)
                {
                    case "Tarih": col.Header = "İşlem Tarihi"; break;
                    case "Islem": col.Header = "Yapılan İşlem / Detay"; break;
                    case "Tur": col.Header = "İşlem Türü"; break;
                    case "Tutar": col.Header = "Tutar (₺)"; break;
                }

             
                if (col is DataGridTextColumn textCol && textCol.Binding is System.Windows.Data.Binding binding)
                {
                    if (header == "Tarih")
                        binding.StringFormat = "dd.MM.yyyy HH:mm";
                    else if (header == "Tutar")
                        binding.StringFormat = "N2"; 
                }
            }
        }

        private void btnKapat_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl parentKonteynir)
            {
                parentKonteynir.Content = new Firmalar();
            }
        }
    }
}