using Microsoft.Data.SqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using GamaGarage.mesagekutu;

namespace GamaGarage.Veriler
{
    public static class VeriIslemleri
    {
        public static void ComboBoxFirmaDoldur(ComboBox cmb)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(App.BaglantiCumlesi))
                {
                    
                    string sorgu = "SELECT FirmaID, FirmaAd FROM Firmalar ORDER BY FirmaAd ASC";
                    SqlCommand komut = new SqlCommand(sorgu, baglanti);
                    baglanti.Open();

                    SqlDataReader dr = komut.ExecuteReader();

                    
                    var firmaListesi = new System.Collections.Generic.List<object>();

                    while (dr.Read())
                    {
                        firmaListesi.Add(new
                        {
                            ID = Convert.ToInt32(dr["FirmaID"]),
                            Ad = dr["FirmaAd"].ToString()
                        });
                    }

                    cmb.ItemsSource = firmaListesi;
                    cmb.DisplayMemberPath = "Ad";    
                    cmb.SelectedValuePath = "ID";   
                }
            }
            catch (Exception ex)
            {
                GamaMesaj.Tamam("Global Firma Listesi Yükleme Hatası: " + ex.Message);
            }
        }
    }
}