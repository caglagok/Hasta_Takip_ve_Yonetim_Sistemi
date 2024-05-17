using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prolab22__3.Controllers
{
    public class RandevularController:Controller
    {
        private readonly string _connectionString; // Connection String'i doğru şekilde ayarlayın

        public RandevularController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }
        // Randevuların listesini getir
        public IActionResult Index()
        {
            var randevular = new List<Randevu>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RandevuID, HastaID, DoktorID, RandevuTarihi, RandevuSaati FROM Randevular", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        randevular.Add(new Randevu
                        {
                            RandevuID = reader.GetInt32(0),
                            HastaID = reader.GetInt32(1),
                            DoktorID = reader.GetInt32(2),
                            RandevuTarihi = reader.GetDateTime(3),
                            RandevuSaati = reader.GetTimeSpan(4)
                        });
                    }
                }
            }

            // ViewBag.Randevular'a veri atama
            ViewBag.Randevular = randevular;

            return View(randevular);
        }

      //  [HttpGet]
        public JsonResult GetUzmanlikAlanlari()
        {
            List<string> uzmanlikAlanlari = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT DISTINCT UzmanlikAlani FROM Doktorlar", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uzmanlikAlanlari.Add(reader.GetString(0));
                    }
                }
            }
            return Json(uzmanlikAlanlari);
        }
       // [HttpGet]
        public JsonResult GetHastanelerByUzmanlik(string uzmanlik)
        {
            List<string> hastaneler = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT DISTINCT CalistigiHastane FROM Doktorlar WHERE UzmanlikAlani = @Uzmanlik", connection);
                command.Parameters.AddWithValue("@Uzmanlik", uzmanlik);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hastaneler.Add(reader.GetString(0));
                    }
                }
            }
            return Json(hastaneler);
        }

        //[HttpGet]
        public JsonResult GetDoktorlarByHastaneAndUzmanlik(string hastane, string uzmanlik)
        {
            List<object> doktorlar = new List<object>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT DoktorID, Ad, Soyad FROM Doktorlar WHERE CalistigiHastane = @Hastane AND UzmanlikAlani = @Uzmanlik", connection);
                command.Parameters.AddWithValue("@Hastane", hastane);
                command.Parameters.AddWithValue("@Uzmanlik", uzmanlik);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        doktorlar.Add(new
                        {
                            DoktorID = reader.GetInt32(0),
                            DoktorBilgisi = $"{reader.GetString(1)} {reader.GetString(2)}"
                        });
                    }
                }
            }
            return Json(doktorlar);
        }
        //GET: Randevular/Details/5
        public IActionResult Details(int id)
        {
            Randevu randevu = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RandevuID, HastaID, DoktorID, RandevuTarihi, RandevuSaati FROM Randevular WHERE RandevuID = @HastaID", connection);
                command.Parameters.AddWithValue("@RandevuID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        randevu = new Randevu
                        {
                            RandevuID = reader.GetInt32(0),
                            HastaID = reader.GetInt32(1),
                            DoktorID = reader.GetInt32(2),
                            RandevuTarihi = reader.GetDateTime(3),
                            RandevuSaati = reader.GetTimeSpan(4)
                        };
                    }
                }
            }
            if (randevu == null)
            {
                return NotFound();
            }
            return View(randevu);
        }
        
        // GET: Randevular/Create
        public IActionResult Create(int hastaId)
        {
            TempData["PreviousUrl"] = Request.Headers["Referer"].ToString();
            int? hastaID = HttpContext.Session.GetInt32("HastaID");
            if (!hastaID.HasValue)
            {
                hastaID = hastaId;
            }

            var doktorlar = GetDoktorlar();
            ViewBag.Doktorlar = new SelectList(doktorlar, "DoktorID", "DoktorBilgisi");
            ViewBag.HastaID = hastaID.Value;

            // Hasta adı ve soyadını çekme
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT Ad, Soyad FROM Hastalar WHERE HastaID = @HastaID", connection);
                command.Parameters.AddWithValue("@HastaID", hastaID.Value);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ViewBag.HastaAdi = reader.GetString(0);
                        ViewBag.HastaSoyadi = reader.GetString(1);
                    }
                }
            }

            return View(new Randevu { HastaID = hastaID.Value });
        }

        private List<Doktor> GetDoktorlar()
        {
            List<Doktor> doktorlar = new List<Doktor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT DoktorID, Ad, Soyad, UzmanlikAlani FROM Doktorlar", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        doktorlar.Add(new Doktor
                        {
                            DoktorID = reader.GetInt32(0),
                            Ad = reader.GetString(1),
                            Soyad = reader.GetString(2),
                            UzmanlikAlani = reader.GetString(3)
                        });
                    }
                }
            }
            return doktorlar;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Randevu randevu)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Önce HastaID'nin varlığını kontrol edin
                    var checkCommand = new SqlCommand("SELECT COUNT(1) FROM Hastalar WHERE HastaID = @HastaID", connection);
                    checkCommand.Parameters.AddWithValue("@HastaID", randevu.HastaID);
                    connection.Open();
                    int exists = (int)checkCommand.ExecuteScalar();

                    if (exists == 0)
                    {
                        // HastaID mevcut değil
                        TempData["ErrorMessage"] = "Geçersiz Hasta ID. Lütfen geçerli bir Hasta ID giriniz.";
                        return View(randevu);
                    }

                    // HastaID mevcutsa randevuyu kaydet
                    var command = new SqlCommand("INSERT INTO Randevular (HastaID, DoktorID, RandevuTarihi, RandevuSaati) VALUES (@HastaID, @DoktorID, @RandevuTarihi, @RandevuSaati)", connection);
                    command.Parameters.AddWithValue("@HastaID", randevu.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", randevu.DoktorID);
                    command.Parameters.AddWithValue("@RandevuTarihi", randevu.RandevuTarihi);
                    command.Parameters.AddWithValue("@RandevuSaati", randevu.RandevuSaati);
                    command.ExecuteNonQuery();


                    // Doktora bildirim ekleme
                    var doktorBildirimMesaj = $"Yeni bir randevu alındı: {randevu.RandevuTarihi.ToString("dd/MM/yyyy")} {randevu.RandevuSaati}";
                    DoktoraBildirimEkle(randevu.DoktorID, doktorBildirimMesaj);

                    TempData["SuccessMessage"] = "Randevu başarıyla kaydedildi.";
                    string previousUrl = TempData["PreviousUrl"] as string;
                    if (!string.IsNullOrEmpty(previousUrl))
                    {
                        return Redirect(previousUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "HastaInterface");
                    }
                }
            }

            return View(randevu);
        }
        private void DoktoraBildirimEkle(int doktorID, string mesaj)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("INSERT INTO Bildirimler (KullaniciID, Mesaj, OlusturmaTarihi, Okundu, Role) VALUES (@KullaniciID, @Mesaj, @OlusturmaTarihi, @Okundu, @Role)", connection);
                command.Parameters.AddWithValue("@KullaniciID", doktorID);
                command.Parameters.AddWithValue("@Mesaj", mesaj);
                command.Parameters.AddWithValue("@OlusturmaTarihi", DateTime.Now);
                command.Parameters.AddWithValue("@Okundu", false);
                command.Parameters.AddWithValue("@Role", "Doktor");
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        // GET: Randevular/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Randevu randevu = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RandevuID, HastaID, DoktorID, RandevuTarihi, RandevuSaati FROM Randevular WHERE RandevuID = @RandevuID", connection);
                command.Parameters.AddWithValue("@RandevuID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        randevu = new Randevu
                        {
                            RandevuID = reader.GetInt32(0),
                            HastaID = reader.GetInt32(1),
                            DoktorID = reader.GetInt32(2),
                            RandevuTarihi = reader.GetDateTime(3),
                            RandevuSaati = reader.GetTimeSpan(4)
                        };
                    }
                }
            }
            if (randevu == null)
            {
                return NotFound();
            }

            ViewBag.HastaID = randevu.HastaID;
            ViewBag.DoktorID = randevu.DoktorID;
            ViewBag.UserRole = HttpContext.Session.GetString("Role") ?? "";

            TempData["PreviousUrl"] = Request.Headers["Referer"].ToString();

            return View(randevu);
        }

        // POST: Randevular/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("RandevuID,HastaID,DoktorID,RandevuTarihi,RandevuSaati")] Randevu randevu)
        {
            if (id != randevu.RandevuID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE Randevular SET HastaID=@HastaID,DoktorID=@DoktorID, RandevuTarihi=@RandevuTarihi, RandevuSaati=@RandevuSaati WHERE RandevuID=@RandevuID", connection);
                    command.Parameters.AddWithValue("@HastaID", randevu.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", randevu.DoktorID);
                    command.Parameters.AddWithValue("@RandevuTarihi", randevu.RandevuTarihi);
                    command.Parameters.AddWithValue("@RandevuSaati", randevu.RandevuSaati);
                    command.Parameters.AddWithValue("@RandevuID", randevu.RandevuID);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                string userRole = HttpContext.Session.GetString("Role") ?? "";
                if (userRole == "Yonetici")
                {
                    return RedirectToAction("Details", "Hastalar", new { id = randevu.HastaID });
                }
                else if (userRole == "Hasta")
                {
                    return RedirectToAction("Index", "HastaInterface");
                }
                else
                {
                    string previousUrl = TempData["PreviousUrl"] as string;
                    if (!string.IsNullOrEmpty(previousUrl))
                    {
                        return Redirect(previousUrl);
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            return View(randevu);
        }



        // GET: Randevular/Delete/5
        public IActionResult Delete(int id, int hastaId)
        {
            ViewBag.HastaID = hastaId;
            Randevu randevu = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RandevuID, HastaID, DoktorID, RandevuTarihi, RandevuSaati FROM Randevular WHERE RandevuID = @RandevuID", connection);
                command.Parameters.AddWithValue("@RandevuID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        randevu = new Randevu
                        {
                            RandevuID = reader.GetInt32(0),
                            HastaID = reader.GetInt32(1),
                            DoktorID = reader.GetInt32(2),
                            RandevuTarihi = reader.GetDateTime(3),
                            RandevuSaati = reader.GetTimeSpan(4)
                        };
                    }
                }
            }
            if (randevu == null)
            {
                return NotFound();
            }
            return View(randevu);
        }
        // POST: Randevular/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, int hastaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DELETE FROM Randevular WHERE RandevuID = @RandevuID", connection);
                command.Parameters.AddWithValue("@RandevuID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("Details", "Hastalar", new { id = hastaId });
        }
    }
}
