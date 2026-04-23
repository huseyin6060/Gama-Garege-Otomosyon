using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.Data.SqlClient;
using GamaGarage.mesagekutu; 

namespace GamaGarage.Anamenuler
{
    public partial class YedeklemeEkrani : UserControl
    {

        private string MasterBaglantiOlustur()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(App.BaglantiCumlesi);
            builder.InitialCatalog = "master";
            return builder.ConnectionString;
        }

        string dbAdi = "GamaGarageDB";

        public YedeklemeEkrani()
        {
            InitializeComponent();
        }

        private void BtnYedekAl_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Yedek Dosyası (*.bak)|*.bak";
            sfd.FileName = dbAdi + "_" + DateTime.Now.ToString("ddMMyyyy_HHmm");

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (SqlConnection baglanti = new SqlConnection(MasterBaglantiOlustur()))
                    {
                        string sqlSorgu = $@"BACKUP DATABASE [{dbAdi}] TO DISK = '{sfd.FileName}' WITH FORMAT, MEDIANAME = 'GamaBackup', NAME = 'Full Backup of {dbAdi}';";

                        SqlCommand komut = new SqlCommand(sqlSorgu, baglanti);
                        baglanti.Open();
                        komut.ExecuteNonQuery();

                       
                        GamaMesaj.Tamam("Veritabanı yedeği başarıyla oluşturuldu!", "İŞLEM BAŞARILI");
                    }
                }
                catch (Exception ex)
                {
                  
                    GamaMesaj.Tamam("Yedekleme sırasında bir hata oluştu. Sunucuya erişilemiyor olabilir.\n\nDetay: " + ex.Message, "HATA");
                }
            }
        }


        private void BtnYedekYukle_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Yedek Dosyası (*.bak)|*.bak";

            if (ofd.ShowDialog() == true)
            {
                
                if (GamaMesaj.Onay("Veritabanını geri yüklemek mevcut tüm verileri silecektir. Emin misiniz?", "KRİTİK UYARI"))
                {
                    try
                    {
                        using (SqlConnection baglanti = new SqlConnection(MasterBaglantiOlustur()))
                        {
                            baglanti.Open();

                            string sqlSorgu = $@"
                                ALTER DATABASE [{dbAdi}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                RESTORE DATABASE [{dbAdi}] FROM DISK = '{ofd.FileName}' WITH REPLACE;
                                ALTER DATABASE [{dbAdi}] SET MULTI_USER;";

                            SqlCommand komut = new SqlCommand(sqlSorgu, baglanti);
                            komut.ExecuteNonQuery();

                         
                            GamaMesaj.Tamam("Veritabanı geri yükleme işlemi başarıyla tamamlandı!", "İŞLEM BAŞARILI");
                        }
                    }
                    catch (Exception ex)
                    {
                       
                        GamaMesaj.Tamam("Geri yükleme hatası! Lütfen SQL Server Configuration Manager'dan erişim izinlerini kontrol edin.\n\nDetay: " + ex.Message, "HATA");
                    }
                }
            }
        }
    }
}