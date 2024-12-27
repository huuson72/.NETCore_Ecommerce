using SV21T1020659.DomainModels;

namespace SV21T1020659.Shop.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public  List<Product> Data { get; set; }
        public List<ProductPhoto> Photos { get; set; } = new List<ProductPhoto>();
        public decimal Price => Product.Price;  // Giá bán
        public int ProductID => Product.ProductID;  // ID sản phẩm
        public int? CategoryID { get; set; }
        public IList<ProductAttribute> Attributes { get; set; }  // Danh sách thuộc tính sản phẩm
    }

}
