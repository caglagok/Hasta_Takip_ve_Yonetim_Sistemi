using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

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
        {
            // Giriş yapmış kullanıcı ID'si (hastaID) bir şekilde elde edilmeli, burada örnek olarak 1 kullanılmıştır.
            int hastaID = HttpContext.Session.GetInt32("HastaID") ?? 1; // Varsayılan olarak 1 kullanıldı
            var model = new HastaDashboardViewModel
            {
                Randevular = GetRandevular(hastaID),
                TibbiRaporlar = GetTibbiRaporlar(hastaID)
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
    }
}
