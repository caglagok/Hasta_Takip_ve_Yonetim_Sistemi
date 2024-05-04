using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prolab22__3.Models
{
    public class RandevuViewModel
    {
        public string UzmanlikAlani { get; set; }
        public string Hastane { get; set; }
        public int DoktorID { get; set; }
        public DateTime RandevuTarihi { get; set; }
        public TimeSpan RandevuSaati { get; set; }
        public List<SelectListItem> UzmanlikAlanlari { get; set; }
        public List<SelectListItem> Hastaneler { get; set; }
        public List<SelectListItem> Doktorlar { get; set; }
    }
}
