using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Prolab22__3.Controllers
{
    public class TibbiRaporlarController:Controller
    {
        private readonly string _connectionString; // Connection String'i doğru şekilde ayarlayın

        public TibbiRaporlarController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

        // Tıbbi raporların listesini getir
        public IActionResult Index()
        {
            var TibbiRaporlar = new List<TibbiRapor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RaporID, HastaID, DoktorID, RaporTarihi, RaporIcerigi, URL FROM TibbiRaporlar", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TibbiRaporlar.Add(new TibbiRapor
                        {
                            RaporID = reader.GetInt32(0),
                            HastaID = reader.GetInt32(1),
                            DoktorID = reader.GetInt32(2),
                            RaporTarihi = reader.GetDateTime(3),
                            RaporIcerigi = reader.GetString(4),
                            URL = reader.GetString(5)
                        });
                    }
                }
            }
            return View(TibbiRaporlar);
        }
      
        // GET: Paporlar/Details/5
        public IActionResult Details(int id)
        {
            TibbiRapor tibbiRapor = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RaporID, HastaID, DoktorID, RaporTarihi, RaporIcerigi, URL FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                command.Parameters.AddWithValue("@RaporID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        tibbiRapor = new TibbiRapor
                        {
                            RaporID = reader.GetInt32(0),
                            HastaID = reader.GetInt32(1),
                            DoktorID = reader.GetInt32(2),
                            RaporTarihi = reader.GetDateTime(3),
                            RaporIcerigi = reader.GetString(4),
                            URL = reader.GetString(5)
                        };
                    }
                }
            }
            if (tibbiRapor == null)
            {
                return NotFound();
            }
            return View(tibbiRapor);
        }
        // GET: TibbiRaporlar/Create
        public IActionResult Create()
        {
            // Oturumda saklanan HastaID değerini al, eğer yoksa varsayılan olarak 1 kullan
            int hastaID = HttpContext.Session.GetInt32("HastaID") ?? 1;

            // Model oluştururken, HastaID değerini set ediyoruz
            var tibbiRapor = new TibbiRapor { HastaID = hastaID };

            // Create view'ını, oluşturduğumuz model ile birlikte döndürüyoruz
            return View(tibbiRapor);
        }
        // POST: TibbiRaporlar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("HastaID, DoktorID, RaporTarihi, RaporIcerigi, URL")] TibbiRapor tibbiRapor)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("INSERT INTO TibbiRaporlar (HastaID, DoktorID, RaporTarihi, RaporIcerigi, URL) VALUES (@HastaID, @DoktorID, @RaporTarihi, @RaporIcerigi, @URL)", connection);
                    command.Parameters.AddWithValue("@HastaID", tibbiRapor.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", tibbiRapor.DoktorID);
                    command.Parameters.AddWithValue("@RaporTarihi", tibbiRapor.RaporTarihi);
                    command.Parameters.AddWithValue("@RaporIcerigi", tibbiRapor.RaporIcerigi);
                    command.Parameters.AddWithValue("@URL", tibbiRapor.URL);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                // Tıbbi rapor başarıyla eklendikten sonra, hasta detayları sayfasına geri dön
                return RedirectToAction("Details", "Hastalar", new { id = tibbiRapor.HastaID });
            }
            return View(tibbiRapor);
        }
        // GET: Hastalar/Edit/5
        public IActionResult Edit(int id)
        {
            TibbiRapor tibbiRapor = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RaporID, HastaID, DoktorID, RaporTarihi, RaporIcerigi,URL  FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                command.Parameters.AddWithValue("@RaporID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        tibbiRapor = new TibbiRapor
                        {
                            RaporID = reader.GetInt32(0),
                            HastaID = reader.GetInt32(1),
                            DoktorID = reader.GetInt32(2),
                            RaporTarihi = reader.GetDateTime(3),
                            RaporIcerigi = reader.GetString(4),
                            URL = reader.GetString(5)
                        };
                    }
                }
            }
            if (tibbiRapor == null)
            {
                return NotFound();
            }
            return View(tibbiRapor);
        }


        // POST: raporlar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("RaporID, HastaID, DoktorID, RaporTarihi, RaporIcerigi,URL")] TibbiRapor tibbiRapor)
        {
            if (id != tibbiRapor.RaporID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE TibbiRaporlar SET HastaID = @HastaID, DoktorID = @DoktorID, RaporTarihi = @RaporTarihi, RaporIcerigi=@RaporIcerigi, URL=@URL WHERE RaporID = @RaporID", connection);
                    command.Parameters.AddWithValue("@RaporID", tibbiRapor.RaporID);
                    command.Parameters.AddWithValue("@HastaID", tibbiRapor.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", tibbiRapor.DoktorID);
                    command.Parameters.AddWithValue("@RaporTarihi", tibbiRapor.RaporTarihi);
                    command.Parameters.AddWithValue("@RaporIcerigi", tibbiRapor.RaporIcerigi);
                    command.Parameters.AddWithValue("@URL", tibbiRapor.URL);


                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tibbiRapor);
        }
        // GET: Hastalar/Delete/5
        public IActionResult Delete(int id)
        {
            TibbiRapor tibbiRapor = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RaporID, HastaID, DoktorID, RaporTarihi, RaporIcerigi,URL FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                command.Parameters.AddWithValue("@RaporID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        tibbiRapor = new TibbiRapor
                        {
                            RaporID = reader.GetInt32(0),
                            HastaID = reader.GetInt32(1),
                            DoktorID = reader.GetInt32(2),
                            RaporTarihi = reader.GetDateTime(3),
                            RaporIcerigi = reader.GetString(4),
                            URL = reader.GetString(5)
                        };
                    }
                }
            }
            if (tibbiRapor == null)
            {
                return NotFound();
            }
            return View(tibbiRapor);
        }

        // POST: Hastalar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DELETE FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                command.Parameters.AddWithValue("@RaporID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
