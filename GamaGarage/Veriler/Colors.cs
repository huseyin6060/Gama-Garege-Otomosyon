using System;
using System.Windows.Media;

namespace GamaGarage.Veriler
{
    public static class TemaYardimcisi
    {
        
        public static void TemaRenkleriniYukle(GradientStop stop1, GradientStop stop3)
        {
            try
            {
                
                var c1 = Properties.Settings.Default.TemaRenk1;
                var c2 = Properties.Settings.Default.TemaRenk2;

              
                if (c1.A != 0 || c2.A != 0)
                {
                    if (stop1 != null)
                        stop1.Color = Color.FromArgb(c1.A, c1.R, c1.G, c1.B);

                    if (stop3 != null)
                        stop3.Color = Color.FromArgb(c2.A, c2.R, c2.G, c2.B);
                }
            }
            catch {  }
        }
    }
}