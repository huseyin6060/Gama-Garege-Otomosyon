using Microsoft.Data.SqlClient;
using System;
using System.Windows;

namespace GamaGarage.mesagekutu
{
    public partial class AdminOnayPenceresi : Window
    {
        public string? GelenYetki { get; set; }

        public AdminOnayPenceresi()
        {
            InitializeComponent();
            GamaGarage.Veriler.TemaYardimcisi.TemaRenkleriniYukle(this.AnaStop1, this.AnaStop3);
        }

        private void btnOnayla_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    
                    string sorgu = "SELECT YETKI_SEVIYESI FROM TBL_KULLANICILAR WHERE KAD=@kadi AND SIFRE=@sifre AND DURUM = 1";

                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    komut.Parameters.AddWithValue("@kadi", txtAdminKadi.Text);
                    komut.Parameters.AddWithValue("@sifre", txtAdminSifre.Password);

                    baglanti.Open();
                    object sonuc = komut.ExecuteScalar();

                    if (sonuc != null)
                    {
                        GelenYetki = sonuc.ToString()!;
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                      
                        MessageBox.Show("Bilgiler hatalı veya bu işlem için yetkiniz (hesabınız) pasif durumda!", "Onay Reddedildi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sistem Hatası: " + ex.Message);
            }
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try { this.DragMove(); } catch { }
        }

        private void btnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}