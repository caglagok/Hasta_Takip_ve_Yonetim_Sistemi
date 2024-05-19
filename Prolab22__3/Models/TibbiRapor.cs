namespace Prolab22__3.Models
{
    public class TibbiRapor
    {
        public int? RaporID { get; set; }  
        public int? HastaID { get; set; }
        public int? DoktorID { get; set; }
        public DateTime RaporTarihi { get; set; }
        public string? RaporIcerigi { get; set; }
        public string? URL { get; set; } 
        public Hasta? Hasta { get; set; }
        public Doktor? Doktor { get; set; }
    }
}
