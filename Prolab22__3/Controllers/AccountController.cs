using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Data;
namespace Prolab22__3.Controllers
{
    public class AccountController:Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public IActionResult LoginDoktor(int doktorID, string password)
        {
            if (IsValidUser(doktorID, password, "Doktorlar","DoktorID"))
                return RedirectToAction("Index", "Doktorlar");
            else
            {
                ViewBag.Error = "Invalid credentials for Doktor";
                return View("LoginDoktor");
            }
        }

        [HttpPost]
        public IActionResult LoginHasta(int hastaID, string password)
        {
            if (IsValidUser(hastaID, password, "Hastalar", "HastaID")) // 'Hastalar' tablosu ve 'HastaID' sütunu
                return RedirectToAction("Index", "Hastalar");
            else
            {
                ViewBag.Error = "Invalid credentials for Hasta";
                return View("LoginHasta");
            }
        }

        [HttpPost]
        public IActionResult LoginYonetici(int YoneticiID, string password)
        {
            if (IsValidUser(YoneticiID, password, "Yoneticiler","YoneticiID"))
                return RedirectToAction("Index", "Yoneticiler");
            else
            {
                ViewBag.Error = "Invalid credentials for Yonetici";
                return View("LoginYonetici");
            }
        }
        private bool IsValidUser(int id, string password, string tableName, string idColumn)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // SQL sorgusunu belirli bir tabloya ve sütuna göre hazırla
                string sql = $"SELECT COUNT(1) FROM {tableName} WHERE {idColumn} = @ID AND Password = @Password";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ID", id);
                command.Parameters.AddWithValue("@Password", password);

                connection.Open();
                int result = (int)command.ExecuteScalar();
                return result == 1;
            }
        }

    
    }
}
