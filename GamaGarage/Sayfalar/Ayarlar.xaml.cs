using GamaGarage.mesagekutu;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GamaGarage.Sayfalar
{
    public partial class Ayarlar : UserControl
    {
        string baglantiYolu = App.BaglantiCumlesi;
        string secilenFotoYolu = "";
        string _aktifYetki = ""; 
        bool _secilenKullaniciDurum = true;

        public Ayarlar(string yetki)
        {

          
            InitializeComponent();


            GamaGarage.Veriler.TemaYardimcisi.TemaRenkleriniYukle(this.AnaStop1, this.AnaStop3);
            _aktifYetki = yetki; 

            KullanicilariDoldur();
            YetkiListesiniDoldur();
            YetkiKontroluYap();
        }

        private void YetkiKontroluYap()
        {
            if (FindName("cardRenkAyarlari") is FrameworkElement cardRenk)
                cardRenk.Visibility = Visibility.Visible;
            if (_aktifYetki == "Yönetici")
            {
                cardKullaniciYonetimi.Visibility = Visibility.Visible;
            }
            else
            {
                cardKullaniciYonetimi.Visibility = Visibility.Collapsed;
            }
        }

        private void YetkiListesiniDoldur()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(baglantiYolu))
                {
                    string sorgu = "SELECT DISTINCT YETKI_SEVIYESI FROM TBL_KULLANICILAR WHERE YETKI_SEVIYESI IS NOT NULL";
                    SqlDataAdapter da = new SqlDataAdapter(sorgu, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbYetki.ItemsSource = dt.DefaultView;
                    cmbYetki.DisplayMemberPath = "YETKI_SEVIYESI";
                    cmbYetki.SelectedValuePath = "YETKI_SEVIYESI";
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Yetki listesi hatası: " + ex.Message, "HATA"); }
        }

        private void KullanicilariDoldur()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(baglantiYolu))
                {
                    string sorgu = @"SELECT ID, 
                                     CASE WHEN DURUM = 1 THEN KAD ELSE KAD + ' (PASİF)' END AS KAD 
                                     FROM TBL_KULLANICILAR";

                    SqlDataAdapter da = new SqlDataAdapter(sorgu, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbKullaniciSec.ItemsSource = dt.DefaultView;
                    cmbKullaniciSec.DisplayMemberPath = "KAD";
                    cmbKullaniciSec.SelectedValuePath = "ID";
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Liste hatası: " + ex.Message, "HATA"); }
        }

        private void CmbKullaniciSec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbKullaniciSec.SelectedValue == null) return;

            using (SqlConnection con = new SqlConnection(baglantiYolu))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM TBL_KULLANICILAR WHERE ID=@id", con);
                cmd.Parameters.AddWithValue("@id", cmbKullaniciSec.SelectedValue);
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    txtDuzenleKadi.Text = dr["KAD"].ToString();
                    txtDuzenleSifre.Password = dr["SIFRE"].ToString();
                    cmbYetki.SelectedValue = dr["YETKI_SEVIYESI"].ToString();

                    _secilenKullaniciDurum = Convert.ToBoolean(dr["DURUM"]);

                    if (_secilenKullaniciDurum == false)
                    {
                        btnKullaniciSil.Content = "KULLANICIYI AKTİF ET";
                        btnKullaniciSil.Background = new SolidColorBrush(Color.FromRgb(46, 139, 87));
                    }
                    else
                    {
                        btnKullaniciSil.Content = "KULLANICIYI PASİF YAP";
                        btnKullaniciSil.Background = new SolidColorBrush(Color.FromRgb(205, 92, 92));
                    }

                    if (dr["FOTOGRAF"] != DBNull.Value)
                    {
                        byte[] data = (byte[])dr["FOTOGRAF"];
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit(); bi.CacheOption = BitmapCacheOption.OnLoad; bi.StreamSource = ms; bi.EndInit();
                            imgKullaniciFoto.ImageSource = bi;
                        }
                    }
                    else
                    {
                        imgKullaniciFoto.ImageSource = null;
                    }
                }
            }
        }

        private void imgKullaniciFoto_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Resim Dosyaları (*.jpg;*.png)|*.jpg;*.png" };
            if (ofd.ShowDialog() == true)
            {
                secilenFotoYolu = ofd.FileName;
                imgKullaniciFoto.ImageSource = new BitmapImage(new Uri(secilenFotoYolu));
            }
        }

        private void btnSifreGoster_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            txtSifreAcik.Visibility = Visibility.Visible;
            txtDuzenleSifre.Visibility = Visibility.Collapsed;
            txtSifreAcik.Text = txtDuzenleSifre.Password;
        }

        private void btnSifreGoster_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            txtSifreAcik.Visibility = Visibility.Collapsed;
            txtDuzenleSifre.Visibility = Visibility.Visible;
        }

        private void btnRenkUygula_Click(object sender, RoutedEventArgs e)
        {
            var anaPencere = Window.GetWindow(this) as MainWindow;
            if (anaPencere != null)
            {
                if (anaPencere.AnaStop1 != null) anaPencere.AnaStop1.Color = cpColor1.Color;
                if (anaPencere.AnaStop3 != null) anaPencere.AnaStop3.Color = cpColor3.Color;
                if (this.AnaStop1 != null) this.AnaStop1.Color = cpColor1.Color;
                if (this.AnaStop3 != null) this.AnaStop3.Color = cpColor3.Color;
                Properties.Settings.Default.TemaRenk1 = System.Drawing.Color.FromArgb(cpColor1.Color.A, cpColor1.Color.R, cpColor1.Color.G, cpColor1.Color.B);
                Properties.Settings.Default.TemaRenk2 = System.Drawing.Color.FromArgb(cpColor3.Color.A, cpColor3.Color.R, cpColor3.Color.G, cpColor3.Color.B);
                Properties.Settings.Default.Save();

                GamaMesaj.Tamam("Renk teması kaydedildi ve tüm ekranlarda güncellendi!", "GÖRÜNÜM");
            }
        }
        private void BtnOrijinalRenk_Click(object sender, RoutedEventArgs e)
        {
            cpColor1.Color = Colors.OrangeRed;
            cpColor3.Color = Colors.SteelBlue;
            btnRenkUygula_Click(null!, null!);
        }

        private void btnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (cmbKullaniciSec.SelectedValue == null)
            {
                GamaMesaj.Tamam("Lütfen düzenlenecek kullanıcıyı seçin!", "EKSİK SEÇİM");
                return;
            }

            AdminOnayPenceresi onay = new AdminOnayPenceresi();
            onay.Owner = Window.GetWindow(this);

            if (onay.ShowDialog() == true)
            {
                if (onay.GelenYetki == "Yönetici")
                {
                    Anamenuler.ButonYetkiPenceresi yetkiPenceresi = new Anamenuler.ButonYetkiPenceresi();
                    yetkiPenceresi.Owner = Window.GetWindow(this);

                    if (yetkiPenceresi.ShowDialog() == true)
                    {
                        GuncellemeIsleminiYap(yetkiPenceresi);
                    }
                }
                else
                {
                    GamaMesaj.Tamam("Sadece yöneticiler güncelleyebilir.", "YETKİSİZ ERİŞİM");
                }
            }
        }

        private void GuncellemeIsleminiYap(Anamenuler.ButonYetkiPenceresi yetkiPenceresi)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(baglantiYolu))
                {
                    string sorgu = @"UPDATE TBL_KULLANICILAR SET 
                        KAD=@k, SIFRE=@s, YETKI_SEVIYESI=@sev, DURUM=1,
                        YETKI_PERSONEL=@y1, YETKI_MUSTERI=@y2, YETKI_STOK=@y3, YETKI_KASA=@y4,
                        YETKI_CEKSENET=@y5, YETKI_RANDEVU=@y6, YETKI_MUHASEBE=@y7, 
                        YETKI_URUNLER=@y8, YETKI_FIRMALAR=@y9, YETKI_GRAFIKLER=@y10";

                    if (!string.IsNullOrEmpty(secilenFotoYolu)) sorgu += ", FOTOGRAF=@foto";
                    sorgu += " WHERE ID=@id";

                    SqlCommand cmd = new SqlCommand(sorgu, con);
                    cmd.Parameters.AddWithValue("@k", txtDuzenleKadi.Text);
                    cmd.Parameters.AddWithValue("@s", txtDuzenleSifre.Password);
                    cmd.Parameters.AddWithValue("@sev", cmbYetki.Text);
                    cmd.Parameters.AddWithValue("@id", cmbKullaniciSec.SelectedValue);
                    cmd.Parameters.AddWithValue("@y1", yetkiPenceresi.chkPersonel.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y2", yetkiPenceresi.chkMusteri.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y3", yetkiPenceresi.chkStok.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y4", yetkiPenceresi.chkKasa.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y5", yetkiPenceresi.chkCekSenet.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y6", yetkiPenceresi.chkRandevu.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y7", yetkiPenceresi.chkMuhasebe.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y8", yetkiPenceresi.chkUrunler.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y9", yetkiPenceresi.chkFirmalar.IsChecked == false);
                    cmd.Parameters.AddWithValue("@y10", yetkiPenceresi.chkGrafikler.IsChecked == false);

                    if (!string.IsNullOrEmpty(secilenFotoYolu))
                        cmd.Parameters.AddWithValue("@foto", File.ReadAllBytes(secilenFotoYolu));

                    con.Open();
                    cmd.ExecuteNonQuery();

                    GamaMesaj.Tamam("Kullanıcı bilgileri güncellendi!", "BAŞARILI");
                    KullanicilariDoldur();
                    YetkiListesiniDoldur();
                }
            }
            catch (Exception ex) { GamaMesaj.Tamam("Hata: " + ex.Message, "SİSTEM HATASI"); }
        }

        private void btnKullaniciSil_Click(object sender, RoutedEventArgs e)
        {
            if (cmbKullaniciSec.SelectedValue == null)
            {
                GamaMesaj.Tamam("Lütfen bir kullanıcı seçin!", "EKSİK SEÇİM");
                return;
            }

            AdminOnayPenceresi onay = new AdminOnayPenceresi();
            onay.Owner = Window.GetWindow(this);

            if (onay.ShowDialog() == true)
            {
                if (onay.GelenYetki == "Yönetici")
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(baglantiYolu))
                        {
                            con.Open();
                            int yeniDurum = _secilenKullaniciDurum ? 0 : 1;
                            string query = "UPDATE TBL_KULLANICILAR SET DURUM = @durum WHERE ID = @id";

                            SqlCommand cmd = new SqlCommand(query, con);
                            cmd.Parameters.AddWithValue("@durum", yeniDurum);
                            cmd.Parameters.AddWithValue("@id", cmbKullaniciSec.SelectedValue);

                            cmd.ExecuteNonQuery();

                            string mesaj = yeniDurum == 1 ? "Kullanıcı AKTİF edildi." : "Kullanıcı PASİF edildi.";
                            GamaMesaj.Tamam(mesaj, "BAŞARILI");

                            txtDuzenleKadi.Clear();
                            txtDuzenleSifre.Clear();
                            imgKullaniciFoto.ImageSource = null;
                            KullanicilariDoldur();
                        }
                    }
                    catch (Exception ex) { GamaMesaj.Tamam("İşlem hatası: " + ex.Message, "HATA"); }
                }
            }
        }
    }
}