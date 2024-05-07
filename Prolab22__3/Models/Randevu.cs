namespace Prolab22__3.Models
{
    public class Randevu
    {

        public int RandevuID { get; set; }
        public DateTime RandevuTarihi { get; set; }
        public TimeSpan RandevuSaati { get; set; }
        public int HastaID { get; set; }
        public int DoktorID { get; set; }
        public string? HastaAdi { get; set; }
        public string? HastaSoyadi { get; set; }
        public Hasta? Hasta { get; set; }
        public Doktor? Doktor { get; set; }
        public string? DoktorAdi { get; set; }
    }
}
