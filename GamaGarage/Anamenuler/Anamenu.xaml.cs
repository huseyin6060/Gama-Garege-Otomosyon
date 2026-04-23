using GamaGarage.mesagekutu;
using System;
using System.Windows;
using System.Windows.Input;
using GamaGarage.Sayfalar;
using System.Windows.Media;
using System.Windows.Controls;

namespace GamaGarage.Anamenuler
{
    public partial class Anamenu : Window
    {
        public Anamenu()
        {
            InitializeComponent();
            GamaGarage.Veriler.TemaYardimcisi.TemaRenkleriniYukle(this.AnaStop1, this.AnaStop3);
            AnamenuIcerik.Content = null;
        }


        private void SayfaYonlendir(UserControl yeniSayfa)
        {
            if (AnamenuIcerik.Content != null && AnamenuIcerik.Content.GetType() == yeniSayfa.GetType())
            {
                AnamenuIcerik.Content = null;
            }
            else
            {
                AnamenuIcerik.Content = yeniSayfa;
            }
        }

        private void BtnGiris_Click(object sender, RoutedEventArgs e)
        {
            SayfaYonlendir(new GirisEkrani());
        }

        private void BtnKayıt_Click(object sender, RoutedEventArgs e)
        {
            SayfaYonlendir(new KayitEkrani());
        }

        private void BtnYedekal_Click(object sender, RoutedEventArgs e)
        {
            if (AnamenuIcerik.Content is YedeklemeEkrani)
            {
                AnamenuIcerik.Content = null;
                return;
            }
            AdminOnayPenceresi onay = new AdminOnayPenceresi { Owner = this };
            if (onay.ShowDialog() == true)
            {
                AnamenuIcerik.Content = new YedeklemeEkrani();
            }
            else
            {
                GamaMesaj.Tamam("Yedekleme alanına girmek için yönetici izni gereklidir.", "YETKİSİZ ERİŞİM");
            }
        }

        private void BtnCikis_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try { this.DragMove(); } catch { }
        }
    }
}