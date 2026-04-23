using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using GamaGarage.mesagekutu;

namespace GamaGarage.Sayfalar
{
    public partial class Randevu : UserControl
    {
        Button seciliSlot = null!;
        DispatcherTimer otomatizeKontrol;
        const int KATEGORI_SINIRI = 12;

        public Randevu()
        {
            InitializeComponent();
            VarsayilanSlotlariYukle();
            PersonelYukle();
            RandevuListesiniGuncelle();


            otomatizeKontrol = new DispatcherTimer();
            otomatizeKontrol.Interval = TimeSpan.FromMinutes(1);
            otomatizeKontrol.Tick += (s, e) => SlotDurumlariniGuncelle();
            otomatizeKontrol.Start();
        }


        private void VarsayilanSlotlariYukle()
        {
            wpLiftler.Children.Clear();
            wpCukurlar.Children.Clear();
            wpGarajlar.Children.Clear();

            try
            {
                using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
                {
                    bag.Open();

                    SqlCommand cmd = new SqlCommand("SELECT SlotNo, Tip FROM Slotlar", bag);
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        string sNo = dr["SlotNo"].ToString()!;
                        string tip = dr["Tip"].ToString()!;

                        // Veritabanındaki tipe göre ilgili panele ekle
                        if (tip == "LIFT") SlotOlustur(wpLiftler, PackIconKind.Forklift, sNo);
                        else if (tip == "CUKUR") SlotOlustur(wpCukurlar, PackIconKind.Plus, sNo);
                        else if (tip == "GARAJ") SlotOlustur(wpGarajlar, PackIconKind.Garage, sNo);
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Slot Yükleme Hatası: " + ex.Message, "HATA"); }

            SlotDurumlariniGuncelle();
        }

        private void SlotOlustur(WrapPanel panel, PackIconKind ikon, string slotNo)
        {
            if (panel.Children.Count >= KATEGORI_SINIRI)
            {
                GamaMesaj.Tamam($"Bu bölüme maksimum {KATEGORI_SINIRI} slot ekleyebilirsiniz!", "SINIR AŞILDI");
                return;
            }

            Button btn = new Button
            {
                Name = slotNo,
                Width = 55,
                Height = 55,
                Margin = new Thickness(5),
                Background = Brushes.GreenYellow,
                BorderThickness = new Thickness(0),
                Tag = slotNo,
                Content = new PackIcon { Kind = ikon, Width = 30, Height = 30, Foreground = Brushes.Black }
            };

            btn.Click += (s, e) => {

                seciliSlot = btn;
                SlotDurumlariniGuncelle();

                IslemKaydiEkle(slotNo + " alanı seçildi.");
            };

            panel.Children.Add(btn);
        }



        private void btnRandevuEkle_Click(object sender, RoutedEventArgs e)
        {
            if (seciliSlot == null)
            {
                GamaMesaj.Tamam("Lütfen bir alan seçin!", "SEÇİM YOK");
                return;
            }

            string slotID = seciliSlot.Tag.ToString()!;
            DateTime bas = dpRandevuBaslangıc.SelectedDate ?? DateTime.Now;
            DateTime bit = dpRandevuBitis.SelectedDate ?? bas.AddHours(2);

            using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
            {
                bag.Open();

             
                string kontrolSorgu = @"SELECT COUNT(*) FROM Randevular 
                                WHERE SlotNo = @sno 
                                AND ((@bas >= BaslangicTarih AND @bas < BitisTarih) OR 
                                     (@bit > BaslangicTarih AND @bit <= BitisTarih) OR
                                     (BaslangicTarih >= @bas AND BitisTarih <= @bit))";

                SqlCommand cmdKontrol = new SqlCommand(kontrolSorgu, bag);
                cmdKontrol.Parameters.AddWithValue("@sno", slotID);
                cmdKontrol.Parameters.AddWithValue("@bas", bas);
                cmdKontrol.Parameters.AddWithValue("@bit", bit);

                if ((int)cmdKontrol.ExecuteScalar() > 0)
                {
                    GamaMesaj.Tamam("Seçtiğiniz alan bu saatler arasında dolu!", "DOLU");
                    return;
                }

       
                SoruEkrani soruPenceresi = new SoruEkrani();
                if (soruPenceresi.ShowDialog() != true) return;
                string tip = soruPenceresi.Secim?.Trim()!;
                if (string.IsNullOrWhiteSpace(tip) || tip.ToUpper() == "VAZGEÇ") return;

           
                string sorgu = @"INSERT INTO Randevular (Ad, Soyad, Plaka, Telefon, BaslangicTarih, BitisTarih, PersonelID, SlotNo, IslemTipi, Tutar, Aciklama) 
                        VALUES (@ad, @soyad, @plaka, @tel, @bas, @bit, @pid, @sno, @tip, @tutar, @aciklama)";

                SqlCommand cmd = new SqlCommand(sorgu, bag);
                cmd.Parameters.AddWithValue("@ad", txtRAd.Text.Trim());
                cmd.Parameters.AddWithValue("@soyad", txtRSoyad.Text.Trim());
                cmd.Parameters.AddWithValue("@plaka", Rplaka.Text.Trim().ToUpper());
                cmd.Parameters.AddWithValue("@tel", txtRTelefon.Text.Trim());
                cmd.Parameters.AddWithValue("@bas", bas);
                cmd.Parameters.AddWithValue("@bit", bit);
                cmd.Parameters.AddWithValue("@pid", cmbRPersonel.SelectedValue ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@sno", slotID);
                cmd.Parameters.AddWithValue("@tip", tip);
                
                decimal.TryParse(txtRTutar.Text, out decimal tutarDeger);
                cmd.Parameters.AddWithValue("@tutar", tutarDeger);
                cmd.Parameters.AddWithValue("@aciklama", txtRAciklama.Text.Trim());

                cmd.ExecuteNonQuery();

                seciliSlot = null!;
                RandevuListesiniGuncelle();
                SlotDurumlariniGuncelle();
                GamaMesaj.Tamam("Randevu başarıyla eklendi.", "BAŞARILI");
                btnRTemizle_Click(null!, null!);
            }
        }

        private void SlotDurumlariniGuncelle()
        {
            try
            {
                using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
                {
                    bag.Open();
                 
                    string sorgu = @"SELECT SlotNo, IslemTipi FROM Randevular 
                             WHERE GETDATE() BETWEEN BaslangicTarih AND BitisTarih";

                    SqlDataAdapter da = new SqlDataAdapter(sorgu, bag);
                    DataTable dtActive = new DataTable();
                    da.Fill(dtActive);

                    Panel[] paneller = { wpLiftler, wpCukurlar, wpGarajlar };
                    foreach (var p in paneller)
                    {
                        foreach (Control item in p.Children)
                        {
                            if (item is Button btn && btn.Tag != null)
                            {
                                string btnTag = btn.Tag.ToString()!;

                                
                                btn.Background = Brushes.GreenYellow;

                              
                                foreach (DataRow satir in dtActive.Rows)
                                {
                                    if (satir["SlotNo"].ToString() == btnTag)
                                    {
                                        string durum = satir["IslemTipi"].ToString()!.ToUpper();
                                        if (durum == "ŞİMDİ" || durum == "DOLU") btn.Background = Brushes.Red;
                                        else if (durum == "REZERVE") btn.Background = Brushes.Orange;
                                        break;
                                    }
                                }

                             
                                if (seciliSlot != null && seciliSlot == btn)
                                    btn.Background = Brushes.SteelBlue;
                            }
                        }
                    }
                }
            }
            catch {  }
        }

        private void btnRandevuSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgRandevuListesi.SelectedItem is DataRowView satir)
            {
                if (GamaMesaj.Onay("Bu randevu kaydını kalıcı olarak silmek istiyor musunuz?", "SİLME ONAYI"))
                {
                    try
                    {
                        using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
                        {
                            SqlCommand cmd = new SqlCommand("DELETE FROM Randevular WHERE ID=@id", bag);
                            cmd.Parameters.AddWithValue("@id", satir["ID"]);
                            bag.Open();
                            cmd.ExecuteNonQuery();
                        }
                        RandevuListesiniGuncelle();
                        SlotDurumlariniGuncelle();
                        GamaMesaj.Tamam("Randevu silindi.", "BİLGİ");
                    }
                    catch (Exception ex) { GamaMesaj.Tamam("Silme Hatası: " + ex.Message, "HATA"); }
                }
            }
        }
        private void RandevuListesiniGuncelle()
        {
            try
            {
                using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
                {
                   
                    string sorgu = @"SELECT r.*, (p.Ad + ' ' + p.Soyad) as PersonelIsim 
                             FROM Randevular r 
                             LEFT JOIN Personeller p ON r.PersonelID = p.ID 
                             ORDER BY r.ID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(sorgu, bag);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgRandevuListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ) {  }
        }

        private void PersonelYukle()
        {
            try
            {
                using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT ID, (Ad + ' ' + Soyad) as Isim FROM dbo.Personeller WHERE Durum=1", bag);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbRPersonel.ItemsSource = dt.DefaultView;
                    cmbRPersonel.DisplayMemberPath = "Isim";
                    cmbRPersonel.SelectedValuePath = "ID";
                }
            }
            catch { }
        }



        private void btnFiltreleGenel_Click(object sender, RoutedEventArgs e)
        {
            if (dpBaslangicGenel.SelectedDate == null || dpBitisGenel.SelectedDate == null)
            {
                GamaMesaj.Tamam("Lütfen tarih aralığını tam seçin!", "UYARI");
                return;
            }

            try
            {
                using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM Randevular WHERE BaslangicTarih >= @bas AND BitisTarih <= @bit ORDER BY ID DESC";
                    SqlCommand cmd = new SqlCommand(sorgu, bag);
                    cmd.Parameters.AddWithValue("@bas", dpBaslangicGenel.SelectedDate.Value);
                    cmd.Parameters.AddWithValue("@bit", dpBitisGenel.SelectedDate.Value.AddDays(1).AddSeconds(-1));

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgRandevuListesi.ItemsSource = dt.DefaultView;

                    if (dt.Rows.Count == 0) GamaMesaj.Tamam("Belirtilen tarihlerde kayıt bulunamadı.", "BİLGİ");
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Filtreleme Hatası: " + ex.Message, "HATA"); }
        }




        private void btnRandevuGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgRandevuListesi.SelectedItem is DataRowView satir)
            {
                if (seciliSlot == null)
                {
                    GamaMesaj.Tamam("Lütfen bir masa seçin!", "UYARI");
                    return;
                }


                string eskiSlotID = satir["SlotNo"].ToString()!;
                string yeniSlotID = seciliSlot.Tag.ToString()!;
                int randevuID = Convert.ToInt32(satir["ID"]);

                using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
                {
                    bag.Open();

                    if (eskiSlotID != yeniSlotID)
                    {

                        string kontrolSorgu = "SELECT COUNT(*) FROM Randevular WHERE SlotNo = @sno AND BitisTarih >= GETDATE() AND ID != @rid";
                        SqlCommand cmdKontrol = new SqlCommand(kontrolSorgu, bag);
                        cmdKontrol.Parameters.AddWithValue("@sno", yeniSlotID);
                        cmdKontrol.Parameters.AddWithValue("@rid", randevuID);
                        int doluluk = (int)cmdKontrol.ExecuteScalar();

                        if (doluluk > 0)
                        {
                            GamaMesaj.Tamam("Seçtiğiniz yeni masa şu an dolu! Lütfen başka bir yer seçin.", "MASA DOLU");
                            return;
                        }
                    }

                    SoruEkrani soruPenceresi = new SoruEkrani();
                    if (soruPenceresi.ShowDialog() == true)
                    {
                        string secilenTip = soruPenceresi.Secim;
                        if (string.IsNullOrEmpty(secilenTip) || secilenTip == "Vazgeç") return;
                        string sorgu = @"UPDATE Randevular 
                                 SET Ad=@ad, Soyad=@soyad, Plaka=@plaka, Telefon=@tel, 
                                     IslemTipi=@tip, SlotNo=@sno 
                                 WHERE ID=@id";

                        SqlCommand cmd = new SqlCommand(sorgu, bag);
                        cmd.Parameters.AddWithValue("@ad", txtRAd.Text.Trim());
                        cmd.Parameters.AddWithValue("@soyad", txtRSoyad.Text.Trim());
                        cmd.Parameters.AddWithValue("@plaka", Rplaka.Text.Trim().ToUpper());
                        cmd.Parameters.AddWithValue("@tel", txtRTelefon.Text.Trim());
                        cmd.Parameters.AddWithValue("@tip", secilenTip);
                        cmd.Parameters.AddWithValue("@sno", yeniSlotID);
                        cmd.Parameters.AddWithValue("@id", randevuID);

                        cmd.ExecuteNonQuery();

                        seciliSlot = null!;
                        RandevuListesiniGuncelle();
                        SlotDurumlariniGuncelle();
                        GamaMesaj.Tamam("Randevu ve Masa başarıyla güncellendi.", "BİLGİ");
                    }
                }
            }
        }

        private void txtRandevuAra_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM Randevular WHERE Ad LIKE @p OR Soyad LIKE @p OR Plaka LIKE @p ORDER BY ID DESC";
                    SqlCommand cmd = new SqlCommand(sorgu, bag);
                    cmd.Parameters.AddWithValue("@p", "%" + txtRandevuAra.Text + "%");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgRandevuListesi.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }


        private void btnGCLSil_Click(object sender, RoutedEventArgs e)
        {
            if (seciliSlot == null)
            {
                GamaMesaj.Tamam("Lütfen önce silmek istediğiniz slotu seçin (Mavi olmalı).", "SEÇİM YOK");
                return;
            }

            if (GamaMesaj.Onay($"{seciliSlot.Tag} slotunu görselden kaldırmak istiyor musunuz?", "ONAY"))
            {
                if (wpLiftler.Children.Contains(seciliSlot)) wpLiftler.Children.Remove(seciliSlot);
                else if (wpCukurlar.Children.Contains(seciliSlot)) wpCukurlar.Children.Remove(seciliSlot);
                else if (wpGarajlar.Children.Contains(seciliSlot)) wpGarajlar.Children.Remove(seciliSlot);

                IslemKaydiEkle($"{seciliSlot.Tag} görselden kaldırıldı.");
                seciliSlot = null!;
            }
        }


        private void SlotuVeritabaninaKaydet(string ad, string tip)
        {
            using (SqlConnection bag = new SqlConnection(App.BaglantiCumlesi))
            {
                bag.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Slotlar (SlotNo, Tip) VALUES (@ad, @tip)", bag);
                cmd.Parameters.AddWithValue("@ad", ad);
                cmd.Parameters.AddWithValue("@tip", tip);
                cmd.ExecuteNonQuery();
            }
        }


        private void btnLift_Click(object sender, RoutedEventArgs e)
        {
            string yeniAd = "Lift_" + Guid.NewGuid().ToString().Substring(0, 4);
            SlotuVeritabaninaKaydet(yeniAd, "LIFT");
            VarsayilanSlotlariYukle();
        }

        private void btnCukur_Click(object sender, RoutedEventArgs e)
        {
            string yeniAd = "Cukur_" + Guid.NewGuid().ToString().Substring(0, 4);
            SlotuVeritabaninaKaydet(yeniAd, "CUKUR");
            VarsayilanSlotlariYukle();
        }

        private void btnGaraj_Click(object sender, RoutedEventArgs e)
        {
            string yeniAd = "Garaj_" + Guid.NewGuid().ToString().Substring(0, 4);
            SlotuVeritabaninaKaydet(yeniAd, "GARAJ");
            VarsayilanSlotlariYukle();
        }


        private void btnRTemizle_Click(object sender, RoutedEventArgs e)
        {
            txtRAd.Clear(); txtRSoyad.Clear(); Rplaka.Clear(); txtRTelefon.Clear();
            cmbRPersonel.SelectedIndex = -1;
            dpRandevuBaslangıc.SelectedDate = null;
            dpRandevuBitis.SelectedDate = null;
            seciliSlot = null!;
            SlotDurumlariniGuncelle();
        }

        private void IslemKaydiEkle(string m)
        {

            spIslemKayitlari.Children.Clear();
            spIslemKayitlari.Children.Add(new TextBlock
            {
                Text = $"• {DateTime.Now:HH:mm} - {m}",
                Margin = new Thickness(0, 2, 0, 2),
                Foreground = Brushes.DimGray
            });
        }

        private void dgRandevuListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRandevuListesi.SelectedItem is DataRowView satir)
            {
                try
                {
                  
                    txtRAd.Text = satir["Ad"]?.ToString();
                    txtRSoyad.Text = satir["Soyad"]?.ToString();
                    Rplaka.Text = satir["Plaka"]?.ToString();
                    txtRTelefon.Text = satir["Telefon"]?.ToString();
                    txtRTutar.Text = satir["Tutar"]?.ToString();
                    txtRAciklama.Text = satir["Aciklama"]?.ToString();

               
                    if (satir["PersonelID"] != DBNull.Value)
                        cmbRPersonel.SelectedValue = satir["PersonelID"];
                    else
                        cmbRPersonel.SelectedIndex = -1;

                    if (satir["BaslangicTarih"] != DBNull.Value)
                        dpRandevuBaslangıc.SelectedDate = Convert.ToDateTime(satir["BaslangicTarih"]);

                    if (satir["BitisTarih"] != DBNull.Value)
                        dpRandevuBitis.SelectedDate = Convert.ToDateTime(satir["BitisTarih"]);

                   
                    string mevcutSlotNo = satir["SlotNo"]?.ToString()!;
                    seciliSlot = null!; 

                    Panel[] paneller = { wpLiftler, wpCukurlar, wpGarajlar };
                    foreach (var p in paneller)
                    {
                        foreach (Control item in p.Children)
                        {
                            if (item is Button btn && btn.Tag != null && btn.Tag.ToString() == mevcutSlotNo)
                            {
                                seciliSlot = btn;
                                break;
                            }
                        }
                        if (seciliSlot != null) break;
                    }

                    
                    SlotDurumlariniGuncelle();
                }
                catch (Exception ex)
                {
                   
                    GamaMesaj.Tamam("Bilgiler doldurulurken hata: " + ex.Message, "HATA");
                }
            }
        }

        private void dgRandevuListesi_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in dgRandevuListesi.Columns)
            {
                if (col.Header == null) continue;
                string header = col.Header.ToString()!;

        
                if (header == "ID" || header == "PersonelID" || header == "SlotID")
                {
                    col.Visibility = Visibility.Collapsed;
                }

              
                switch (header)
                {
                    case "Ad": col.Header = "Müşteri Adı"; break;
                    case "Soyad": col.Header = "Müşteri Soyadı"; break;
                    case "Plaka": col.Header = "Araç Plakası"; break;
                    case "Telefon": col.Header = "İletişim No"; break;
                    case "BaslangicTarih": col.Header = "Başlangıç Zamanı"; break;
                    case "BitisTarih": col.Header = "Bitiş Zamanı"; break;
                    case "IslemTipi": col.Header = "İşlem Durumu"; break;
                    case "SlotNo": col.Header = "Alan / Masa"; break;
                    case "Tutar": col.Header = "Ücret (₺)"; break;
                    case "Aciklama": col.Header = "Notlar"; break;
                    case "PersonelIsim": col.Header = "Görevli Personel"; break; 
                }

           
                if (header == "BaslangicTarih" || header == "BitisTarih" || header == "Tutar")
                {
                    var textColumn = col as System.Windows.Controls.DataGridTextColumn;
                    if (textColumn != null && textColumn.Binding is System.Windows.Data.Binding binding)
                    {
                        if (header == "Tutar") binding.StringFormat = "N2";
                        else binding.StringFormat = "dd.MM.yyyy HH:mm";
                    }
                }
            }
        }

        private void btnYenile_Click(object sender, RoutedEventArgs e)
        {
            RandevuListesiniGuncelle();
            SlotDurumlariniGuncelle();
        }


    }
}