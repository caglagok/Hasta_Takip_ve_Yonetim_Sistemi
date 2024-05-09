namespace Prolab22__3.Models
{
    public class YoneticiDashboardViewModel
    {
        public IEnumerable<Yonetici> Yoneticiler { get; set; }
        public IEnumerable<Hasta> Hastalar { get; set; }
        public IEnumerable<Doktor> Doktorlar { get; set; }
        public IEnumerable<Randevu> Randevular { get; set; }
        public IEnumerable<TibbiRapor> TibbiRaporlar { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}