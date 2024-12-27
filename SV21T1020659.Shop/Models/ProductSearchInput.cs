using SV21T1020659.DomainModels;

namespace SV21T1020659.Shop.Models
{
    public class ProductSearchInput : PaginationSearchInput
    {
        public int CategoryID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;
     
        //public string Province { get; set; } // Thêm thuộc tính tỉnh thành
        //public List<Customer> Customers { get; set; } // Thêm danh sách khách hàng

    }
}
