


using SV21T1020629.BusinessLayers;
using SV21T1020659.DataLayers;
using SV21T1020659.DataLayers.SQLServer;
using SV21T1020659.DomainModels;

namespace SV21T1020659.BusinessLayers
{
    public static class ProductDataService
    {
        public static readonly IProductDAL productDB;

        /// <summary>
        ///Ctor
        /// </summary>
        static ProductDataService()
        {
            productDB = new ProductDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng không phân trang
        /// </summary>
        public static List<Product> ListProducts(string searchValue = "")
        {
            return productDB.ListProducts(searchValue);
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng theo phân trang
        /// </summary>
        /// 
        public static List<Product> ListProducts(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            rowCount = productDB.Count(searchValue, categoryID, supplierID, minPrice, maxPrice);
            return productDB.List(page, pageSize, searchValue, categoryID, supplierID, minPrice, maxPrice);
        }
        /*  public static List<Product> ListProducts(int page, int pageSize, int pageSize1, string searchValue, int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
          {
              return productDB.List(page, pageSize, searchValue, categoryID, supplierID, minPrice, maxPrice);
          }
  */
        /// <summary>
        /// Đếm số lượng mặt hàng theo điều kiện tìm kiếm
        /// </summary>
        public static int CountProducts(string searchValue, int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            return productDB.Count(searchValue, categoryID, supplierID, minPrice, maxPrice);
        }

        /// <summary>
        /// Lấy thông tin mặt hàng theo ID
        /// </summary>
        public static Product? GetProduct(int productID)
        {
            return productDB.Get(productID);
        }

        /// <summary>
        /// Thêm mới mặt hàng
        /// </summary>
        public static int AddProduct(Product data)
        {
            return productDB.Add(data);
        }

        /// <summary>
        /// Cập nhật thông tin mặt hàng
        /// </summary>
        public static bool UpdateProduct(Product data)
        {
            return productDB.Update(data);
        }

        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        public static bool DeleteProduct(int productID)
        {
            var product = productDB.Get(productID);
            if (product != null && !productDB.InUsed(productID))
            {
                return productDB.Delete(product);
            }
            return false;
        }


        public static bool InUsedProduct(int productID)
        {
            return productDB.InUsed(productID);
        }

        /// <summary>
        /// Lấy danh sách ảnh của mặt hàng
        /// </summary>
        public static List<ProductPhoto> ListPhotos(int productID)
        {
            // Gọi phương thức ListPhotos từ DAL và chuyển đổi kiểu trả về thành List<ProductPhoto>
            IList<ProductPhoto> photos = productDB.ListPhotos(productID);
            return photos.ToList(); // Chuyển đổi IList thành List<ProductPhoto>
        }


        public static ProductPhoto? GetPhoto(long photoID)
        {
            return productDB.GetPhoto(photoID);
        }

        public static long AddPhoto(ProductPhoto data)
        {
            return productDB.AddPhoto(data);
        }

        public static bool UpdatePhoto(ProductPhoto data)
        {
            return productDB.UpdatePhoto(data);
        }

        public static bool DeletePhoto(long photoID)
        {
            var photo = productDB.GetPhoto(photoID);
            return photo != null && productDB.DeletePhoto(photo);
        }

        public static List<ProductAttribute> ListAttributes(int productID)
        {
            return (List<ProductAttribute>)productDB.ListAttributes(productID);
        }

        public static ProductAttribute? GetAttribute(int attributeID)
        {
            return productDB.GetAttribute(attributeID);
        }

        public static long AddAttribute(ProductAttribute data)
        {
            return productDB.AddAttribute(data);
        }

        public static bool UpdateAttribute(ProductAttribute data)
        {
            return productDB.UpdateAttribute(data);
        }

        public static bool DeleteAttribute(long attributeID)
        {
            return productDB.DeleteAttribute(attributeID);
        }

        /// <summary>
        /// Lấy thông tin ảnh của mặt hàng theo mã ảnh
        /// </summary>
        public static ProductPhoto? GetProductPhoto(long photoID)
        {
            return productDB.GetPhoto(photoID);
        }

        /// <summary>
        /// Thêm mới ảnh cho mặt hàng
        /// </summary>
        public static long AddProductPhoto(ProductPhoto data)
        {
            return productDB.AddPhoto(data);
        }

        /// <summary>
        /// Cập nhật ảnh của mặt hàng
        /// </summary>
        public static bool UpdateProductPhoto(ProductPhoto data)
        {
            return productDB.UpdatePhoto(data);
        }

        /// <summary>
        /// Xóa ảnh của mặt hàng
        /// </summary>
        public static bool DeleteProductPhoto(long productID)
        {

            return productDB.DeletePhotosByProductId(productID);
        }
       
        /// <summary>
        /// Lấy danh sách thuộc tính của ảnh
        /// </summary>
        public static IList<ProductAttribute> ListProductAttributes(int photoID)
        {
            return productDB.ListAttributes(photoID);
        }

        /// <summary>
        /// Lấy thông tin thuộc tính của ảnh theo ID
        /// </summary>
        public static ProductAttribute? GetProductAttribute(long attributeID)
        {
            return productDB.GetAttribute(attributeID);
        }

        /// <summary>
        /// Thêm mới thuộc tính cho ảnh
        /// </summary>
        public static long AddProductAttribute(ProductAttribute data)
        {
            return productDB.AddAttribute(data);
        }

        /// <summary>
        /// Cập nhật thuộc tính của ảnh
        /// </summary>
        public static bool UpdateProductAttribute(ProductAttribute data)
        {
            return productDB.UpdateAttribute(data);
        }
      
        /// <summary>
        /// Xóa thuộc tính của ảnh
        /// </summary>
        public static bool DeleteProductAttribute(long productID)
        {
            return productDB.DeleteProductAttributes(productID);
        }

        public static void DeleteProductPhotosByProductId(int productId)
        {
            
            productDB.DeletePhotosByProductId(productId);
        }

        public static bool ProductExists(int productId)
        {
            var product = productDB.Get(productId); 
            return product != null;
        }

    }
}
