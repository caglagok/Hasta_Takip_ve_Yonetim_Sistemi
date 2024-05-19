namespace Prolab22__3.Models
{
    public class HastaDashboardViewModel
    {
        public int HastaID { get; set; }  
        public IEnumerable<Randevu> Randevular { get; set; }
        public IEnumerable<TibbiRapor> TibbiRaporlar { get; set; }
        public List<Bildirim> Bildirimler { get; set; } 
    }
}
