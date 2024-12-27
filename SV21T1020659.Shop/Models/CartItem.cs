
using SV21T1020659.DomainModels;

namespace SV21T1020659.Shop.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";
        public int Quantity { get; set; } = 0;
        public string Unit { get; set; } = "";
        public decimal SalePrice { get; set; } = 0;
        public int? CategoryID { get; set; }
        public Product Product { get; set; }
        public decimal TotalPrice
        {
            get
            {
                return Quantity * SalePrice;
            }
        }
        
    }
}
