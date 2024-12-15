using SV21T1020659.DomainModels;

namespace SV21T1020659.Web.Models
{
    public class OrderDetailModel
    {
        public Order? Order { get; set; }
        public required List<OrderDetail> Details { get; set; }
        public List<string> AvailableActions { get; set; }
        public List<Shipper> Shippers { get; set; }
    }
}
