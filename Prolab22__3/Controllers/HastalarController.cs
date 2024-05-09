﻿using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Prolab22__3.Controllers
{
    public class HastalarController : Controller
    {
        private readonly string _connectionString;

        public HastalarController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

        // GET: Hastalar
        public IActionResult Index(int page = 1)
        {
            int pageSize = 50;
            var hastalar = new List<Hasta>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Hastalar", connection))
                {
                    int count = (int)command.ExecuteScalar();
                    int totalPage = (int)Math.Ceiling(count / (double)pageSize);

                    command.CommandText = @"SELECT HastaID, Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, Password 
                                            FROM ( 
                                                SELECT *, ROW_NUMBER() OVER (ORDER BY HastaID) as RowNum 
                                                FROM Hastalar
                                            ) as HastalarWithRowNum
                                            WHERE RowNum BETWEEN @startRow AND @endRow";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@startRow", (page - 1) * pageSize + 1);
                    command.Parameters.AddWithValue("@endRow", page * pageSize);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hastalar.Add(new Hasta
                            {
                                HastaID = reader.GetInt32(0),
                                Ad = reader.GetString(1),
                                Soyad = reader.GetString(2),
                                DogumTarihi = reader.GetDateTime(3),
                                Cinsiyet = reader.GetString(4),
                                TelefonNumarasi = reader.GetString(5),
                                Adres = reader.GetString(6),
                                Password = reader.GetString(7)
                            });
                        }
                    }
                    ViewBag.TotalPages = totalPage;
                    ViewBag.CurrentPage = page;
                }
            }
            return View(hastalar);
        }
            // GET: Hastalar/LoginHasta
            public IActionResult LoginHasta()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginHasta(string HastaID, string password)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand("SELECT COUNT(1) FROM Hastalar WHERE HastaID = @HastaID AND Password = @Password", connection))
                {
                    command.Parameters.Add("@HastaID", SqlDbType.Int).Value = HastaID;
                    command.Parameters.Add("@Password", SqlDbType.NVarChar, 50).Value = password;
                    connection.Open();
                    int result = Convert.ToInt32(command.ExecuteScalar());

                    if (result == 1)
                    {
                        HttpContext.Session.SetInt32("HastaID", Convert.ToInt32(HastaID));
                        return RedirectToAction("Index", "HastaInterface");
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
                // Log the error (uncomment ex variable name and write a log.)
                ViewBag.ErrorMessage = "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                return View();
            }
        } 

        // GET: Hastalar/Details/5
        public IActionResult Details(int id)
        {
            Hasta hasta = null;
            List<TibbiRapor> raporlar = new List<TibbiRapor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                // Hasta bilgilerini çek
                var hastaCommand = new SqlCommand("SELECT HastaID, Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres FROM Hastalar WHERE HastaID = @HastaID", connection);
                hastaCommand.Parameters.AddWithValue("@HastaID", id);
                connection.Open();
                using (var reader = hastaCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hasta = new Hasta
                        {
                            HastaID = reader.GetInt32(0),
                            Ad = reader.GetString(1),
                            Soyad = reader.GetString(2),
                            DogumTarihi = reader.GetDateTime(3),
                            Cinsiyet = reader.GetString(4),
                            TelefonNumarasi = reader.GetString(5),
                            Adres = reader.GetString(6)
                        };
                    }
                }

                // Raporları çek
                var raporCommand = new SqlCommand("SELECT RaporID, RaporTarihi, RaporIcerigi, URL FROM TibbiRaporlar WHERE HastaID = @HastaID", connection);
                raporCommand.Parameters.AddWithValue("@HastaID", id);
                using (var reader = raporCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        raporlar.Add(new TibbiRapor
                        {
                            RaporID = reader.GetInt32(0),
                            RaporTarihi = reader.GetDateTime(1),
                            RaporIcerigi = reader.GetString(2),
                            URL = reader.GetString(3)
                        });
                    }
                }
            }

            ViewBag.TibbiRaporlar = raporlar;
            return View(hasta);
        }

        // GET: Hastalar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Hastalar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Ad,Soyad,DogumTarihi,Cinsiyet,TelefonNumarasi,Adres")] Hasta hasta)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("INSERT INTO Hastalar (Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, Password) VALUES (@Ad, @Soyad, @DogumTarihi, @Cinsiyet, @TelefonNumarasi, @Adres)", connection);
                    command.Parameters.AddWithValue("@Ad", hasta.Ad);
                    command.Parameters.AddWithValue("@Soyad", hasta.Soyad);
                    command.Parameters.AddWithValue("@DogumTarihi", hasta.DogumTarihi);
                    command.Parameters.AddWithValue("@Cinsiyet", hasta.Cinsiyet);
                    command.Parameters.AddWithValue("@TelefonNumarasi", hasta.TelefonNumarasi);
                    command.Parameters.AddWithValue("@Adres", hasta.Adres);
                    command.Parameters.AddWithValue("@Password", hasta.Password);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hasta);
        }

        // GET: Hastalar/Edit/5
        public IActionResult Edit(int id)
        {
            Hasta hasta = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT HastaID, Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, Password FROM Hastalar WHERE HastaID = @HastaID", connection);
                command.Parameters.AddWithValue("@HastaID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hasta = new Hasta
                        {
                            HastaID = reader.GetInt32(0),
                            Ad = reader.GetString(1),
                            Soyad = reader.IsDBNull(2) ? null : reader.GetString(2),
                            DogumTarihi = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                            Cinsiyet = reader.IsDBNull(4) ? null : reader.GetString(4),
                            TelefonNumarasi = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Adres = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Password = reader.IsDBNull(7) ? null : reader.GetString(7)
                        };
                    }
                }
            }
            if (hasta == null)
            {
                return NotFound();
            }
            return View(hasta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("HastaID,Ad,Soyad,DogumTarihi,Cinsiyet,TelefonNumarasi,Adres,Password")] Hasta hasta)
        {
            if (id != hasta.HastaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE Hastalar SET Ad = @Ad, Soyad = @Soyad, DogumTarihi = @DogumTarihi, Cinsiyet = @Cinsiyet, TelefonNumarasi = @TelefonNumarasi, Adres = @Adres, Password = @Password WHERE HastaID = @HastaID", connection);
                    command.Parameters.AddWithValue("@HastaID", hasta.HastaID);
                    command.Parameters.AddWithValue("@Ad", hasta.Ad);
                    command.Parameters.AddWithValue("@Soyad", hasta.Soyad);
                    command.Parameters.AddWithValue("@DogumTarihi", hasta.DogumTarihi);
                    command.Parameters.AddWithValue("@Cinsiyet", hasta.Cinsiyet);
                    command.Parameters.AddWithValue("@TelefonNumarasi", hasta.TelefonNumarasi);
                    command.Parameters.AddWithValue("@Adres", hasta.Adres);
                    command.Parameters.AddWithValue("@Password", hasta.Password);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hasta);
        }

        // GET: Hastalar/Delete/5
        public IActionResult Delete(int id)
        {
            Hasta hasta = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT HastaID, Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, Password FROM Hastalar WHERE HastaID = @HastaID", connection);
                command.Parameters.AddWithValue("@HastaID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hasta = new Hasta
                        {
                            HastaID = reader.GetInt32(0),
                            Ad = reader.GetString(1),
                            Soyad = reader.GetString(2),
                            DogumTarihi = reader.GetDateTime(3),
                            Cinsiyet = reader.GetString(4),
                            TelefonNumarasi = reader.GetString(5),
                            Adres = reader.GetString(6),
                            Password = reader.GetString(7)
                        };
                    }
                }
            }
            if (hasta == null)
            {
                return NotFound();
            }
            return View(hasta);
        }

        // POST: Hastalar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DELETE FROM Hastalar WHERE HastaID = @HastaID", connection);
                command.Parameters.AddWithValue("@HastaID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("Index", "YoneticiInterface");
        }
    }
}