using SV21T1020659.DomainModels;

namespace SV21T1020659.Web.Models
{
    public class ProductSearchResult : PaginationSearchResult
    {
        public int CategoryID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;


        public required List<Product> Data { get; set; }
        //public required List<Category> Categories { get; set; } 
        //public required List<Supplier> Suppliers { get; set; }
        public int CurrentPage { get; set; } = 1; 

    }
}
