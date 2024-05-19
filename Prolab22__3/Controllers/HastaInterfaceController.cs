using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Prolab22__3.Controllers
{
    public class HastaInterfaceController : Controller
    {
        private readonly string _connectionString;
        public HastaInterfaceController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
           
        }

        public IActionResult Index()
        {   int hastaID = HttpContext.Session.GetInt32("HastaID") ?? 1; 
            var model = new HastaDashboardViewModel
            {
                Randevular = GetRandevular(hastaID),
                TibbiRaporlar = GetTibbiRaporlar(hastaID),
                Bildirimler = GetHastaBildirimler(hastaID) 
            };
            return View(model);
        }

        private List<Randevu> GetRandevular(int hastaID)
        {
            List<Randevu> randevular = new List<Randevu>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(@"
            SELECT r.RandevuID, r.RandevuTarihi, r.RandevuSaati, d.Ad + ' ' + d.Soyad AS DoktorAdi
            FROM Randevular r
            JOIN Doktorlar d ON r.DoktorID = d.DoktorID
            WHERE r.HastaID = @HastaID", connection);

                command.Parameters.AddWithValue("@HastaID", hastaID);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        randevular.Add(new Randevu
                        {
                            RandevuID = reader.GetInt32(0),
                            RandevuTarihi = reader.GetDateTime(1),
                            RandevuSaati = reader.GetTimeSpan(2),
                            DoktorAdi = reader.GetString(3)
                        });
                    }
                }
            }
            return randevular;
        }
        private List<TibbiRapor> GetTibbiRaporlar(int hastaID)
        {
            List<TibbiRapor> raporlar = new List<TibbiRapor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RaporID, RaporTarihi, RaporIcerigi FROM TibbiRaporlar WHERE HastaID = @HastaID", connection);
                command.Parameters.AddWithValue("@HastaID", hastaID);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        raporlar.Add(new TibbiRapor
                        {
                            RaporID = reader.GetInt32(0),
                            RaporTarihi = reader.GetDateTime(1),
                            RaporIcerigi = reader.GetString(2)
                        });
                    }
                }
            }
            return raporlar;
        }
        private List<Bildirim> GetHastaBildirimler(int hastaID)
        {
            List<Bildirim> bildirimler = new List<Bildirim>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(@"
                    SELECT BildirimID, Mesaj, OlusturmaTarihi, Okundu 
                    FROM Bildirimler 
                    WHERE KullaniciID = @HastaID AND Role = 'Hasta'", connection);

                command.Parameters.AddWithValue("@HastaID", hastaID);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bildirimler.Add(new Bildirim
                        {
                            BildirimID = reader.GetInt32(0),
                            Mesaj = reader.GetString(1),
                            OlusturmaTarihi = reader.GetDateTime(2),
                            Okundu = reader.GetBoolean(3)
                        });
                    }
                }
            }
            return bildirimler;
        }

        [HttpPost]
        public IActionResult MarkAsRead(int bildirimID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("UPDATE Bildirimler SET Okundu = 1 WHERE BildirimID = @BildirimID", connection);
                command.Parameters.AddWithValue("@BildirimID", bildirimID);
                connection.Open();
                command.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult GetHastaRandevular(int hastaID)
        {
            var randevular = new List<RandevuViewModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    @"SELECT r.RandevuTarihi, r.RandevuSaati, d.Ad, d.Soyad 
                      FROM Randevular r
                      INNER JOIN Doktorlar d ON r.DoktorID = d.DoktorID
                      WHERE r.HastaID = @HastaID", connection);

                command.Parameters.AddWithValue("@HastaID", hastaID);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var doktorAdi = reader.GetString(2); 
                        var doktorSoyadi = reader.GetString(3); 

                        randevular.Add(new RandevuViewModel
                        {
                            RandevuTarihi = reader.GetDateTime(0),
                            RandevuSaati = reader.GetTimeSpan(1),
                            DoktorAdi = doktorAdi + " " + doktorSoyadi 
                        });
                    }
                }
            }
            return View(randevular);
        }
    }
}
