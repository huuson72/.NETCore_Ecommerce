using SV21T1020659.DomainModels;

namespace SV21T1020659.Shop.Models
{
    public class PaymentInfo
    {
        public string Province { get; set; }  // Tỉnh/Thành phố giao hàng
        public string Address { get; set; }  // Địa chỉ chi tiết giao hàng
        public string PhoneNumber { get; set; }  // Địa chỉ chi tiết giao hàng

        public List<Province> Provinces { get; set; }
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Province) && !string.IsNullOrWhiteSpace(Address);
        }
    }

}
