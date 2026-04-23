using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GamaGarage.mesagekutu
{
    public partial class ArabaYukleme : Window
    {
        public ArabaYukleme(string mesaj)
        {
            InitializeComponent();
            TxtAMesaj.Text = mesaj;
            this.Loaded += (s, e) => ArabaAnimasyonunuBaslat();
        }

        private async void ArabaAnimasyonunuBaslat()
        {
            DoubleAnimation giris = new DoubleAnimation
            {
                From = -150,
                To = 80,
                Duration = TimeSpan.FromSeconds(2.5), 
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } 
            };

            ArabaHareket.BeginAnimation(TranslateTransform.XProperty, giris);

       
            await Task.Delay(4500);
            DoubleAnimation gazla = new DoubleAnimation
            {
                To = 600,
                Duration = TimeSpan.FromSeconds(0.8), 
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 8 }
            };

            gazla.Completed += (s, e) =>
            {
                this.DialogResult = true; 
                this.Close(); 
            };

            ArabaHareket.BeginAnimation(TranslateTransform.XProperty, gazla);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}