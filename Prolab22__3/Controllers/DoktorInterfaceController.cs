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
            // Giriş yapmış kullanıcı ID'si (doktorID) bir şekilde elde edilmeli, burada örnek olarak 1 kullanılmıştır.
            int DoktorID = HttpContext.Session.GetInt32("DoktorID") ?? 1; // Varsayılan olarak 1 kullanıldı
            var model = new DoktorDashboardViewModel
            {
                Randevular = GetDoktorunRandevulari(DoktorID),
                TibbiRaporlar = GetDoktorunTıbbiRaporları(DoktorID)
            };
            return View(model);
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
