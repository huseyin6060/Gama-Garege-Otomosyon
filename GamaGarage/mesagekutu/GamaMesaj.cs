using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;

namespace GamaGarage.mesagekutu
{
    public static class GamaMesaj
    {

        public static void Tamam(string mesaj, string baslik = "SİSTEM BİLGİSİ")
        {
            CustomMessageBox mbox = new CustomMessageBox();
            mbox.TxtMesaj.Text = mesaj;
            mbox.TxtBaslik.Text = baslik.ToUpper();

            mbox.PnlTamam.Visibility = Visibility.Visible;
            mbox.PnlOnay.Visibility = Visibility.Collapsed;

        
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
            {
                mbox.Owner = Application.Current.MainWindow;
                mbox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                mbox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            mbox.ShowDialog();
        }


        public static bool Onay(string mesaj, string baslik = "ONAY İŞLEMİ")
        {
            CustomMessageBox mbox = new CustomMessageBox();
            mbox.TxtMesaj.Text = mesaj;
            mbox.TxtBaslik.Text = baslik.ToUpper();

          
            mbox.PnlTamam.Visibility = Visibility.Collapsed;
            mbox.PnlOnay.Visibility = Visibility.Visible;

            if (mbox != Application.Current.MainWindow)
            {
                mbox.Owner = Application.Current.MainWindow;
            }


            return mbox.ShowDialog() ?? false;
        }
    }
}
