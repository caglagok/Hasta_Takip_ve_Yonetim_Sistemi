namespace Prolab22__3.Models
{
    public class HastaDashboardViewModel
    {
        public int HastaID { get; set; }  // Bu satırı ekleyin
        public IEnumerable<Randevu> Randevular { get; set; }
        public IEnumerable<TibbiRapor> TibbiRaporlar { get; set; }
    }
}
