using GamaGarage.mesagekutu;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace GamaGarage.Anamenuler
{
    public partial class KayitEkrani : UserControl
    {
        string baglantiYolu = App.BaglantiCumlesi;
        string secilenFotoYolu = "";

        public KayitEkrani()
        {
            InitializeComponent();
            if (dpKayitTarih != null) dpKayitTarih.SelectedDate = DateTime.Now;
            YetkiListesiniGuncelle();
        }

      
        private void YetkiListesiniGuncelle()
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(baglantiYolu))
                {
                    string sorgu = "SELECT DISTINCT YETKI_SEVIYESI FROM TBL_KULLANICILAR WHERE YETKI_SEVIYESI IS NOT NULL";
                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    baglanti.Open();
                    SqlDataReader dr = komut.ExecuteReader();

                    cmbYetki.Items.Clear();
                    while (dr.Read())
                    {
                        cmbYetki.Items.Add(dr["YETKI_SEVIYESI"].ToString());
                    }
                    dr.Close();
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void imgKayıtFoto_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları (*.jpg;*.png)|*.jpg;*.png";
            if (ofd.ShowDialog() == true)
            {
                secilenFotoYolu = ofd.FileName;
                imgKayıtFoto.ImageSource = new BitmapImage(new Uri(secilenFotoYolu));
            }
        }

        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtKayitKadi.Text) || string.IsNullOrEmpty(txtKayitSifre.Password) || string.IsNullOrEmpty(cmbYetki.Text))
            {
                GamaMesaj.Tamam("Kullanıcı adı, şifre ve yetki seviyesi boş olamaz!", "EKSİK BİLGİ");
                return;
            }

            AdminOnayPenceresi onay = new AdminOnayPenceresi();
            onay.Owner = Window.GetWindow(this);

            if (onay.ShowDialog() == true)
            {
                if (onay.GelenYetki == "Yönetici")
                {
                    ButonYetkiPenceresi yetkiPenceresi = new ButonYetkiPenceresi();
                    yetkiPenceresi.Owner = Window.GetWindow(this);

                    if (yetkiPenceresi.ShowDialog() == true)
                    {
                        try
                        {
                            using (SqlConnection baglanti = new SqlConnection(baglantiYolu))
                            {
                                string sorgu = @"INSERT INTO TBL_KULLANICILAR 
                                       (KAD, SIFRE, TARIH, YETKI_SEVIYESI, YETKI_PERSONEL, YETKI_MUSTERI, 
                                        YETKI_STOK, YETKI_KASA, YETKI_CEKSENET, YETKI_RANDEVU, 
                                        YETKI_MUHASEBE, YETKI_URUNLER, YETKI_FIRMALAR, YETKI_GRAFIKLER, 
                                        YETKI_YEDEK, FOTOGRAF, DURUM, YETKI_KAYIT) 
                                       VALUES 
                                       (@kadi, @sifre, @tarih, @seviye, @yPer, @yMus, @yStok, @yKasa, @yCek, @yRan, 
                                        @yMuh, @yUrun, @yFir, @yGraf, @yYedek, @foto, 1, 1)";

                                SqlCommand komut = new SqlCommand(sorgu, baglanti);
                                komut.Parameters.AddWithValue("@kadi", txtKayitKadi.Text);
                                komut.Parameters.AddWithValue("@sifre", txtKayitSifre.Password);
                                komut.Parameters.AddWithValue("@tarih", dpKayitTarih.SelectedDate ?? DateTime.Now);
                                komut.Parameters.AddWithValue("@seviye", cmbYetki.Text);
                                komut.Parameters.AddWithValue("@yPer", yetkiPenceresi.chkPersonel.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yMus", yetkiPenceresi.chkMusteri.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yStok", yetkiPenceresi.chkStok.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yKasa", yetkiPenceresi.chkKasa.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yCek", yetkiPenceresi.chkCekSenet.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yRan", yetkiPenceresi.chkRandevu.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yMuh", yetkiPenceresi.chkMuhasebe.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yUrun", yetkiPenceresi.chkUrunler.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yFir", yetkiPenceresi.chkFirmalar.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yGraf", yetkiPenceresi.chkGrafikler.IsChecked ?? false);
                                komut.Parameters.AddWithValue("@yYedek", true);

                                if (!string.IsNullOrEmpty(secilenFotoYolu) && File.Exists(secilenFotoYolu))
                                    komut.Parameters.AddWithValue("@foto", File.ReadAllBytes(secilenFotoYolu));
                                else
                                    komut.Parameters.Add("@foto", System.Data.SqlDbType.VarBinary).Value = DBNull.Value;

                                baglanti.Open();
                                komut.ExecuteNonQuery();

                                GamaMesaj.Tamam($"Kullanıcı '{cmbYetki.Text}' yetkisiyle kaydedildi!", "BAŞARILI");
                                YetkiListesiniGuncelle();
                                Temizle();
                            }
                        }
                        catch (Exception ex)
                        {
                            GamaMesaj.Tamam("Veritabanı Hatası: " + ex.Message, "SİSTEM HATASI");
                        }
                    }
                }
                else
                {
                    GamaMesaj.Tamam("Yetkisiz işlem!", "HATA");
                }
            }
        }

        private void Temizle()
        {
            txtKayitKadi.Clear();
            txtKayitSifre.Clear();
            imgKayıtFoto.ImageSource = null;
            secilenFotoYolu = "";
            if (cmbYetki != null)
            {
                cmbYetki.SelectedIndex = -1;
                cmbYetki.Text = "";
            }
        }

        private void btnSifreGoster_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtSifreAcik.Text = txtKayitSifre.Password;
            txtKayitSifre.Visibility = Visibility.Collapsed;
            txtSifreAcik.Visibility = Visibility.Visible;
        }

        private void btnSifreGoster_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtSifreAcik.Visibility = Visibility.Collapsed;
            txtKayitSifre.Visibility = Visibility.Visible;
            txtKayitSifre.Focus();
        }
    }
}