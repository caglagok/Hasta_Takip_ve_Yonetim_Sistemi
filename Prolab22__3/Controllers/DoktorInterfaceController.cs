using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;

namespace Prolab22__3.Controllers
{
    public class DoktorInterfaceController : Controller
    {
        private readonly string _connectionString;

        public DoktorInterfaceController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }
        public IActionResult Index()
        {
            int? doktorID = HttpContext.Session.GetInt32("DoktorID");
            if (doktorID == null)
            {
                return RedirectToAction("LoginDoktor", "Doktorlar");
            }

            // Giriş yapmış kullanıcı ID'si (doktorID) bir şekilde elde edilmeli, burada örnek olarak 1 kullanılmıştır.
            //int DoktorID = HttpContext.Session.GetInt32("DoktorID") ?? 1; // Varsayılan olarak 1 kullanıldı
            var model = new DoktorDashboardViewModel
            {
                Randevular = GetDoktorunRandevulari(doktorID.Value),
                TibbiRaporlar = GetDoktorunTıbbiRaporları(doktorID.Value),
                Bildirimler = GetDoktorBildirimler(doktorID.Value)
            };
            return View(model);
        }
        private List<Bildirim> GetDoktorBildirimler(int doktorID)
        {
            List<Bildirim> bildirimler = new List<Bildirim>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(@"
                    SELECT BildirimID, Mesaj, OlusturmaTarihi, Okundu 
                    FROM Bildirimler 
                    WHERE KullaniciID = @DoktorID AND Role = 'Doktor'", connection);

                command.Parameters.AddWithValue("@DoktorID", doktorID);
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

        private List<Randevu> GetDoktorunRandevulari(int doktorID)
        {
            var randevular = new List<Randevu>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(@"
            SELECT r.RandevuID, r.RandevuTarihi, r.RandevuSaati, h.Ad AS HastaAdi, h.Soyad AS HastaSoyadi, h.HastaID
            FROM Randevular r
            JOIN Hastalar h ON r.HastaID = h.HastaID
            WHERE r.DoktorID = @DoktorID", connection);

                command.Parameters.AddWithValue("@DoktorID", doktorID);
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
                            HastaAdi = reader.GetString(3),
                            HastaSoyadi = reader.GetString(4),
                            HastaID = reader.GetInt32(5)
                        });
                    }
                }
            }
            return randevular;
        }
        private List<TibbiRapor> GetDoktorunTıbbiRaporları(int doktorID)
        {
            List<TibbiRapor> raporlar = new List<TibbiRapor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(@"
                    SELECT tr.RaporID, tr.RaporTarihi, tr.RaporIcerigi, h.Ad + ' ' + h.Soyad AS HastaAdi
                    FROM TibbiRaporlar tr
                    JOIN Hastalar h ON tr.HastaID = h.HastaID
                    WHERE tr.DoktorID = @DoktorID", connection);

                command.Parameters.AddWithValue("@DoktorID", doktorID);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        raporlar.Add(new TibbiRapor
                        {
                            RaporID = reader.GetInt32(0),
                            RaporTarihi = reader.GetDateTime(1),
                            RaporIcerigi = reader.GetString(2),
                        });
                    }
                }
            }
            return raporlar;
        }
    }
}