using GamaGarage.mesagekutu;
using GamaGarage.Sayfalar;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace GamaGarage
{
    public partial class MainWindow : Window
    {
  
        private string yeniVadeDetay = "";
        private int yeniVadeSayisi = 0;
        private List<string> suAnkiBildirimImzalari = new List<string>();

        public MainWindow()
        {
         

            InitializeComponent();
            GamaGarage.Veriler.TemaYardimcisi.TemaRenkleriniYukle(this.AnaStop1, this.AnaStop3);

            BadgeVade.Visibility = Visibility.Collapsed;
            YetkileriKontrolEt();
            this.ContentRendered += MainWindow_ContentRendered!;
            
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
           
            DispatcherTimer vadeTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            vadeTimer.Tick += (s, ev) => VadeHatirlaticiKontrolEt();
            vadeTimer.Start();
            DispatcherTimer clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            clockTimer.Tick += (s, ev) => Guncelle();
            clockTimer.Start();

            Guncelle();
            VadeHatirlaticiKontrolEt();
        }



   


        void Guncelle()
        {
            txtSaat.Text = DateTime.Now.ToString("HH:mm:ss");
            txtTarih.Text = DateTime.Now.ToString("dd MMMM yyyy dddd");
        }

        private async void VadeHatirlaticiKontrolEt()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                    {
                        baglanti.Open();
                        string sorgu = @"SELECT MuhatapCariAd, Tutar, EvrakTuru FROM CekSenetIslemleri 
                                WHERE (CAST(VadeTarihi AS DATE) = CAST(@bugun AS DATE) OR CAST(VadeTarihi AS DATE) = CAST(@dun AS DATE))
                                AND (MuhatapCariAd + '_' + FORMAT(Tutar, 'N2', 'tr-TR') + '_' + EvrakTuru) 
                                NOT IN (SELECT EVRAK_IMZA FROM TBL_BILDIRIM_KAYITLARI)";

                        using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                        {
                            komut.Parameters.AddWithValue("@bugun", DateTime.Now.Date);
                            komut.Parameters.AddWithValue("@dun", DateTime.Now.Date.AddDays(-1));

                            using (SqlDataReader dr = komut.ExecuteReader())
                            {
                                List<string> listeGorsel = new List<string>();
                                List<string> listeImza = new List<string>();
                                int sayac = 0;

                                while (dr.Read())
                                {
                                    string cari = dr["MuhatapCariAd"].ToString()!;                     
                                    string tutar = Convert.ToDecimal(dr["Tutar"]).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
                                    string tur = dr["EvrakTuru"].ToString()!;

                                    listeGorsel.Add($"• {cari} ({tur}) - {tutar} TL");
                                    listeImza.Add($"{cari}_{tutar}_{tur}");
                                    sayac++;
                                }

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    yeniVadeSayisi = sayac;
                                    if (yeniVadeSayisi > 0)
                                    {
                                        yeniVadeDetay = string.Join("\n", listeGorsel);
                                        suAnkiBildirimImzalari = listeImza;
                                        BadgeVade.Badge = yeniVadeSayisi;
                                        BadgeVade.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        BadgeVade.Visibility = Visibility.Collapsed;
                                        suAnkiBildirimImzalari.Clear();
                                    }
                                });
                            }
                        }
                    }
                });
            }
            catch { }
        }

        private void BtnVadeBildirim_Click(object sender, RoutedEventArgs e)
        {
            if (yeniVadeSayisi > 0)
            {
                GamaMesaj.Tamam($"{yeniVadeSayisi} adet YENİ evrak bildirimi:\n\n{yeniVadeDetay}", "VADE TAKİBİ");

                try
                {
                    using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                    {
                        baglanti.Open();
                        foreach (string imza in suAnkiBildirimImzalari)
                        {
                      
                            string sorgu = "INSERT INTO TBL_BILDIRIM_KAYITLARI (EVRAK_IMZA) VALUES (@imza)";

                            using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                            {
                                komut.Parameters.AddWithValue("@imza", imza);
                                try { komut.ExecuteNonQuery(); } catch { }
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }

                BadgeVade.Visibility = Visibility.Collapsed;
                yeniVadeSayisi = 0;
                suAnkiBildirimImzalari.Clear();
            }
        }

        private void SayfaYonlendir(UserControl yeniSayfa)
        {
            if (SayfaIcerigi.Content != null && SayfaIcerigi.Content.GetType() == yeniSayfa.GetType())
                SayfaIcerigi.Content = null;
            else
                SayfaIcerigi.Content = yeniSayfa;
        }

        private void YetkileriKontrolEt()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = "SELECT * FROM TBL_KULLANICILAR WHERE KAD = @kadi";
                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@kadi", App.GirisYapanKullanici);
                    baglanti.Open();
                    SqlDataReader dr = komut.ExecuteReader();
                    if (dr.Read())
                    {
                        txtAdSoyad.Text = dr["KAD"].ToString()?.ToUpper() ?? "KULLANICI";
                        if (dr["FOTOGRAF"] != DBNull.Value)
                        {
                            byte[] imgData = (byte[])dr["FOTOGRAF"];
                            using (MemoryStream ms = new MemoryStream(imgData))
                            {
                                BitmapImage bi = new BitmapImage();
                                bi.BeginInit(); bi.CacheOption = BitmapCacheOption.OnLoad;
                                bi.StreamSource = ms; bi.EndInit();
                                imgGFoto.ImageSource = bi;
                            }
                        }
                        btnAyarlar.Visibility = Visibility.Visible;
                        BtnPersonel.Visibility = GetBool(dr, "YETKI_PERSONEL") ? Visibility.Visible : Visibility.Collapsed;
                        BtnMusteri.Visibility = GetBool(dr, "YETKI_MUSTERI") ? Visibility.Visible : Visibility.Collapsed;
                        BtnStok.Visibility = GetBool(dr, "YETKI_STOK") ? Visibility.Visible : Visibility.Collapsed;
                        BtnKasa.Visibility = GetBool(dr, "YETKI_KASA") ? Visibility.Visible : Visibility.Collapsed;
                        BtnCekSenet.Visibility = GetBool(dr, "YETKI_CEKSENET") ? Visibility.Visible : Visibility.Collapsed;
                        BtnRandevu.Visibility = GetBool(dr, "YETKI_RANDEVU") ? Visibility.Visible : Visibility.Collapsed;
                        BtnMuhasebe.Visibility = GetBool(dr, "YETKI_MUHASEBE") ? Visibility.Visible : Visibility.Collapsed;
                        BtnUrunler.Visibility = GetBool(dr, "YETKI_URUNLER") ? Visibility.Visible : Visibility.Collapsed;
                        BtnFirmalar.Visibility = GetBool(dr, "YETKI_FIRMALAR") ? Visibility.Visible : Visibility.Collapsed;
                        BtnGrafikler.Visibility = GetBool(dr, "YETKI_GRAFIKLER") ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Yetki yükleme hatası: " + ex.Message); }
        }
        private void BtnPersonel_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.PersonelSayfasi());
        private void BtnMusteri_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.MusteriSayfasi());
        private void BtnStok_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.Stok());
        private void BtnKasa_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.Kasa());
        private void BtnCekSenet_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.CekSenet());
        private void BtnRandevu_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.Randevu());
        private void BtnMuhasebe_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.Muhasebe());
        private void BtnUrunler_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.Urunler());
        private void BtnFirmalar_Click(object sender, RoutedEventArgs e) => SayfaYonlendir(new Sayfalar.Firmalar());
        private void BtnGrafikler_Click(object sender, RoutedEventArgs e)
        {
            if (SayfaIcerigi.Content is Sayfalar.GrafiklerSayfasi) { SayfaIcerigi.Content = null; }
            else { var gs = new Sayfalar.GrafiklerSayfasi(); SayfaIcerigi.Content = gs; gs.VerileriYukle(); }
        }
        private void BtnAnaMenu_Click(object sender, RoutedEventArgs e) { new Anamenuler.Anamenu().Show(); this.Close(); }
        private void BtnKapat_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void BtnAyarlar_Click(object sender, RoutedEventArgs e)
        {
            AdminOnayPenceresi onay = new AdminOnayPenceresi { Owner = this };
            if (onay.ShowDialog() == true) { SayfaIcerigi.Content = new Sayfalar.Ayarlar(onay.GelenYetki!); }
        }
        private bool GetBool(SqlDataReader dr, string kolon)
        {
            try { int i = dr.GetOrdinal(kolon); return dr.IsDBNull(i) ? false : Convert.ToBoolean(dr.GetValue(i)); }
            catch { return false; }
        }
    }
}