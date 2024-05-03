using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Prolab22__3.Controllers
{
    public class YoneticiController:Controller
    {
        private readonly string _connectionString;

        public YoneticiController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Yoneticiler
        public IActionResult Index()
        {
            var yoneticiler = new List<Yonetici>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT YoneticiID, Ad, Soyad, Password FROM Yoneticiler", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yoneticiler.Add(new Yonetici
                        {
                            YoneticiID = reader.GetInt32(0),
                            Ad = reader.GetString(1),
                            Soyad = reader.GetString(2),
                            Password = reader.GetString(3)
                        });
                    }
                }
            }
            return View(yoneticiler);
        }


        // GET: Yoneticiler/Login
        public IActionResult LoginYonetici()
        {
            return View();
        }

        // POST: Yoneticiler/Login
        [HttpPost]
        public IActionResult LoginYonetici(int YoneticiID, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT COUNT(1) FROM Yoneticiler WHERE YoneticiID = @YoneticiID AND Password = @Password ", connection);
                command.Parameters.AddWithValue("@YoneticiID", YoneticiID);
                command.Parameters.AddWithValue("@Password", password);

                int result = Convert.ToInt32(command.ExecuteScalar());

                if (result == 1)
                {
                    // Eğer kullanıcı bulunduysa ve bilgiler doğruysa, Hasta'nın ana sayfasına yönlendir.
                    return RedirectToAction("Index");
                }
                else
                {
                    // Eğer kullanıcı bilgileri yanlışsa veya bulunamadıysa, hata mesajı ile login sayfasına geri dön.
                    ViewBag.ErrorMessage = "Kullanıcı ID veya şifre yanlış. Lütfen tekrar deneyin.";
                    return View();
                }
            }
        }
    }
    }
       
   
