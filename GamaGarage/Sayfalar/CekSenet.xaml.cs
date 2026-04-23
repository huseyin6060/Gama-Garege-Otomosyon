using GamaGarage.mesagekutu;
using GamaGarage.Veriler;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GamaGarage.Sayfalar
{
    public partial class CekSenet : UserControl
    {
        int seciliCekID = 0;

        public CekSenet()
        {
            InitializeComponent();
            CekSenetListele();


            VeriIslemleri.ComboBoxFirmaDoldur(cmbCFirmaTuru);
        }


        private void CekSenetListele()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {

                    string sorgu = @"SELECT C.*, F.FirmaAd 
                 FROM CekSenetIslemleri C
                 LEFT JOIN Firmalar F ON C.Firma = F.FirmaID 
                 ORDER BY C.VadeTarihi ASC";

                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgCekSenet.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Liste yüklenirken bir hata oluştu: " + ex.Message, "LİSTE HATASI");
            }
        }


        private void dgCekSenet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCekSenet.SelectedItem is DataRowView satir)
            {
                try
                {
                    seciliCekID = Convert.ToInt32(satir["EvrakID"]);
                    txtAciklama.Text = satir["Aciklama"].ToString();
                    txtCari.Text = satir["MuhatapCariAd"].ToString();
                    txtSeriNo.Text = satir["SeriNo"].ToString();
                    txtTutar.Text = satir["Tutar"].ToString();
                    cmbEvrakTuru.Text = satir["EvrakTuru"].ToString();
                    cmbCFirmaTuru.Text = satir["Firma"].ToString();

                    if (satir["VerisTarihi"] != DBNull.Value)
                        dpDuzenleme.SelectedDate = Convert.ToDateTime(satir["VerisTarihi"]);
                    if (satir["VadeTarihi"] != DBNull.Value)
                        dpVade.SelectedDate = Convert.ToDateTime(satir["VadeTarihi"]);

                    string yon = satir["IslemYonu"].ToString()!;
                    rbGiris.IsChecked = (yon == "Giriş");
                    rbCikis.IsChecked = (yon == "Çıkış");
                }
                catch (Exception ex)
                {
                    GamaMesaj.Tamam("Seçim sırasında hata: " + ex.Message, "SEÇİM HATASI");
                }
            }
        }

     
        private void btnYeni_Click(object sender, RoutedEventArgs e)
        {
            if (!BilgilerDogruMu()) return;

            CekSenetIslemi("INSERT INTO CekSenetIslemleri (EvrakTuru, SeriNo, VerisTarihi, VadeTarihi, Tutar, MuhatapCariAd, Firma, Aciklama, IslemYonu) VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9)");
        }

        private void btnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (seciliCekID == 0) { GamaMesaj.Tamam("Lütfen güncellenecek kaydı seçin!", "SEÇİM YAP"); return; }
            if (!BilgilerDogruMu()) return;

            CekSenetIslemi(@"UPDATE CekSenetIslemleri SET EvrakTuru=@p1, SeriNo=@p2, VerisTarihi=@p3, VadeTarihi=@p4, 
                            Tutar=@p5, MuhatapCariAd=@p6, Firma=@p7, Aciklama=@p8, IslemYonu=@p9 WHERE EvrakID=@id", true);
        }

        private void btnSil_Click(object sender, RoutedEventArgs e)
        {
            if (seciliCekID == 0) { GamaMesaj.Tamam("Silinecek kaydı seç!", "SEÇİM EKSİK"); return; }

            if (GamaMesaj.Onay("Bu kaydı ve buna bağlı ödeme bilgisini silmek istediğine emin misin?", "SİLME ONAYI"))
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    try
                    {
                        baglanti.Open();
                       
                        SqlTransaction islem = baglanti.BeginTransaction();

                        try
                        {
                            
                            string silOdemeSorgu = "DELETE FROM FirmaOdemeleri WHERE EvrakTuru = @id";
                            using (SqlCommand cmdOdeme = new SqlCommand(silOdemeSorgu, baglanti, islem))
                            {
                                cmdOdeme.Parameters.AddWithValue("@id", seciliCekID.ToString());
                                cmdOdeme.ExecuteNonQuery();
                            }

                          
                            string silCekSorgu = "DELETE FROM CekSenetIslemleri WHERE EvrakID = @id";
                            using (SqlCommand cmdCek = new SqlCommand(silCekSorgu, baglanti, islem))
                            {
                                cmdCek.Parameters.AddWithValue("@id", seciliCekID);
                                cmdCek.ExecuteNonQuery();
                            }

                           
                            islem.Commit();

                            GamaMesaj.Tamam("Kayıt ve bağlı ödeme başarıyla silindi.", "BİLGİ");
                            CekSenetListele();
                            Temizle();
                        }
                        catch (Exception ex)
                        {
                            islem.Rollback();
                            throw new Exception("Silme işlemi sırasında bir hata oluştu: " + ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        GamaMesaj.Tamam("Silme Hatası: " + ex.Message, "HATA");
                    }
                }
            }
        }

        private void CekSenetIslemi(string sorgu, bool isUpdate = false)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    baglanti.Open();
                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@p1", cmbEvrakTuru.Text ?? "");
                    komut.Parameters.AddWithValue("@p2", txtSeriNo.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", (object)dpDuzenleme.SelectedDate! ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p4", (object)dpVade.SelectedDate! ?? DBNull.Value);

                    
                    decimal tutar;
                    decimal.TryParse(txtTutar.Text.Replace(".", ","), out tutar);
                    komut.Parameters.AddWithValue("@p5", tutar);

                    komut.Parameters.AddWithValue("@p6", txtCari.Text.Trim());
                    komut.Parameters.AddWithValue("@p7", cmbCFirmaTuru.SelectedValue ?? DBNull.Value);
                    komut.Parameters.AddWithValue("@p8", txtAciklama.Text.Trim());
                    komut.Parameters.AddWithValue("@p9", rbGiris.IsChecked == true ? "Giriş" : "Çıkış");

                    if (isUpdate) komut.Parameters.AddWithValue("@id", seciliCekID);

                    komut.ExecuteNonQuery();
                    GamaMesaj.Tamam("İşlem başarıyla tamamlandı.", "BAŞARILI");
                    CekSenetListele();
                    Temizle();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Hata: " + ex.Message, "İŞLEM HATASI"); }
        }

     
        private void txtEvrakAra_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string arama = txtEvrakAra.Text.Trim();
                    string sorgu = "SELECT * FROM CekSenetIslemleri WHERE MuhatapCariAD LIKE @search OR SeriNo LIKE @search";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@search", "%" + arama + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgCekSenet.ItemsSource = dt.DefaultView;
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
                    
                    string sorgu = "SELECT * FROM CekSenetIslemleri WHERE 1=1";
                    if (dpBaslangic.SelectedDate != null)
                    {
                        sorgu += " AND VerisTarihi >= @t1";
                    }
                    if (dpBitis.SelectedDate != null)
                    {
                        sorgu += " AND VadeTarihi <= @t2";
                    }

                    sorgu += " ORDER BY VadeTarihi DESC";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        if (dpBaslangic.SelectedDate != null)
                        {
                            komut.Parameters.Add("@t1", SqlDbType.Date).Value = dpBaslangic.SelectedDate.Value.Date;
                        }

                        if (dpBitis.SelectedDate != null)
                        {
                           
                            komut.Parameters.Add("@t2", SqlDbType.Date).Value = dpBitis.SelectedDate.Value.Date;
                        }

                        SqlDataAdapter da = new SqlDataAdapter(komut);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgCekSenet.ItemsSource = null;
                        dgCekSenet.ItemsSource = dt.DefaultView;

                     
                        if (dt.Rows.Count == 0 && (dpBaslangic.SelectedDate != null || dpBitis.SelectedDate != null))
                        {
                            GamaMesaj.Tamam("Seçtiğiniz kriterlere uygun kayıt bulunamadı.", "SONUÇ YOK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Filtreleme Hatası: " + ex.Message, "HATA");
            }
        }

        private bool BilgilerDogruMu()
        {
            if (string.IsNullOrWhiteSpace(txtSeriNo.Text)) { GamaMesaj.Tamam("Lütfen Seri No girin!", "EKSİK BİLGİ"); return false; }
            if (string.IsNullOrWhiteSpace(txtTutar.Text)) { GamaMesaj.Tamam("Lütfen Tutar girin!", "EKSİK BİLGİ"); return false; }
            return true;
        }

        private void Temizle()
        {
            seciliCekID = 0;
            txtAciklama.Clear(); txtCari.Clear(); txtSeriNo.Clear(); txtTutar.Clear();
            dpDuzenleme.SelectedDate = null; dpVade.SelectedDate = null;
            cmbCFirmaTuru.SelectedIndex = -1; cmbEvrakTuru.SelectedIndex = -1;
            rbGiris.IsChecked = true;
        }

        private void dgCekSenet_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgCekSenet.Columns)
            {
                string header = col.Header?.ToString() ?? "";

              
                if (header == "EvrakID" || header == "Firma" || header == "Aciklama")
                {
                    col.Visibility = Visibility.Collapsed;
                    continue;
                }

              
                switch (header)
                {
                    case "EvrakTuru": col.Header = "Evrak Türü"; break;
                    case "SeriNo": col.Header = "Seri / Belge No"; break;
                    case "VerisTarihi": col.Header = "Düzenleme Tarihi"; break;
                    case "VadeTarihi": col.Header = "Vade Tarihi"; break;
                    case "Tutar": col.Header = "Tutar (₺)"; break;
                    case "MuhatapCariAd": col.Header = "Asıl Borçlu / Muhatap"; break;
                    case "FirmaAd": col.Header = "İlgili Firma"; break;
                    case "IslemYonu": col.Header = "Yön"; break;
                }

               
                if (col is DataGridTextColumn textCol)
                {
                    var binding = textCol.Binding as System.Windows.Data.Binding;
                    if (binding == null) continue;

                   
                    if (header.Contains("Tarihi"))
                    {
                        binding.StringFormat = "dd.MM.yyyy";
                    }
                
                    else if (header == "Tutar")
                    {
                        binding.StringFormat = "N2";
                       
                        Style sagStyle = new Style(typeof(DataGridCell));
                        sagStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                        textCol.CellStyle = sagStyle;
                    }
                }
            }
        }

        private void btnYenile_Click(object sender, RoutedEventArgs e)
        {
            CekSenetListele();
            VeriIslemleri.ComboBoxFirmaDoldur(cmbCFirmaTuru);
        }

       

        private void btntemizle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtEvrakAra.Clear();
                dpBaslangic.SelectedDate = null;
                dpBitis.SelectedDate = null;

                Temizle();
                CekSenetListele();
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Temizleme sırasında bir hata oluştu: " + ex.Message, "HATA");
            }
        }

        
    }
}