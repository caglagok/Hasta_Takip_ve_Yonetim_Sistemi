using Microsoft.AspNetCore.Mvc;
using Prolab22__3.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Prolab22__3.Controllers
{
    public class YoneticiInterfaceController : Controller
    {
        private readonly string _connectionString;
        public YoneticiInterfaceController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index(int page = 1)
        {

            int pageSize = 50; 
            List<Hasta> hastalar = new List<Hasta>();
            List<Doktor> doktorlar = new List<Doktor>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
              
                SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Hastalar", connection);
                int totalHastaCount = (int)countCmd.ExecuteScalar();

              
                var hastaCommand = new SqlCommand($@"
                SELECT *
                FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY HastaID) AS RowNum, *
                    FROM Hastalar
                ) AS RowConstrainedResult
                WHERE RowNum >= {(page - 1) * pageSize + 1}
                AND RowNum < {page * pageSize + 1}
                ORDER BY RowNum", connection);

                

                using (var reader = hastaCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hastalar.Add(new Hasta
                        {
                            HastaID = reader.GetInt32(reader.GetOrdinal("HastaID")),
                            Ad = reader.GetString(reader.GetOrdinal("Ad")),
                            Soyad = reader.GetString(reader.GetOrdinal("Soyad")),
                           
                        });
                    }
                }

           
                var doktorCommand = new SqlCommand($@"
                SELECT *
                FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY DoktorID) AS RowNum, *
                    FROM Doktorlar
                ) AS RowConstrainedResult
                WHERE RowNum >= {(page - 1) * pageSize + 1}
                AND RowNum < {page * pageSize + 1}
                ORDER BY RowNum", connection);

                using (var reader = doktorCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        doktorlar.Add(new Doktor
                        {
                            DoktorID = reader.GetInt32(reader.GetOrdinal("DoktorID")),
                            Ad = reader.GetString(reader.GetOrdinal("Ad")),
                            Soyad = reader.GetString(reader.GetOrdinal("Soyad")),
                            UzmanlikAlani = reader.GetString(reader.GetOrdinal("UzmanlikAlani")),
                            
                        });
                    }
                }

                int totalPages = (int)Math.Ceiling(totalHastaCount / (double)pageSize);

                var model = new YoneticiDashboardViewModel
                {
                    Hastalar = hastalar,
                    Doktorlar = doktorlar,
                    CurrentPage = page,
                    TotalPages = totalPages
                };
  
            return View(model);
            }
    }
        private List<Hasta> GetHastalar()
        {
            var hastalar = new List<Hasta>(); 
            using (var connection = new SqlConnection(_connectionString)) 
            {
              
                var command = new SqlCommand("SELECT HastaID, Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres FROM Hastalar", connection);
                connection.Open();
                using (var reader = command.ExecuteReader()) 
                {
                    while (reader.Read()) 
                    {
                        hastalar.Add(new Hasta 
                        {
                            HastaID = reader.GetInt32(reader.GetOrdinal("HastaID")),
                            Ad = reader.IsDBNull(reader.GetOrdinal("Ad")) ? null : reader.GetString(reader.GetOrdinal("Ad")),
                            Soyad = reader.IsDBNull(reader.GetOrdinal("Soyad")) ? null : reader.GetString(reader.GetOrdinal("Soyad")),
                            DogumTarihi = reader.IsDBNull(reader.GetOrdinal("DogumTarihi")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DogumTarihi")),
                            Cinsiyet = reader.IsDBNull(reader.GetOrdinal("Cinsiyet")) ? null : reader.GetString(reader.GetOrdinal("Cinsiyet")),
                            TelefonNumarasi = reader.IsDBNull(reader.GetOrdinal("TelefonNumarasi")) ? null : reader.GetString(reader.GetOrdinal("TelefonNumarasi")),
                            Adres = reader.IsDBNull(reader.GetOrdinal("Adres")) ? null : reader.GetString(reader.GetOrdinal("Adres"))
                        });
                    }
                }
            }
            return hastalar; 
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
                            Ad = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Soyad = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            UzmanlikAlani = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                        });
                    }
                }
            }
            return doktorlar;
        }
        // GET: YoneticiInterface/HastaEkle
        public IActionResult HastaEkle()
        {
            return View();
        }
        // POST: YoneticiInterface/HastaEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HastaEkle(Hasta hasta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        var command = new SqlCommand("INSERT INTO Hastalar (Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, Password) VALUES (@Ad, @Soyad, @DogumTarihi, @Cinsiyet, @TelefonNumarasi, @Adres, @Password)", connection);
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
                    ViewBag.SuccessMessage = "Hasta başarıyla eklendi.";
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Hasta eklenirken bir hata oluştu. Lütfen tekrar deneyin.";
                }
            }
            return View();
        }
        // GET: YoneticiInterface/HastaSil
        public IActionResult HastaSil()
        {
            return View();
        }

        // POST: YoneticiInterface/HastaSil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HastaSil(int hastaID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("DELETE FROM Hastalar WHERE HastaID = @HastaID", connection);
                    command.Parameters.AddWithValue("@HastaID", hastaID);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        ViewBag.SuccessMessage = "Hasta başarıyla silindi.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Belirtilen HastaID ile eşleşen hasta bulunamadı.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Hasta silinirken bir hata oluştu. Lütfen tekrar deneyin.";
            }
            return View();
        }
        // GET: YoneticiInterface/DoktorEkle
        public IActionResult DoktorEkle()
        {
            return View();
        }
        // POST: YoneticiInterface/DoktorEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoktorEkle(Doktor doktor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        var command = new SqlCommand("INSERT INTO Doktorlar (Ad, Soyad, UzmanlikAlani, CalistigiHastane, Password) VALUES (@Ad, @Soyad, @UzmanlikAlani, @CalistigiHastane, @Password)", connection);
                        command.Parameters.AddWithValue("@Ad", doktor.Ad);
                        command.Parameters.AddWithValue("@Soyad", doktor.Soyad);
                        command.Parameters.AddWithValue("@UzmanlikAlani", doktor.UzmanlikAlani);
                        command.Parameters.AddWithValue("@CalistigiHastane", doktor.CalistigiHastane);
                        command.Parameters.AddWithValue("@Password", doktor.Password);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    ViewBag.SuccessMessage = "Doktor başarıyla eklendi.";
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Doktor eklenirken bir hata oluştu. Lütfen tekrar deneyin.";
                }
            }
            return View();
        }

        // GET: YoneticiInterface/DoktorSil
        public IActionResult DoktorSil()
        {
            return View();
        }

        // POST: YoneticiInterface/DoktorSil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoktorSil(int doktorID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Aktif randevusu olan bir doktoru silememe kontrolü
                    var commandCheck = new SqlCommand("SELECT COUNT(1) FROM Randevular WHERE DoktorID = @DoktorID", connection);
                    commandCheck.Parameters.AddWithValue("@DoktorID", doktorID);
                    connection.Open();
                    int appointmentCount = Convert.ToInt32(commandCheck.ExecuteScalar());
                    if (appointmentCount > 0)
                    {
                        ViewBag.ErrorMessage = "Aktif randevusu bulunan bir doktoru silemezsiniz.";
                        return View();
                    }

                    // Doktoru silme işlemi
                    var command = new SqlCommand("DELETE FROM Doktorlar WHERE DoktorID = @DoktorID", connection);
                    command.Parameters.AddWithValue("@DoktorID", doktorID);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        ViewBag.SuccessMessage = "Doktor başarıyla silindi.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Belirtilen DoktorID ile eşleşen doktor bulunamadı.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Doktor silinirken bir hata oluştu. Lütfen tekrar deneyin.";
            }
            return View();
        }
        
        // GET: YoneticiInterface/RandevuIptal
        public IActionResult RandevuIptal()
        {
            return View();
        }

        // POST: YoneticiInterface/RandevuIptal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RandevuIptal(int randevuID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("DELETE FROM Randevular WHERE RandevuID = @RandevuID", connection);
                    command.Parameters.AddWithValue("@RandevuID", randevuID);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        ViewBag.SuccessMessage = "Randevu başarıyla iptal edildi.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Belirtilen RandevuID ile eşleşen randevu bulunamadı.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Randevu iptal edilirken bir hata oluştu. Lütfen tekrar deneyin.";
            }
            return View();
        }

       
        // GET: YoneticiInterface/TibbiRaporEkle
        public IActionResult TibbiRaporEkle()
        {
            return View();
        }

        // POST: YoneticiInterface/TibbiRaporEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TibbiRaporEkle(TibbiRapor rapor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        var command = new SqlCommand("INSERT INTO TibbiRaporlar (RaporTarihi, RaporIcerigi, URL, HastaID, DoktorID) VALUES (@RaporTarihi, @RaporIcerigi, @URL, @HastaID, @DoktorID)", connection);
                        command.Parameters.AddWithValue("@RaporTarihi", rapor.RaporTarihi);
                        command.Parameters.AddWithValue("@RaporIcerigi", rapor.RaporIcerigi);
                        command.Parameters.AddWithValue("@URL", rapor.URL);
                        command.Parameters.AddWithValue("@HastaID", rapor.HastaID);
                        command.Parameters.AddWithValue("@DoktorID", rapor.DoktorID);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    ViewBag.SuccessMessage = "Tıbbi rapor başarıyla eklendi.";
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Tıbbi rapor eklenirken bir hata oluştu. Lütfen tekrar deneyin.";
                }
            }
            return View();
        }

        // GET: YoneticiInterface/HastaGuncelle
        public IActionResult HastaGuncelle()
        {
            return View();
        }

        // POST: YoneticiInterface/HastaGuncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HastaGuncelle(Hasta hasta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        var command = new SqlCommand("UPDATE Hastalar SET Ad = @Ad, Soyad = @Soyad, DogumTarihi = @DogumTarihi, Cinsiyet = @Cinsiyet, TelefonNumarasi = @TelefonNumarasi, Adres = @Adres, Password = @Password WHERE HastaID = @HastaID", connection);
                        command.Parameters.AddWithValue("@Ad", hasta.Ad);
                        command.Parameters.AddWithValue("@Soyad", hasta.Soyad);
                        command.Parameters.AddWithValue("@DogumTarihi", hasta.DogumTarihi);
                        command.Parameters.AddWithValue("@Cinsiyet", hasta.Cinsiyet);
                        command.Parameters.AddWithValue("@TelefonNumarasi", hasta.TelefonNumarasi);
                        command.Parameters.AddWithValue("@Adres", hasta.Adres);
                        command.Parameters.AddWithValue("@Password", hasta.Password);
                        command.Parameters.AddWithValue("@HastaID", hasta.HastaID);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    ViewBag.SuccessMessage = "Hasta bilgileri başarıyla güncellendi.";
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Hasta bilgileri güncellenirken bir hata oluştu. Lütfen tekrar deneyin.";
                }
            }
            return View();
        }
       
        // GET: YoneticiInterface/DoktorGuncelle
        public IActionResult DoktorGuncelle()
        {
            return View();
        }
        // POST: YoneticiInterface/DoktorGuncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoktorGuncelle(Doktor doktor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        var command = new SqlCommand("UPDATE Doktorlar SET Ad = @Ad, Soyad = @Soyad, UzmanlikAlani = @UzmanlikAlani, CalistigiHastane = @CalistigiHastane, Password = @Password WHERE DoktorID = @DoktorID", connection);
                        command.Parameters.AddWithValue("@Ad", doktor.Ad);
                        command.Parameters.AddWithValue("@Soyad", doktor.Soyad);
                        command.Parameters.AddWithValue("@UzmanlikAlani", doktor.UzmanlikAlani);
                        command.Parameters.AddWithValue("@CalistigiHastane", doktor.CalistigiHastane);
                        command.Parameters.AddWithValue("@Password", doktor.Password);
                        command.Parameters.AddWithValue("@DoktorID", doktor.DoktorID);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    ViewBag.SuccessMessage = "Doktor bilgileri başarıyla güncellendi.";
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Doktor bilgileri güncellenirken bir hata oluştu. Lütfen tekrar deneyin.";
                }
            }
            return View();
        }

        // GET: YoneticiInterface/RandevuGuncelle
        public IActionResult RandevuGuncelle()
        {
            return View();
        }

        // POST: YoneticiInterface/RandevuGuncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RandevuGuncelle(int randevuID, DateTime yeniRandevuTarihi)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE Randevular SET RandevuTarihi = @YeniRandevuTarihi WHERE RandevuID = @RandevuID", connection);
                    command.Parameters.AddWithValue("@YeniRandevuTarihi", yeniRandevuTarihi);
                    command.Parameters.AddWithValue("@RandevuID", randevuID);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        ViewBag.SuccessMessage = "Randevu bilgileri başarıyla güncellendi.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Belirtilen RandevuID ile eşleşen randevu bulunamadı.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Randevu bilgileri güncellenirken bir hata oluştu. Lütfen tekrar deneyin.";
            }
            return View();
        }

    
        // GET: YoneticiInterface/TibbiRaporGuncelle
        public IActionResult TibbiRaporGuncelle()
        {
            return View();
        }

        // POST: YoneticiInterface/TibbiRaporGuncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TibbiRaporGuncelle(int raporID, string yeniRaporIcerigi)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE TibbiRaporlar SET RaporIcerigi = @YeniRaporIcerigi WHERE RaporID = @RaporID", connection);
                    command.Parameters.AddWithValue("@YeniRaporIcerigi", yeniRaporIcerigi);
                    command.Parameters.AddWithValue("@RaporID", raporID);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        ViewBag.SuccessMessage = "Tıbbi rapor bilgileri başarıyla güncellendi.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Belirtilen RaporID ile eşleşen rapor bulunamadı.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Tıbbi rapor bilgileri güncellenirken bir hata oluştu. Lütfen tekrar deneyin.";
            }
            return View();
        }
    }
}
