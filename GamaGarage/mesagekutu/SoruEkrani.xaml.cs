using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GamaGarage.mesagekutu
{
    public partial class SoruEkrani : Window
    {
        public string Secim { get; set; } = "Vazgeç";

        public SoruEkrani()
        {
            InitializeComponent();
            GamaGarage.Veriler.TemaYardimcisi.TemaRenkleriniYukle(this.AnaStop1, this.AnaStop3);
        }



    
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                Secim = btn.Content.ToString()!;
                this.DialogResult = true;
            }
        }
    }
}