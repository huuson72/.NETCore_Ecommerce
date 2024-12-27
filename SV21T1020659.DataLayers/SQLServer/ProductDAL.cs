using Dapper;
using SV21T1020659.DomainModels;
using System.Data;

namespace SV21T1020659.DataLayers.SQLServer
{
    public class ProductDAL : BaseDAL, IProductDAL
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }

      /*   public int Add(Product data)
         {
             using (var connection = OpenConnection())
             {
                 var sql = @"
             INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
             VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
             SELECT CAST(SCOPE_IDENTITY() as int)";

                 return connection.ExecuteScalar<int>(sql, data);
             }
         }*/
        public int Add(Product data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from Products where ProductName = @ProductName)
                                select -1;
                            else
                                begin
                                    insert into Products(ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                                    values (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling)
                                    select SCOPE_IDENTITY()
                                end";
                var parameters = new
                {
                    ProductName = data.ProductName ?? "",
                    ProductDescription = data.ProductDescription ?? "",
                    SupplierID = data.SupplierID,
                    CategoryID = data.CategoryID,
                    Unit = data.Unit ?? "",
                    Price = data.Price,
                    Photo = data.Photo ?? "",
                    IsSelling = data.IsSelling
                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }

       /* public long AddAttribute(ProductAttribute data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from ProductAttributes where AttributeName = @AttributeName and ProductID = @ProductID)
                                select -1;
                            else
                                begin
                                    insert into ProductAttributes(ProductID, AttributeName, AttributeValue, DisplayOrder)
                                    values (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder)
                                    select SCOPE_IDENTITY()
                                end";
                var parameters = new
                {
                    ProductID = data.ProductID,
                    AttributeName = data.AttributeName ?? "",
                    AttributeValue = data.AttributeValue ?? "",
                    DisplayOrder = data.DisplayOrder,

                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }
*/
         public long AddAttribute(ProductAttribute data)
          {
              using (var connection = OpenConnection())
              {
                  var sql = @"
                            INSERT INTO ProductAttributes (ProductID, AttributeName, DisplayOrder, AttributeValue)
                            VALUES (@ProductID, @AttributeName, @DisplayOrder, @AttributeValue);
                            SELECT CAST(SCOPE_IDENTITY() as bigint)";

                  return connection.ExecuteScalar<long>(sql, data);
              }
          }

        public long AddPhoto(ProductPhoto data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"insert into ProductPhotos(ProductID, Photo, Description, DisplayOrder, IsHidden)
                            values (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden)
                            select SCOPE_IDENTITY()";
                var parameters = new
                {
                    ProductID = data.ProductID,
                    Photo = data.Photo ?? "",
                    Description = data.Description ?? "",
                    DisplayOrder = data.DisplayOrder,
                    IsHidden = data.IsHidden,

                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }
        /*public long AddPhoto(ProductPhoto data)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"
                          INSERT INTO ProductPhotos (ProductID, Description, Photo, DisplayOrder, IsHidden)
                          VALUES (@ProductID, @Description, @Photo, @DisplayOrder, @IsHidden);
                          SELECT CAST(SCOPE_IDENTITY() as bigint)";

                return connection.ExecuteScalar<long>(sql, data);
            }
        }
*/

        public int Count(string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            using (var connection = OpenConnection())
            {
                // Câu lệnh SQL để đếm số lượng sản phẩm thỏa mãn điều kiện tìm kiếm
                var sql = @"
            SELECT COUNT(*)
             FROM Products
             WHERE (@SearchValue = '' OR ProductName LIKE @SearchValue)
               AND (@CategoryID = 0 OR CategoryID = @CategoryID)
               AND (@SupplierID = 0 OR SupplierID = @SupplierID)
               AND (Price >= @MinPrice)
               AND (@MaxPrice = 0 OR Price <= @MaxPrice)";

                // Tham số cho câu lệnh SQL
                var parameters = new
                {
                    SearchValue = "%" + searchValue + "%", // Tìm kiếm với mẫu
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                // Thực thi câu lệnh SQL và trả về số lượng sản phẩm
                return connection.ExecuteScalar<int>(sql, parameters);
            }
        }


        public bool Delete(Product data)
        {
            using (var connection = OpenConnection())
            {
                var sql = "DELETE FROM Products WHERE ProductID = @ProductID";
                return connection.Execute(sql, new { ProductID = data.ProductID }) > 0;
            }
        }

        public bool DeleteAttribute(long attributeID)
        {
            using (var connection = OpenConnection())
            {
                var sql = "DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID";
                return connection.Execute(sql, new { AttributeID = attributeID }) > 0;
            }
        }
        public bool DeleteProductAttributes(long productID)
        {
            using (var connection = OpenConnection())
            {
                var deleteAttributesSql = "DELETE FROM ProductAttributes WHERE ProductID = @ProductID";
                return connection.Execute(deleteAttributesSql, new { ProductID = productID }) > 0;
            }
                
           
        }



        public bool DeletePhoto(ProductPhoto data)
        {
            using (var connection = OpenConnection())
            {
                var sql = "DELETE FROM ProductPhotos WHERE PhotoID = @PhotoID";
                return connection.Execute(sql, new { data.PhotoID }) > 0;
            }
        }


        public Product? Get(int productID)
        {
            using (var connection = OpenConnection())
            {
                var sql = "SELECT * FROM Products WHERE ProductID = @ProductID";
                return connection.QuerySingleOrDefault<Product>(sql, new { ProductID = productID });
            }
        }


        public ProductAttribute? GetAttribute(long attributeID)
        {
            using (var connection = OpenConnection())
            {
                var sql = "SELECT * FROM ProductAttributes WHERE AttributeID = @AttributeID";
                return connection.QuerySingleOrDefault<ProductAttribute>(sql, new { AttributeID = attributeID });
            }
        }


        public ProductPhoto? GetPhoto(long photoID)
        {
            using (var connection = OpenConnection())
            {
                var sql = "SELECT * FROM ProductPhotos WHERE PhotoID = @PhotoID";
                return connection.QuerySingleOrDefault<ProductPhoto>(sql, new { PhotoID = photoID });
            }
        }


        public bool InUsed(int productID)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"
            SELECT COUNT(*) 
            FROM OrderDetails 
            WHERE ProductID = @ProductID";

                // Thực thi truy vấn và trả về true nếu số lượng bản ghi lớn hơn 0
                int count = connection.ExecuteScalar<int>(sql, new { ProductID = productID });
                return count > 0; // Nếu có bản ghi thì sản phẩm đang được sử dụng
            }
        }


        public List<Product> List(int page = 1, int pageSize = 0,
            string searchValue = "", int categoryID = 0, int supplierID = 0,
            decimal minPrice = 0, decimal maxPrice = 0)

        {

            using (var connection = OpenConnection())
            {
                var sql = @"
                SELECT *
                FROM (
                    SELECT *,
                      ROW_NUMBER() OVER (ORDER BY ProductName) AS RowNumber
                    FROM Products
                    WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                          AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                          AND (@SupplierID = 0 OR SupplierId = @SupplierID)
                          AND (Price >= @MinPrice)
                          AND (@MaxPrice <= 0 OR Price <= @MaxPrice)
                ) AS t
                WHERE (@PageSize = 0)
                      OR (RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize)";

                var parameters = new
                {
                    SearchValue = "%" + searchValue + "%",
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Page = page,
                    PageSize = pageSize
                };

                return connection.Query<Product>(sql, parameters).AsList();
            }
        }

       

        public IList<ProductAttribute> ListAttributes(int productID)
        {
            using (var connection = OpenConnection())
            {
                var sql = "SELECT * FROM ProductAttributes WHERE ProductID = @ProductID ORDER BY DisplayOrder";
                return connection.Query<ProductAttribute>(sql, new { ProductID = productID }).AsList();
            }
        }
     


        public IList<ProductPhoto> ListPhotos(int productID)
        {
            using (var connection = OpenConnection())
            {
                var sql = "SELECT * FROM ProductPhotos WHERE ProductID = @ProductID ORDER BY DisplayOrder";
                return connection.Query<ProductPhoto>(sql, new { ProductID = productID }).AsList();
            }
        }

        public List<Product> ListProducts(string searchValue)
        {
            throw new NotImplementedException();
        }
        public bool Update(Product data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from Products where ProductID <> @ProductID and ProductName = @ProductName)
                            begin
                                update	Products
                                set		ProductName = @ProductName, 
                                        ProductDescription = @ProductDescription, 
                                        SupplierID = @SupplierID, 
                                        CategoryID = @CategoryID, 
                                        Unit = @Unit, 
                                        Price = @Price, 
                                        Photo = @Photo, 
                                        IsSelling = @IsSelling
                                where	ProductID = @ProductID
                            end";
                var parameters = new
                {
                    ProductID = data.ProductID,
                    ProductName = data.ProductName ?? "",
                    ProductDescription = data.ProductDescription,
                    SupplierID = data.SupplierID,
                    CategoryID = data.CategoryID,
                    Unit = data.Unit ?? "",
                    Price = data.Price,
                    Photo = data.Photo ?? "",
                    IsSelling = data.IsSelling,
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        /* public bool Update(Product data)
         {
             using (var connection = OpenConnection())
             {
                 var sql = @"
             UPDATE Products
             SET ProductName = @ProductName,
                 ProductDescription = @ProductDescription,
                 SupplierID = @SupplierID,
                 CategoryID = @CategoryID,
                 Unit = @Unit,
                 Price = @Price,
                 Photo = @Photo,
                 IsSelling = @IsSelling
             WHERE ProductID = @ProductID";

                 return connection.Execute(sql, data) > 0;
             }
         }*/


        public bool UpdateAttribute(ProductAttribute data)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"
                         UPDATE ProductAttributes
                         SET AttributeName = @AttributeName,
                           DisplayOrder = @DisplayOrder,
                           AttributeValue = @AttributeValue
                         WHERE AttributeID = @AttributeID";

                return connection.Execute(sql, data) > 0;
            }
        }

      /*  public bool UpdateAttribute(ProductAttribute data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from ProductAttributes where AttributeID <> @AttributeID and AttributeName = @AttributeName and ProductID = @ProductID)
                            begin
                                update	ProductAttributes
                                set		ProductID = @ProductID, 
                                        AttributeName = @AttributeName, 
                                        AttributeValue = @AttributeValue, 
                                        DisplayOrder = @DisplayOrder
                                where	AttributeID = @AttributeID
                            end";
                var parameters = new
                {
                    AttributeID = data.AttributeID,
                    ProductID = data.ProductID,
                    AttributeName = data.AttributeName ?? "",
                    AttributeValue = data.AttributeValue ?? "",
                    DisplayOrder = data.DisplayOrder,
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }*/
        public bool UpdatePhoto(ProductPhoto data)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"
                          UPDATE ProductPhotos
                          SET Description = @Description,
                            Photo = @Photo,
                            DisplayOrder = @DisplayOrder,
                            IsHidden = @IsHidden
                          WHERE PhotoID = @PhotoID";

                return connection.Execute(sql, data) > 0;
            }
        }

        public bool DeletePhotosByProductId(long productId)
        {
            using (var connection = OpenConnection())
            {
                var sql = "DELETE FROM ProductPhotos WHERE ProductID = @ProductID";
                return connection.Execute(sql, new { ProductID = productId }) > 0;
            }
        }
        /// <summary>
        /// Lấy danh sách ảnh của sản phẩm
        /// </summary>
        public List<ProductPhoto> GetPhotosByProductId(int productId)
        {
            using (var connection = OpenConnection())
            {
                var sql = "SELECT * FROM ProductPhotos WHERE ProductID = @ProductID";
                return connection.Query<ProductPhoto>(sql, new { ProductID = productId }).ToList();
            }
        }
    }
}
