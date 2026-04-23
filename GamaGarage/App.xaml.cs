using System.Configuration;
using System.Data;
using System.Windows;

namespace GamaGarage
{
 
    public partial class App : Application
    {

        public static string GirisYapanKullanici = "";
        public static string GirisYapanYetki = ""; 

        
        public static bool YetkiKasa, YetkiStok, YetkiPersonel, YetkiMusteri, YetkiCekSenet, YetkiRandevu, YetkiMuhasebe, YetkiUrunler, YetkiFirmalar, YetkiGrafikler, YetkiYedek;


        public static string BaglantiCumlesi = @"Server=HUSEYIN-YUCE\SQLEXPRESS;Database=GamaGarageDB;Trusted_Connection=True;TrustServerCertificate=True;";

    }

}
