namespace SV21T1020659.DomainModels
{
    public class Supplier
    {
        public int SupplierID { get; set; } // ID của nhà cung cấp
        public string SupplierName { get; set; } = string.Empty; // Tên nhà cung cấp
        public string ContactName { get; set; } = string.Empty; // Tên người liên hệ
        public string Province { get; set; } = string.Empty; // Tỉnh
        public string Address { get; set; } = string.Empty; // Địa chỉ
        public string Phone { get; set; } = string.Empty; // Số điện thoại
        public string Email { get; set; } = string.Empty; // Địa chỉ email
    }
}
