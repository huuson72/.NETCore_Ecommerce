using SV21T1020659.DomainModels;

namespace SV21T1020659.Web.Models
{
    public class ProductViewModel
    {
        public ProductSearchInput SearchInput { get; set; }
        public ProductSearchResult SearchResult { get; set; }
        public List<Category> Categories { get; set; }
        public List<Supplier> Suppliers { get; set; }
    }

}
