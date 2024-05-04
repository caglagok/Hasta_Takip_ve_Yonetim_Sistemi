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
            return View(randevular);
        }

        [HttpGet]
        public IActionResult GetUzmanlikAlanlari()
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

        [HttpGet]
        public IActionResult GetHastanelerByUzmanlik(string uzmanlik)
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

        [HttpGet]
        public IActionResult GetDoktorlarByHastaneAndUzmanlik(string hastane, string uzmanlik)
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
        public IActionResult Create()
        {
            int hastaID = HttpContext.Session.GetInt32("HastaID") ?? throw new InvalidOperationException("Hasta girişi yapılmamış.");
            var doktorlar = GetDoktorlar();

            ViewBag.Doktorlar = new SelectList(doktorlar, "DoktorID", "DoktorBilgisi");
            ViewBag.HastaID = hastaID;

            return View();
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
        // POST: Randevular/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Randevu randevu)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("INSERT INTO Randevular (HastaID, DoktorID, RandevuTarihi, RandevuSaati) VALUES (@HastaID, @DoktorID, @RandevuTarihi, @RandevuSaati)", connection);
                    command.Parameters.AddWithValue("@HastaID", randevu.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", randevu.DoktorID);
                    command.Parameters.AddWithValue("@RandevuTarihi", randevu.RandevuTarihi);
                    command.Parameters.AddWithValue("@RandevuSaati", randevu.RandevuSaati);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return RedirectToAction(nameof(Index));
            }

            // Formdaki hataları düzeltmek üzere kullanıcıya formu tekrar göster
            return View(randevu);
        }
        // GET: Randevular/Edit/5
        public IActionResult Edit(int id)
        {
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
        // POST: Randevular/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("HastaID, DoktorID, RandevuTarihi, RandevuSaati")] Randevu randevu)
        {
            if (id != randevu.RandevuID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE Randevular SET HastaID=@HastaID,DoktorID=@DoktorID, RandevuTarihi=@RandevuTarihi, RandevuSaati=@RandevuSaati   WHERE RandevuID=@RandevuID ", connection);
                    command.Parameters.AddWithValue("@HastaID", randevu.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", randevu.DoktorID);
                    command.Parameters.AddWithValue("@RandevuTarihi", randevu.RandevuTarihi);
                    command.Parameters.AddWithValue("@RandevuSaati", randevu.RandevuSaati);


                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(randevu);
        }

        // GET: Randevular/Delete/5
        public IActionResult Delete(int id)
        {
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
        public IActionResult DeleteConfirmed(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DELETE FROM Randevular WHERE RandevuID = @RandevuID", connection);
                command.Parameters.AddWithValue("@RandevuID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
