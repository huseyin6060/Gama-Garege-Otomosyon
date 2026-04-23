using GamaGarage.mesagekutu;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace GamaGarage.Sayfalar
{
    public partial class Kasa : UserControl
    {
        public Kasa()
        {
            InitializeComponent();
            KasaListele();
        }

        private void KasaListele()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                   
                    string sql = "SELECT IslemID, Tarih, Tutar, IslemTipi, Aciklama FROM View_KasaGenelRapor ORDER BY Tarih DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sql, baglanti);
                    DataTable dtHepsi = new DataTable();
                    da.Fill(dtHepsi);

               
                    DataView dvGelir = new DataView(dtHepsi);
                    dvGelir.RowFilter = "IslemTipi = 'GELİR'";
                    dgKasaGelirListesi.ItemsSource = dvGelir;

                    DataView dvGider = new DataView(dtHepsi);
                    dvGider.RowFilter = "IslemTipi = 'GİDER'";
                    dgKasaGiderListesi.ItemsSource = dvGider;

                   
                    decimal tGelir = 0;
                    decimal tGider = 0;
                    foreach (DataRow satir in dtHepsi.Rows)
                    {
                        decimal tutar = Convert.ToDecimal(satir["Tutar"]);
                        if (satir["IslemTipi"].ToString() == "GELİR") tGelir += tutar;
                        else tGider += tutar;
                    }

                    txtToplamGelir.Text = tGelir.ToString("N2") + " ₺";
                    txtToplamGider.Text = tGider.ToString("N2") + " ₺";
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Kasa listeleme hatası: " + ex.Message, "KASA HATASI"); }
        }

        private void KasaKaydet(string tip, string tutar, string aciklama)
        {
            if (string.IsNullOrEmpty(tutar))
            {
                GamaMesaj.Tamam("Lütfen tutar giriniz!", "EKSİK BİLGİ");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    string sorgu = @"INSERT INTO KasaIslemleri (IslemTipi, Tutar, Tarih, Aciklama) 
                                    VALUES (@tip, @tutar, @tarih, @aciklama)";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@tip", tip);
                    komut.Parameters.Add("@tutar", SqlDbType.Decimal).Value = DecimalCevir(tutar);
                    komut.Parameters.AddWithValue("@tarih", DateTime.Now);
                    komut.Parameters.AddWithValue("@aciklama", aciklama);

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam(tip + " kaydı başarıyla eklendi.", "BAŞARILI");
                    KasaListele();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Kayıt Hatası: " + ex.Message, "HATA"); }
        }

        private void btnGelirGirisi_Click(object sender, RoutedEventArgs e) => KasaKaydet("GELİR", txtTutar.Text, txtKAciklama.Text);
        private void btnGiderGirisi_Click(object sender, RoutedEventArgs e) => KasaKaydet("GİDER", txtGiderTutar.Text, txtAciklama.Text);

        private decimal DecimalCevir(string metin)
        {
            if (string.IsNullOrEmpty(metin)) return 0;
            decimal.TryParse(metin.Replace(".", ","), out decimal sonuc);
            return sonuc;
        }

        private void SilmeIslemi(int id)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                  
                    SqlCommand komut = new SqlCommand("DELETE FROM KasaIslemleri WHERE IslemID=@id", baglanti);
                    komut.Parameters.AddWithValue("@id", id);
                    int etkilenen = komut.ExecuteNonQuery();

                    if (etkilenen > 0) GamaMesaj.Tamam("İşlem silindi.", "BİLGİ");
                    else GamaMesaj.Tamam("Bu işlem otomatik bir kayıttır (Maaş, Satış vb.), buradan silinemez.", "UYARI");

                    KasaListele();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Silme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnGelirSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKasaGelirListesi.SelectedItem is DataRowView satir)
                if (GamaMesaj.Onay("Seçili kaydı silmek istiyor musunuz?", "SİLME ONAYI"))
                    SilmeIslemi(Convert.ToInt32(satir["IslemID"]));
        }

        private void btnGiderSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKasaGiderListesi.SelectedItem is DataRowView satir)
                if (GamaMesaj.Onay("Seçili kaydı silmek istiyor musunuz?", "SİLME ONAYI"))
                    SilmeIslemi(Convert.ToInt32(satir["IslemID"]));
        }

        private void dgKasaGelirListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgKasaGelirListesi.SelectedItem is DataRowView satir)
            {
                txtTutar.Text = satir["Tutar"].ToString();
                txtKAciklama.Text = satir["Aciklama"].ToString();

               
                if (satir["Tarih"] != DBNull.Value)
                {
                    dpKOdemeTarih.SelectedDate = Convert.ToDateTime(satir["Tarih"]);
                }
            }
        }

        private void dgKasaGiderListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgKasaGiderListesi.SelectedItem is DataRowView satir)
            {
                txtGiderTutar.Text = satir["Tutar"].ToString();
                txtAciklama.Text = satir["Aciklama"].ToString();

          
                if (satir["Tarih"] != DBNull.Value)
                {
                    dpGiderOdemeTarih.SelectedDate = Convert.ToDateTime(satir["Tarih"]);
                }
            }
        }


        private void dgKasaGelirListesi_AutoGeneratedColumns(object sender, EventArgs e)
        {
            if (sender is DataGrid dg) GridDuzenle(dg);
        }

        private void dgKasaGiderListesi_AutoGeneratedColumns(object sender, EventArgs e)
        {
            if (sender is DataGrid dg) GridDuzenle(dg);
        }

        private void GridDuzenle(DataGrid dg)
        {
            if (dg == null) return;

            foreach (var col in dg.Columns)
            {
                if (col.Header == null) continue;
                string header = col.Header.ToString()!;
                if (header == "IslemID" || header == "IslemTipi")
                {
                    col.Visibility = Visibility.Collapsed;
                    continue;
                }

                switch (header)
                {
                    case "Tarih":
                        col.Header = "İşlem Tarihi";
                        col.DisplayIndex = 0; 
                        col.Width = new DataGridLength(130);
                        break;
                    case "Tutar":
                        col.Header = "Tutar (₺)";
                        col.DisplayIndex = 1;
                        col.Width = new DataGridLength(110);
                        break;
                    case "Aciklama":
                        col.Header = "Açıklama / Detay";
                        col.DisplayIndex = 2;
                        col.Width = new DataGridLength(1, DataGridLengthUnitType.Star); 
                        break;
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


        private void txtKasaAra_TextChanged1(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string ara = txtKasaAra1.Text.Trim();
                   
                    string sorgu = "SELECT * FROM View_KasaGenelRapor WHERE IslemTipi='GELİR' AND Aciklama LIKE @ara ORDER BY Tarih DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@ara", "%" + ara + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgKasaGelirListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

      
        private void txtKasaAra_TextChanged2(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string ara = txtKasaAra2.Text.Trim();
                   
                    string sorgu = "SELECT * FROM View_KasaGenelRapor WHERE IslemTipi='GİDER' AND Aciklama LIKE @ara ORDER BY Tarih DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@ara", "%" + ara + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgKasaGiderListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

     
        private void btnFiltrele_Click1(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM View_KasaGenelRapor WHERE IslemTipi='GELİR'";
                    if (dpKDuzenleme1.SelectedDate != null && dpKBDuzenleme1.SelectedDate != null)
                    {
                        sorgu += " AND Tarih BETWEEN @t1 AND @t2";
                    }
                    sorgu += " ORDER BY Tarih DESC";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        if (dpKDuzenleme1.SelectedDate != null && dpKBDuzenleme1.SelectedDate != null)
                        {
                            komut.Parameters.Add("@t1", SqlDbType.Date).Value = dpKDuzenleme1.SelectedDate.Value.Date;
                            komut.Parameters.Add("@t2", SqlDbType.Date).Value = dpKBDuzenleme1.SelectedDate.Value.Date;
                        }
                        SqlDataAdapter da = new SqlDataAdapter(komut);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgKasaGelirListesi.ItemsSource = null;
                        dgKasaGelirListesi.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Gelir Filtreleme Hatası: " + ex.Message, "HATA"); }
        }

        
        private void btnFiltrele_Click2(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM View_KasaGenelRapor WHERE IslemTipi='GİDER'";
                    if (dpKDuzenleme2.SelectedDate != null && dpKBDuzenleme2.SelectedDate != null)
                    {
                        sorgu += " AND Tarih BETWEEN @t1 AND @t2";
                    }
                    sorgu += " ORDER BY Tarih DESC";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        if (dpKDuzenleme2.SelectedDate != null && dpKBDuzenleme2.SelectedDate != null)
                        {
                            komut.Parameters.Add("@t1", SqlDbType.Date).Value = dpKDuzenleme2.SelectedDate.Value.Date;
                            komut.Parameters.Add("@t2", SqlDbType.Date).Value = dpKBDuzenleme2.SelectedDate.Value.Date;
                        }
                        SqlDataAdapter da = new SqlDataAdapter(komut);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgKasaGiderListesi.ItemsSource = null;
                        dgKasaGiderListesi.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Gider Filtreleme Hatası: " + ex.Message, "HATA"); }
        }

        private void btnYenile_Click(object sender, RoutedEventArgs e)
        {
            KasaListele();
        }

      
    }
}