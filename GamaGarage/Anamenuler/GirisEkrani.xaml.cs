using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using GamaGarage.mesagekutu; 

namespace GamaGarage.Anamenuler
{
    public partial class GirisEkrani : UserControl
    {
        public GirisEkrani()
        {
            InitializeComponent();
        }

        private void btnGirisYap_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtKullanici.Text) || string.IsNullOrEmpty(txtSifre.Password))
            {
                GamaMesaj.Tamam("Lütfen tüm alanları doldurunuz!", "EKSİK BİLGİ");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    string sorgu = @"SELECT YETKI_SEVIYESI, YETKI_PERSONEL, YETKI_MUSTERI, YETKI_STOK, 
                             YETKI_KASA, YETKI_CEKSENET, YETKI_RANDEVU, YETKI_MUHASEBE, 
                             YETKI_URUNLER, YETKI_FIRMALAR, YETKI_GRAFIKLER 
                             FROM TBL_KULLANICILAR 
                             WHERE KAD=@kadi AND SIFRE=@parola AND DURUM = 1";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@kadi", txtKullanici.Text);
                    komut.Parameters.AddWithValue("@parola", txtSifre.Password);

                    baglanti.Open();
                    SqlDataReader dr = komut.ExecuteReader();

                    if (dr.Read())
                    {
                    
                        App.GirisYapanKullanici = txtKullanici.Text;
                        App.GirisYapanYetki = dr["YETKI_SEVIYESI"].ToString()!;
                        App.YetkiKasa = Convert.ToBoolean(dr["YETKI_KASA"]);
                        App.YetkiStok = Convert.ToBoolean(dr["YETKI_STOK"]);
                        App.YetkiPersonel = Convert.ToBoolean(dr["YETKI_PERSONEL"]);
                        App.YetkiMusteri = Convert.ToBoolean(dr["YETKI_MUSTERI"]);
                        App.YetkiCekSenet = Convert.ToBoolean(dr["YETKI_CEKSENET"]);
                        App.YetkiRandevu = Convert.ToBoolean(dr["YETKI_RANDEVU"]);
                        App.YetkiMuhasebe = Convert.ToBoolean(dr["YETKI_MUHASEBE"]);
                        App.YetkiUrunler = Convert.ToBoolean(dr["YETKI_URUNLER"]);
                        App.YetkiFirmalar = Convert.ToBoolean(dr["YETKI_FIRMALAR"]);
                        App.YetkiGrafikler = Convert.ToBoolean(dr["YETKI_GRAFIKLER"]);
                        ArabaYukleme yuklemeEkrani = new ArabaYukleme($"Hoş geldiniz {txtKullanici.Text}!");
                        yuklemeEkrani.Owner = Window.GetWindow(this); 

                        
                        if (yuklemeEkrani.ShowDialog() == true)
                        {
                            
                            MainWindow anaSayfa = new MainWindow();
                            anaSayfa.Show();

                         
                            Window.GetWindow(this)?.Close();
                        }
                    }
                    else
                    {
                        GamaMesaj.Tamam("Kullanıcı adı, şifre hatalı veya hesabınız pasif durumda!", "GİRİŞ REDDEDİLDİ");
                    }
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Bağlantı Hatası: " + ex.Message, "SİSTEM HATASI");
            }
        }
    }
}