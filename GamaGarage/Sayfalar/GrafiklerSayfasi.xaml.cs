using GamaGarage.mesagekutu;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GamaGarage.Sayfalar
{
    public partial class GrafiklerSayfasi : UserControl
    {
        // GLOBAL ERİŞİM İÇİN: Bu sayfa açık olduğu sürece her yerden ulaşılabilir.
        public static GrafiklerSayfasi? Instance { get; private set; }

        public ChartValues<decimal> GelirGiderDegerleri { get; set; } = new();
        public ChartValues<int> StokDegerleri { get; set; } = new();
        public ObservableCollection<string> StokIsimleri { get; set; } = new();
        public ChartValues<int> ServisDegerleri { get; set; } = new();
        public ObservableCollection<string> Gunler { get; set; } = new();
        public ChartValues<decimal> MaasDegerleri { get; set; } = new();
        public ObservableCollection<string> PersonelIsimleri { get; set; } = new();

        public GrafiklerSayfasi()
        {
            InitializeComponent();
            this.DataContext = this;
            Instance = this; // Sayfa belleğe yüklendiğinde kendini 'Instance' olarak kaydeder.
            VerileriYukle();
        }

        // Bu metot public olduğu için dışarıdan çağrılabilir
        public void VerileriYukle()
        {
            try
            {
                KasaDurumuGetir();
                KritikStokGetir();
                ServisTrafigiGetir();
                MaasDagilimiGetir();
                MekanDolulukGetir();
                MusteriProfiliGetir();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Grafik Yenileme Hatası: " + ex.Message);
            }
        }

        private void KasaDurumuGetir()
        {
            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                baglanti.Open();
                string sorgu = @"SELECT ISNULL(SUM(CASE WHEN IslemTipi = 'GELİR' THEN Tutar ELSE 0 END), 0) as GenelToplamGelir,
                                 ISNULL(SUM(CASE WHEN IslemTipi = 'GİDER' THEN Tutar ELSE 0 END), 0) as GenelToplamGider FROM View_KasaGenelRapor";

                SqlCommand komut = new SqlCommand(sorgu, baglanti);
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    GelirGiderDegerleri.Clear(); // Önce temizle
                    if (dr.Read())
                    {
                        GelirGiderDegerleri.Add(Convert.ToDecimal(dr["GenelToplamGelir"]));
                        GelirGiderDegerleri.Add(Convert.ToDecimal(dr["GenelToplamGider"]));
                    }
                }
            }
        }

        private void KritikStokGetir()
        {
            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                baglanti.Open();
                string sorgu = "SELECT TOP 5 UrunAd, StokAdedi FROM dbo.Urunler ORDER BY StokAdedi ASC";
                SqlCommand komut = new SqlCommand(sorgu, baglanti);
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    StokIsimleri.Clear();
                    StokDegerleri.Clear();
                    while (dr.Read())
                    {
                        StokIsimleri.Add(dr["UrunAd"].ToString()!);
                        StokDegerleri.Add(Convert.ToInt32(dr["StokAdedi"]));
                    }
                }
            }
        }

        private void ServisTrafigiGetir()
        {
            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                baglanti.Open();
                string sorgu = @"SELECT TOP 7 CAST(BaslangicTarih AS DATE) as Tarih, COUNT(*) as Adet 
                                 FROM dbo.Randevular 
                                 GROUP BY CAST(BaslangicTarih AS DATE) 
                                 ORDER BY Tarih DESC";
                SqlCommand komut = new SqlCommand(sorgu, baglanti);
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    Gunler.Clear();
                    ServisDegerleri.Clear();
                    while (dr.Read())
                    {
                        Gunler.Add(Convert.ToDateTime(dr["Tarih"]).ToString("dd/MM"));
                        ServisDegerleri.Add(Convert.ToInt32(dr["Adet"]));
                    }
                }
            }
        }

        private void MaasDagilimiGetir()
        {
            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                baglanti.Open();
                string sorgu = @"SELECT P.Ad + ' ' + P.Soyad as Isim, O.NetMaas 
                                 FROM dbo.Personeller P 
                                 INNER JOIN dbo.PersonelOdemeleri O ON P.ID = O.PersonelID";

                SqlCommand komut = new SqlCommand(sorgu, baglanti);
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    PersonelIsimleri.Clear();
                    MaasDegerleri.Clear();
                    while (dr.Read())
                    {
                        PersonelIsimleri.Add(dr["Isim"].ToString()!);
                        MaasDegerleri.Add(Convert.ToDecimal(dr["NetMaas"]));
                    }
                }
            }
        }

        private void MekanDolulukGetir()
        {
            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                baglanti.Open();
                int toplamSlotSayisi = 7;
                int doluSayisi = Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM dbo.Randevular WHERE IslemTipi IN ('Şimdi', 'Dolu')", baglanti).ExecuteScalar());
                int rezerveSayisi = Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM dbo.Randevular WHERE IslemTipi = 'Rezerve'", baglanti).ExecuteScalar());

                int bosSayisi = Math.Max(0, toplamSlotSayisi - (doluSayisi + rezerveSayisi));

                ChartMekan.Series = new SeriesCollection
                {
                    new PieSeries { Title = "Dolu", Values = new ChartValues<int> { doluSayisi }, Fill = System.Windows.Media.Brushes.Crimson, DataLabels = true },
                    new PieSeries { Title = "Rezerve", Values = new ChartValues<int> { rezerveSayisi }, Fill = System.Windows.Media.Brushes.Orange, DataLabels = true },
                    new PieSeries { Title = "Boş", Values = new ChartValues<int> { bosSayisi }, Fill = System.Windows.Media.Brushes.LimeGreen, DataLabels = true }
                };
            }
        }

        private void MusteriProfiliGetir()
        {
            using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
            {
                baglanti.Open();
                int toplamMusteri = (int)new SqlCommand("SELECT COUNT(*) FROM dbo.Musteriler", baglanti).ExecuteScalar();

                ChartMusteri.Series = new SeriesCollection
                {
                    new PieSeries { Title = "Kayıtlı", Values = new ChartValues<int> { toplamMusteri }, DataLabels = true },
                    new PieSeries { Title = "Hedef", Values = new ChartValues<int> { 100 }, DataLabels = true }
                };
            }
        }
    }
}