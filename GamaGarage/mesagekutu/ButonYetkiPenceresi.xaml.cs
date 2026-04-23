using System;
using System.Windows;


namespace GamaGarage.Anamenuler
{
    public partial class ButonYetkiPenceresi : Window
    {
        public ButonYetkiPenceresi()
        {
            InitializeComponent();
            GamaGarage.Veriler.TemaYardimcisi.TemaRenkleriniYukle(this.AnaStop1, this.AnaStop3);
        }

        
        private void btnYetkiTamam_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

    
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try { this.DragMove(); } catch { }
        }
    }
}