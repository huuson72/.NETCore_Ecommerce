using SV21T1020659.DomainModels;

namespace SV21T1020659.Shop.Models
{
    public class OrderViewModel
    {
        public Order Order { get; set; } // Thông tin đơn hàng chính
        public List<OrderDetail> OrderDetails { get; set; }
        public int OrderId { get; set; }
        public int CategoryID { get; set; }
        public List<Category> Categories { get; set; }
    }

}
