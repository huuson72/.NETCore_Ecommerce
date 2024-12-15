using SV21T1020659.DataLayers;
using SV21T1020659.DomainModels;

namespace SV21T1020629.BusinessLayers
{
    public static class CommonDataService
    {
        private static readonly ICommonDAL<Customer> customerDB;
        private static readonly ICommonDAL<Category> categoryDB;
        private static readonly ICommonDAL<Shipper> shipperDB;
        private static readonly ICommonDAL<Supplier> supplierDB;
        private static readonly ICommonDAL<Employee> employeeDB;
        private static readonly ISimpleQueryDAL<Province> provinceDB;  



        static CommonDataService()
        {
            //string connectionString = "server=.;user id=sa;password = 070203;database=DB_ASP;TrustServerCertificate=true";
            string connectionString = Configuration.ConnectionString;
            customerDB = new SV21T1020659.DataLayers.SQLServer.CustomerDAL(connectionString);
            categoryDB = new SV21T1020659.DataLayers.SQLServer.CategoryDAL(connectionString);
            shipperDB = new SV21T1020659.DataLayers.SQLServer.ShipperDAL(connectionString);
            supplierDB = new SV21T1020659.DataLayers.SQLServer.SupplierDAL(connectionString);
            employeeDB = new SV21T1020659.DataLayers.SQLServer.EmployeeDAL(connectionString);
            provinceDB = new SV21T1020659.DataLayers.SQLServer.ProvinceDAL(connectionString);


        }

        public static List<Category> ListOfCategories()
        {
            return categoryDB.List();
        }
        
public static List<Supplier> ListOfSuppliers()
        {
            return supplierDB.List();
        }
        /// <summary>
        /// Tìm kiếm lấy danh sách khách hàng dưới dạng phân trang
        /// </summary>
        /// <param name="rowCount">Số dòng tìm được</param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        /// 


        public static List<Customer> ListOfCustomers(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "")
        {
            rowCount = customerDB.Count(searchValue);
            return customerDB.List(page, pageSize, searchValue);
        }


        public static List<Customer> ListOfCustomers()
        {
            
            return customerDB.List();
        }
        /// <summary>
        /// Lay 1 khach hang co ma la ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Customer? GetCustomer(int id)
        {
            return customerDB.Get(id);
        }
        /// <summary>
        /// Bổ sung 1 khách hàng, hàm trả về id của khách hàng được bổ sung
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int AddCustomer(Customer data)
        {
            return customerDB.Add(data);
        }
        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool UpdateCustomer(Customer data)
        {
            return customerDB.Update(data);
        }
        /// <summary>
        /// Xóa khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool DeleteCustomer(int id)
        {
            return customerDB.Delete(id);
        }

        /// <summary>
        /// Kiểm tra khách hàng có đơn hàng liên quan hay không
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool InUsedCustomer(int id)
        {
            return customerDB.InUsed(id);
        }
        public static List<Category> ListOfCategories(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "")
        {
            rowCount = categoryDB.Count(searchValue);
            return categoryDB.List(page, pageSize, searchValue);

        }
       /* public static List<Category> ListOfCategories(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "")
        {
            rowCount = categoryDB.Count(searchValue); // Đảm bảo rowCount luôn được tính chính xác

            // Lấy danh sách các category, nếu không có kết quả, trả về danh sách trống thay vì null
            var categories = categoryDB.List(page, pageSize, searchValue);

            return categories ?? new List<Category>(); // Trả về danh sách trống nếu categories là null
        }*/

     /*   public static List<Supplier> ListOfSuppliers(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "")
        {
            rowCount = supplierDB.Count(searchValue); // Đảm bảo rowCount được tính chính xác

            var suppliers = supplierDB.List(page, pageSize, searchValue);

            return suppliers ?? new List<Supplier>(); // Trả về danh sách trống nếu suppliers là null
        }*/


        /// <summary>
        /// Lấy thông tin danh mục có mã ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Category? GetCategory(int id)
        {
            return categoryDB.Get(id);
        }


        /// <summary>
        /// Thêm một danh mục, trả về ID của danh mục được thêm
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int AddCategory(Category data)
        {
            return categoryDB.Add(data);
        }
        public static bool UpdateCategory(Category data)
        {
            return categoryDB.Update(data);
        }
        /// <summary>
        /// Xóa danh mục
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool DeleteCategory(int id)
        {
            return categoryDB.Delete(id);
        }
        /// <summary>
        /// Kiểm tra danh mục có đang được sử dụng hay không
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsCategoryInUse(int id)
        {
            return categoryDB.InUsed(id);
        }
            public static List<Shipper> ListOfShippers(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "")
            {
                rowCount = shipperDB.Count(searchValue);
                return shipperDB.List(page, pageSize, searchValue);
            }

        /// <summary>
        /// Lấy thông tin một nhà vận chuyển có mã là ID.
        /// </summary>
        /// <param name="id">ID của nhà vận chuyển.</param>
        /// <returns>Thông tin nhà vận chuyển nếu tìm thấy, ngược lại trả về null.</returns>
        public static Shipper? GetShipper(int id)
        {
            return shipperDB.Get(id);
        }
        /// <summary>
        /// Thêm một nhà cung cấp, trả về ID của nhà cung cấp được thêm
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int AddShiper(Shipper data)
        {
            return shipperDB.Add(data);
        }

        public static bool UpdateShipper(Shipper data)
        {
            return shipperDB.Update(data);
        }
        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool DeleteShipper(int id)
        {
            return shipperDB.Delete(id);
        }
        /// <summary>
        /// Kiểm tra nhà cung cấp có đang được sử dụng hay không
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsShipperInUse(int id)
        {
            return shipperDB.InUsed(id);
        }
        public static List<Supplier> ListOfSuppliers(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "")
        {
            rowCount = supplierDB.Count(searchValue);
            return supplierDB.List(page, pageSize, searchValue);
        }
        /// <summary>
        /// Lấy thông tin nhà cung cấp có mã ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Supplier? GetSupplier(int id)
        {
            return supplierDB.Get(id);
        }
        /// <summary>
        /// Thêm một nhà cung cấp, trả về ID của nhà cung cấp được thêm
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int AddSupplier(Supplier data)
        {
            return supplierDB.Add(data);
        }
        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool UpdateSupplier(Supplier data)
        {
            return supplierDB.Update(data);
        }
        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool DeleteSupplier(int id)
        {
            return supplierDB.Delete(id);
        }

        /// <summary>
        /// Kiểm tra nhà cung cấp có đang được sử dụng hay không
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsSupplierInUse(int id)
        {
            return supplierDB.InUsed(id);
        }

        public static List<Employee> ListOfEmployees(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "")
        {
            rowCount = employeeDB.Count(searchValue);
            return employeeDB.List(page, pageSize, searchValue);
        }
        /// <summary>
        /// Lấy thông tin danh mục có mã ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Employee? GetEmployee(int id)
        {
            return employeeDB.Get(id);
        }


        /// <summary>
        /// Thêm một danh mục, trả về ID của danh mục được thêm
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int AddEmployee(Employee data)
        {
            return employeeDB.Add(data);
        }
        public static bool UpdateEmployee(Employee data)
        {
            return employeeDB.Update(data);
        }
        /// <summary>
        /// Xóa danh mục
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool DeleteEmployee(int id)
        {
            return employeeDB.Delete(id);
        }
        /// <summary>
        /// Kiểm tra danh mục có đang được sử dụng hay không
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsEmployeeInUse(int id)
        {
            return employeeDB.InUsed(id);
        }


        /// <summary>
        /// Lấy danh sách toàn bộ các tỉnh thành
        /// </summary>
        /// <returns></returns>
        public static List<Province> ListOfProvinces()
        {
            return provinceDB.List();
        }

    }
}
