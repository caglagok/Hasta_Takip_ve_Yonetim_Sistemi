namespace Prolab22__3.Models
{
    public class Bildirim
    {
        public int BildirimID { get; set; }
        public int KullaniciID { get; set; }
        public string Mesaj { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public bool Okundu { get; set; }
        public string Role { get; set; } // Bildirimin gideceği rol (hasta veya doktor)
    }
}
