namespace Prolab22__3.Models
{
    public class HastaPagingViewModel
    {
        public IEnumerable<Hasta> Hastalar { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
