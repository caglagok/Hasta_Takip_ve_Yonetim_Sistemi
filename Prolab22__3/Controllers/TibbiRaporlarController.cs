using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Prolab22__3.Controllers
{
    public class TibbiRaporlarController : Controller
    {
        private readonly string _connectionString; // Connection String'i doğru şekilde ayarlayın

        public TibbiRaporlarController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

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

        public IActionResult Details(int id)
        {
            TempData["PreviousUrl"] = Request.Headers["Referer"].ToString();
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
        public IActionResult Create(int? hastaId)
        {
            ViewBag.PreviousUrl = Request.Headers["Referer"].ToString();
            var userRole = HttpContext.Session.GetString("Role");
            var tibbiRapor = new TibbiRapor { HastaID = hastaId ?? 0 };

            if (userRole == "Doktor")
            {
                int? doktorID = HttpContext.Session.GetInt32("DoktorID");
                if (doktorID.HasValue)
                {
                    tibbiRapor.DoktorID = doktorID.Value;
                    ViewBag.DoktorID = doktorID.Value;
                }
                ViewBag.UserRole = "Doktor";
            }
            else if (userRole == "Hasta")
            {
                hastaId = HttpContext.Session.GetInt32("HastaID");
                if (hastaId.HasValue)
                {
                    tibbiRapor.HastaID = hastaId.Value;
                    ViewBag.HastaID = hastaId.Value;
                }
                ViewBag.UserRole = "Hasta";
            }
            else
            {
                ViewBag.UserRole = "Yonetici";
            }
            return View(tibbiRapor);
        }

        // POST: TibbiRaporlar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("HastaID, DoktorID, RaporTarihi, RaporIcerigi, URL")] TibbiRapor tibbiRapor)
        {
            var userRole = HttpContext.Session.GetString("Role");
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("INSERT INTO TibbiRaporlar (HastaID, DoktorID, RaporTarihi, RaporIcerigi, URL) VALUES (@HastaID, @DoktorID, @RaporTarihi, @RaporIcerigi, @URL)", connection);
                    command.Parameters.AddWithValue("@HastaID", tibbiRapor.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", tibbiRapor.DoktorID.HasValue ? (object)tibbiRapor.DoktorID : DBNull.Value);
                    command.Parameters.AddWithValue("@RaporTarihi", tibbiRapor.RaporTarihi);
                    command.Parameters.AddWithValue("@RaporIcerigi", tibbiRapor.RaporIcerigi);
                    command.Parameters.AddWithValue("@URL", tibbiRapor.URL);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                if (userRole == "Hasta")
                {
                    return RedirectToAction("Index", "HastaInterface");
                }
                else
                {
                    return RedirectToAction("Details", "Hastalar", new { id = tibbiRapor.HastaID });
                }
            }
            return View(tibbiRapor);
        }

        public IActionResult Edit(int id)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("RaporID, HastaID, DoktorID, RaporTarihi, RaporIcerigi, URL")] TibbiRapor tibbiRapor)
        {
            if (id != tibbiRapor.RaporID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE TibbiRaporlar SET HastaID = @HastaID, DoktorID = @DoktorID, RaporTarihi = @RaporTarihi, RaporIcerigi = @RaporIcerigi, URL = @URL WHERE RaporID = @RaporID", connection);
                    command.Parameters.AddWithValue("@RaporID", tibbiRapor.RaporID);
                    command.Parameters.AddWithValue("@HastaID", tibbiRapor.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", tibbiRapor.DoktorID.HasValue ? (object)tibbiRapor.DoktorID : DBNull.Value);
                    command.Parameters.AddWithValue("@RaporTarihi", tibbiRapor.RaporTarihi);
                    command.Parameters.AddWithValue("@RaporIcerigi", tibbiRapor.RaporIcerigi);
                    command.Parameters.AddWithValue("@URL", tibbiRapor.URL);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                return RedirectToAction("Index", "HastaInterface", new { id = tibbiRapor.HastaID });
            }
            return View(tibbiRapor);
        }

        public IActionResult Delete(int id)
        {
            var tibbiRapor = GetTibbiRaporById(id);
            if (tibbiRapor == null)
            {
                return NotFound();
            }

            ViewBag.PreviousUrl = Request.Headers["Referer"].ToString();
            return View(tibbiRapor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, string previousUrl)
        {
            int hastaId;
            var userRole = HttpContext.Session.GetString("Role");
            using (var connection = new SqlConnection(_connectionString))
            {
                // Silinen raporun HastaID'sini alalım
                var getHastaIdCommand = new SqlCommand("SELECT HastaID FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                getHastaIdCommand.Parameters.AddWithValue("@RaporID", id);
                connection.Open();
                hastaId = (int)getHastaIdCommand.ExecuteScalar();

                // Raporu silelim
                var deleteCommand = new SqlCommand("DELETE FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                deleteCommand.Parameters.AddWithValue("@RaporID", id);
                deleteCommand.ExecuteNonQuery();
            }

            if (userRole == "Hasta")
            {
                return RedirectToAction("Index", "HastaInterface");
            }
            else
            {
                return RedirectToAction("Details", "Hastalar", new { id = hastaId });
            }
        }
        private TibbiRapor GetTibbiRaporById(int id)
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
            return tibbiRapor;
        } 
        public async Task<IActionResult> Download(int id)
        {
            TibbiRapor tibbiRapor = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RaporID, URL FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                command.Parameters.AddWithValue("@RaporID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        tibbiRapor = new TibbiRapor
                        {
                            RaporID = reader.GetInt32(0),
                            URL = reader.IsDBNull(1) ? null : reader.GetString(1)
                        };
                    }
                }
            }

            if (tibbiRapor == null || string.IsNullOrEmpty(tibbiRapor.URL))
            {
                return NotFound("Rapor bulunamadı veya geçerli bir URL içermiyor.");
            }

            var uri = new Uri(tibbiRapor.URL);
            if (uri.IsAbsoluteUri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                try
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        var fileStream = await response.Content.ReadAsStreamAsync();
                        var contentType = "application/octet-stream";
                        var fileName = Path.GetFileName(uri.LocalPath);
                        return File(fileStream, contentType, fileName);
                    }
                    return NotFound("Dosya erişilebilir değil.");
                }
                catch
                {
                    return BadRequest("Dosyayı indirirken bir hata oluştu.");
                }
            }
            else
            {
                return NotFound("Rapor URL'si geçersiz veya erişilemez.");
            }
        }
        /*
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            int hastaId;
            using (var connection = new SqlConnection(_connectionString))
            {
                // Silinen raporun HastaID'sini alalım
                var getHastaIdCommand = new SqlCommand("SELECT HastaID FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                getHastaIdCommand.Parameters.AddWithValue("@RaporID", id);
                connection.Open();
                hastaId = (int)getHastaIdCommand.ExecuteScalar();

                // Raporu silelim
                var deleteCommand = new SqlCommand("DELETE FROM TibbiRaporlar WHERE RaporID = @RaporID", connection);
                deleteCommand.Parameters.AddWithValue("@RaporID", id);
                deleteCommand.ExecuteNonQuery();
            }

            // Bir önceki sayfaya geri dön
            string previousUrl = TempData["PreviousUrl"] as string;
            if (!string.IsNullOrEmpty(previousUrl))
            {
                return Redirect(previousUrl);
            }

           return RedirectToAction("Details", "Hastalar", new { id = hastaId });
        }*/
    }
}
