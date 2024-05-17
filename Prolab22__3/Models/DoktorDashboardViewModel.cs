namespace Prolab22__3.Models
{
    public class DoktorDashboardViewModel
    {
        public Doktor Doktor { get; set; }
        public IEnumerable<Randevu> Randevular { get; set; }
        public IEnumerable<TibbiRapor> TibbiRaporlar { get; set; }
        public List<Bildirim> Bildirimler { get; set; }
    }
}
