﻿using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Prolab22__3.Controllers
{
    public class DoktorlarController : Controller
    {
        private readonly string _connectionString;
        public DoktorlarController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }
        public IActionResult Index()
        {
            var doktorlar = new List<Doktor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT DoktorID, Ad, Soyad, UzmanlikAlani, CalistigiHastane, Password FROM Doktorlar", connection);
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
                            UzmanlikAlani = reader.GetString(3),
                            CalistigiHastane = reader.GetString(4),
                            Password = reader.GetString(5)
                        });
                    }
                }
            }
            return View(doktorlar);
        }
        public IActionResult LoginDoktor()
        {
            return View();
        }

        // POST: Doktorlar/LoginDoktor
        [HttpPost]
        public IActionResult LoginDoktor(string DoktorID, string password)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand("SELECT COUNT(1) FROM Doktorlar WHERE DoktorID = @DoktorID AND Password = @Password", connection))
                {
                    command.Parameters.Add("@DoktorID", SqlDbType.Int).Value = DoktorID; 
                    command.Parameters.Add("@Password", SqlDbType.NVarChar, 50).Value = password;
                    connection.Open();
                    int result = Convert.ToInt32(command.ExecuteScalar());

                    if (result == 1)
                    {
                        HttpContext.Session.SetInt32("DoktorID", Convert.ToInt32(DoktorID));
                        HttpContext.Session.SetString("Role", "Doktor");  
                        return RedirectToAction("Index", "DoktorInterface");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Kullanıcı ID veya şifre yanlış. Lütfen tekrar deneyin.";
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                return View();
            }
        }
        // GET: Doktorlar/Details/5
        public IActionResult Details(int id)
        {
            Doktor doktor = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT DoktorID, Ad, Soyad, UzmanlikAlani, CalistigiHastane, Password FROM Doktorlar WHERE DoktorID = @DoktorID", connection);
                command.Parameters.AddWithValue("@DoktorID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        doktor = new Doktor
                        {
                            DoktorID = reader.GetInt32(0),
                            Ad = reader.GetString(1),
                            Soyad = reader.GetString(2),
                            UzmanlikAlani = reader.GetString(3),
                            CalistigiHastane = reader.GetString(4),
                            Password = reader.GetString(5)
                        };
                    }
                }
            }
            if (doktor == null)
            {
                return NotFound();
            }

            var randevular = new List<Randevu>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT RandevuID, RandevuTarihi, RandevuSaati FROM Randevular WHERE DoktorID = @DoktorID", connection);
                command.Parameters.AddWithValue("@DoktorID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        randevular.Add(new Randevu
                        {
                            RandevuID = reader.GetInt32(0),
                            RandevuTarihi = reader.GetDateTime(1),
                            RandevuSaati = reader.GetTimeSpan(2)
                        });
                    }
                }
            }

            ViewBag.Randevular = randevular;

            return View(doktor);
        }
        // GET: Doktorlar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Doktorlar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Ad, Soyad, UzmanlikAlani, CalistigiHastane, Password")] Doktor doktor)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                { var command = new SqlCommand("INSERT INTO Doktorlar (Ad, Soyad, UzmanlikAlani, CalistigiHastane, Password) VALUES (@Ad, @Soyad, @UzmanlikAlani, @CalistigiHastane, @Password)", connection);

                    command.Parameters.AddWithValue("@Ad", doktor.Ad);
                    command.Parameters.AddWithValue("@Soyad", doktor.Soyad);
                    command.Parameters.AddWithValue("@UzmanlikAlani", doktor.UzmanlikAlani);
                    command.Parameters.AddWithValue("@CalistigiHastane", doktor.CalistigiHastane);
                    command.Parameters.AddWithValue("@Password", doktor.Password);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return RedirectToAction("Index", "YoneticiInterface");
            }
            return View(doktor);
        }
        //GET: Doktorlar/Edit/
        public IActionResult Edit(int id)
        {

            Doktor doktor = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT DoktorID, Ad, Soyad, UzmanlikAlani, CalistigiHastane, Password FROM Doktorlar WHERE DoktorID = @DoktorID", connection);
                command.Parameters.AddWithValue("@DoktorID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        doktor = new Doktor
                        {
                            DoktorID = reader.GetInt32(0),
                            Ad = reader.GetString(1),
                            Soyad = reader.GetString(2),
                            UzmanlikAlani = reader.GetString(3),
                            CalistigiHastane = reader.GetString(4),
                            Password = reader.GetString(5)
                        };
                    }
                }
            }
            if (doktor == null)
            {
                return NotFound();
            }
            TempData["PreviousUrl"] = Request.Headers["Referer"].ToString();
            return View(doktor);
        }

        // POST: Doktorlar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("DoktorID, Ad, Soyad, UzmanlikAlani, CalistigiHastane, Password")] Doktor doktor)
        {
            if (id != doktor.DoktorID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE Doktorlar SET Ad = @Ad, Soyad = @Soyad, UzmanlikAlani = @UzmanlikAlani, CalistigiHastane = @CalistigiHastane, Password = @Password WHERE DoktorID = @DoktorID", connection);

                    command.Parameters.AddWithValue("@DoktorID", doktor.DoktorID);
                    command.Parameters.AddWithValue("@Ad", doktor.Ad);
                    command.Parameters.AddWithValue("@Soyad", doktor.Soyad);
                    command.Parameters.AddWithValue("@UzmanlikAlani", doktor.UzmanlikAlani);
                    command.Parameters.AddWithValue("@CalistigiHastane", doktor.CalistigiHastane);
                    command.Parameters.AddWithValue("@Password", doktor.Password);

                    connection.Open();
                    command.ExecuteNonQuery();
                } 
                string previousUrl = TempData["PreviousUrl"] as string;
                if (!string.IsNullOrEmpty(previousUrl))
                {
                    return Redirect(previousUrl);
                }
                else
                {
                    return RedirectToAction("Index", "YoneticiInterface");
                }
            }
            return View(doktor);
        }
        

        // GET: 
        public IActionResult Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT DoktorID, Ad, Soyad FROM Doktorlar WHERE DoktorID = @DoktorID", connection);
                command.Parameters.AddWithValue("@DoktorID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var doktor = new Doktor
                        {
                            DoktorID = reader.GetInt32(0),
                            Ad = reader.GetString(1),
                            Soyad = reader.GetString(2)
                        };
                        return View(doktor);
                    }
                }
            }
            return NotFound();
        }


        // POST: 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DELETE FROM Doktorlar WHERE DoktorID = @DoktorID", connection);
                command.Parameters.AddWithValue("@DoktorID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("Index", "YoneticiInterface");
        }

    }
}
