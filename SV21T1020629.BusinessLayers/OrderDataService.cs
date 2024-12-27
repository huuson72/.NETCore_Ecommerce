using SV21T1020659.DataLayers.SQLServer;
using SV21T1020659.DataLayers;
using SV21T1020659.DomainModels;

namespace SV21T1020629.BusinessLayers
{
    public static class OrderDataService
    {
        private static readonly IOrderDAL orderDB;
        /// <summary>

        /// Ctor
        /// </summary>
        static OrderDataService()
        {
            orderDB = new OrderDAL(Configuration.ConnectionString);
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang
        /// </summary>
        public static List<Order> ListOrders(out int rowCount, int page = 1, int pageSize = 0,
        int status = 0, DateTime? fromTime = null, DateTime? toTime = null,

        string searchValue = "")
        {
            rowCount = orderDB.Count(status, fromTime, toTime, searchValue);
            return orderDB.List(page, pageSize, status, fromTime, toTime, searchValue).ToList();
        }
        /// <summary>
        /// Lấy thông tin của 1 đơn hàng
        /// </summary>
        //public static Order? GetOrder(int orderID)
        //{
        //    return orderDB.Get(orderID);
        //}
        public static Order? GetOrder(int orderId)
        {
            Console.WriteLine($"OrderDataService: Fetching Order with ID: {orderId}");
            var order = orderDB.Get(orderId);
            if (order == null)
            {
                Console.WriteLine("OrderDataService: No order found!");
            }
            return order;
        }

        /// <summary>
        /// Khởi tạo 1 đơn hàng mới (tạo đơn hàng mới ở trạng thái Init).
        /// Hàm trả về mã của đơn hàng được tạo mới
        /// </summary>
        /// 
        public static int InitOrder(int? employeeID, int customerID, string deliveryProvince, string deliveryAddress, IEnumerable<OrderDetail> details)
        {
            // Kiểm tra dữ liệu đầu vào
            if (details == null || !details.Any() || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
                return 0;

            try
            {
                // Tạo đối tượng đơn hàng
                Order data = new Order()
                {
                    EmployeeID = employeeID > 0 ? employeeID : null, // Sử dụng NULL nếu EmployeeID không hợp lệ
                    CustomerID = customerID,
                    DeliveryProvince = deliveryProvince,
                    DeliveryAddress = deliveryAddress,
                    OrderTime = DateTime.Now,
                    Status = 1 // Trạng thái mặc định: Đơn hàng vừa gửi
                };

                // Thêm đơn hàng vào cơ sở dữ liệu
                int orderID = orderDB.Add(data);

                // Nếu tạo đơn hàng thành công, thêm chi tiết
                if (orderID > 0)
                {
                    foreach (var item in details)
                    {
                        try
                        {
                            orderDB.SaveDetail(orderID, item.ProductID, item.Quantity, item.SalePrice);
                        }
                        catch (Exception detailEx)
                        {
                           
                            return -1; // Có lỗi khi lưu chi tiết
                        }
                    }
                    return orderID;
                }

                return -1; // Lỗi khi thêm đơn hàng
            }
            catch (Exception ex)
            {
               
                return -1;
            }
        }



        public static int InitOrderCustomer(int? employeeID, int customerID, string deliveryProvince, string deliveryAddress, IEnumerable<OrderDetail> details, string phoneNumber)
        {
            // Kiểm tra dữ liệu đầu vào
            if (details == null || !details.Any() || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
                return 0;

            try
            {
                // Tạo đối tượng đơn hàng
                Order data = new Order()
                {
                    EmployeeID = employeeID > 0 ? employeeID : null, // Sử dụng NULL nếu EmployeeID không hợp lệ
                    CustomerID = customerID,
                    DeliveryProvince = deliveryProvince,
                    DeliveryAddress = deliveryAddress,
                    PhoneNumber = phoneNumber, // Gán số điện thoại vào đối tượng Order
                    OrderTime = DateTime.Now,
                    Status = 1 // Trạng thái mặc định: Đơn hàng vừa gửi
                };

                // Thêm đơn hàng vào cơ sở dữ liệu
                int orderID = orderDB.AddOrderCustomer(data);

                // Nếu tạo đơn hàng thành công, thêm chi tiết
                if (orderID > 0)
                {
                    foreach (var item in details)
                    {
                        try
                        {
                            orderDB.SaveDetail(orderID, item.ProductID, item.Quantity, item.SalePrice);
                        }
                        catch (Exception detailEx)
                        {
                            return -1; // Có lỗi khi lưu chi tiết
                        }
                    }
                    return orderID;
                }

                return -1; // Lỗi khi thêm đơn hàng
            }
            catch (Exception ex)
            {
                return -1;
            }
        }



        //public static int InitOrder(int employeeID, int customerID,

        //string deliveryProvince, string deliveryAddress,
        //IEnumerable<OrderDetail> details)

        //{
        //    if (details.Count() == 0)
        //        return 0;
        //    Order data = new Order()
        //    {
        //        //EmployeeID = employeeID,
        //        EmployeeID = employeeID > 0 ? employeeID : (int?)null, // Sử dụng NULL nếu EmployeeID = 0
        //        CustomerID = customerID,
        //        DeliveryProvince = deliveryProvince,
        //        DeliveryAddress = deliveryAddress
        //    };
        //    int orderID = orderDB.Add(data);
        //    if (orderID > 0)
        //    {
        //        foreach (var item in details)
        //        {
        //            orderDB.SaveDetail(orderID, item.ProductID, item.Quantity, item.SalePrice);
        //        }
        //        return orderID;
        //    }
        //    return 0;
        //}
        /// <summary>
        /// Hủy bỏ đơn hàng
        /// </summary>
        public static bool CancelOrder(int orderID)
        {
            Order? data = orderDB.Get(orderID);
            if (data == null)
                return false;
            if (data.Status != Constants.ORDER_FINISHED)
            {
                data.Status = Constants.ORDER_CANCEL;

                data.FinishedTime = DateTime.Now;
                return orderDB.Update(data);
            }
            return false;
        }
        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        public static bool RejectOrder(int orderID)
        {
            Order? data = orderDB.Get(orderID);
            if (data == null)
                return false;
            if (data.Status == Constants.ORDER_INIT || data.Status == Constants.ORDER_ACCEPTED)
            {
                data.Status = Constants.ORDER_REJECTED;
                data.FinishedTime = DateTime.Now;
                return orderDB.Update(data);
            }
            return false;
        }
        /// <summary>
        /// Duyệt chấp nhận đơn hàng
        /// </summary>
        public static bool AcceptOrder(int orderID)
        {
            Order? data = orderDB.Get(orderID);
            if (data == null)
                return false;
            if (data.Status == Constants.ORDER_INIT)
            {
                data.Status = Constants.ORDER_ACCEPTED;
                data.AcceptTime = DateTime.Now;
                return orderDB.Update(data);
            }
            return false;
        }
        /// <summary>
        /// Xác nhận đã chuyển hàng
        /// </summary>
        public static bool ShipOrder(int orderID, int shipperID)
        {
            Order? data = orderDB.Get(orderID);
            if (data == null)
                return false;
            if (data.Status == Constants.ORDER_ACCEPTED || data.Status == Constants.ORDER_SHIPPING)
            {
                data.Status = Constants.ORDER_SHIPPING;
                data.ShipperID = shipperID;
                data.ShippedTime = DateTime.Now;
                return orderDB.Update(data);
            }
            return false;
        }
        /// <summary>
        /// Ghi nhận kết thúc quá trình xử lý đơn hàng thành công
        /// </summary>
        public static bool FinishOrder(int orderID)
        {
            Order? data = orderDB.Get(orderID);

            if (data == null)
                return false;
            if (data.Status == Constants.ORDER_SHIPPING)
            {
                data.Status = Constants.ORDER_FINISHED;
                data.FinishedTime = DateTime.Now;
                return orderDB.Update(data);
            }
            return false;
        }
        /// <summary>
        /// Xóa đơn hàng và toàn bộ chi tiết của đơn hàng
        /// </summary>
        public static bool DeleteOrder(int orderID)
        {
            var data = orderDB.Get(orderID);
            if (data == null)
                return false;
            if (data.Status == Constants.ORDER_INIT
            || data.Status == Constants.ORDER_CANCEL
            || data.Status == Constants.ORDER_REJECTED)

                return orderDB.Delete(orderID);
            return false;
        }
        /// <summary>
        /// Lấy danh sách các mặt hàng được bán trong đơn hàng
        /// </summary>
        public static List<OrderDetail> ListOrderDetails(int orderID)
        {
            return orderDB.ListDetails(orderID).ToList();
        }

        /// <summary>
        /// Lấy thông tin của 1 mặt hàng được bán trong đơn hàng
        /// </summary>
        public static OrderDetail? GetOrderDetail(int orderID, int productID)
        {
            return orderDB.GetDetail(orderID, productID);
        }
      


        /// <summary>
        /// Lưu thông tin chi tiết của đơn hàng (thêm mặt hàng được bán trong đơn hàng)
        /// theo nguyên tắc:
        /// - Nếu mặt hàng chưa có trong chi tiết đơn hàng thì bổ sung
        /// - Nếu mặt hàng đã có trong chi tiết đơn hàng thì cập nhật lại số lượng và giá bán
        /// </summary>
        public static bool SaveOrderDetail(int orderID, int productID,
        int quantity, decimal salePrice)
        {
            Order? data = orderDB.Get(orderID);
            if (data == null)
                return false;
            if (data.Status == Constants.ORDER_INIT || data.Status == Constants.ORDER_ACCEPTED)
            {
                return orderDB.SaveDetail(orderID, productID, quantity, salePrice);
            }
            return false;
        }
        /// <summary>
        /// Xóa một mặt hàng ra khỏi đơn hàng
        /// </summary>

        public static bool DeleteOrderDetail(int orderID, int productID)
        {
            Order? data = orderDB.Get(orderID);
            if (data == null)
                return false;
            if (data.Status == Constants.ORDER_INIT || data.Status == Constants.ORDER_ACCEPTED)
            {
                return orderDB.DeleteDetail(orderID, productID);
            }
            return false;
        }
        public static List<Order> GetOrdersByCustomerId(int customerId)
        {
            // Kiểm tra giá trị hợp lệ của CustomerID
            if (customerId <= 0)
            {
                return new List<Order>(); // Trả về danh sách rỗng nếu CustomerID không hợp lệ
            }

            // Gọi hàm từ DAL để lấy danh sách đơn hàng
            return orderDB.GetOrdersByCustomerId(customerId);
        }

    }
}
