using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.BusinessLayers;
using SV21T1020659.DomainModels;
using SV21T1020659.Web.Appcodes;
using SV21T1020659.Web.Models;
using System.Globalization;

namespace SV21T1020659.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.SALE}")]
    public class OrderController : Controller
    {
        public const string ORDER_SEARCH_CONDITION = "OrderSearchCondition";
        public const int PAGE_SIZE = 20;
        // Số mặt hàng được hiển thị trên một trang khi tìm kiếm mặt hàng để đưa vào đơn hàng
        private const int PRODUCT_PAGE_SIZE = 5;
        // Tên biến session lưu điều kiện tìm kiếm mặt hàng khi lập đơn hàng

        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchForSale";
        // Tên biến session lưu giỏ hàng
        private const string SHOPPING_CART = "ShoppingCart";
        public IActionResult Index()
        {
            ViewBag.StatusList = new Dictionary<int, string>
{
                         { 0, "-- Trạng thái --" },
                         { 1, "Đơn hàng mới (chờ duyệt)" },
                         { 2, "Đơn hàng đã duyệt (chờ chuyển hàng)" },
                         { 3, "Đơn hàng đang được giao" },
                         { 4, "Đơn hàng đã hoàn tất thành công" },
                         { -1, "Đơn hàng bị hủy" },
                         { -2, "Đơn hàng bị từ chối" }
                    };
            var condition = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH_CONDITION);
            if (condition == null)
            {
                var cultureInfo = new CultureInfo("en-US");
                condition = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    Status = 0,
                    TimeRange = $"{DateTime.Today.AddYears(-4).ToString("dd/MM/yyyy", cultureInfo)} - {DateTime.Today.ToString("dd/MM/yyyy", cultureInfo)}"
                };
            }
            return View(condition);
        }

        public IActionResult Search(OrderSearchInput condition)
        {
            int rowCount;
            var data = OrderDataService.ListOrders(out rowCount, condition.Page, condition.PageSize,
                                                   condition.Status, condition.FromTime, condition.ToTime, condition.SearchValue ?? "");

            // Lưu điều kiện tìm kiếm vào session
            ApplicationContext.SetSessionData(ORDER_SEARCH_CONDITION, condition);

            var model = new OrderSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                Status = condition.Status,
                TimeRange = condition.TimeRange,
                RowCount = rowCount,
                Data = data
            };

            return View(model);
        }


        public IActionResult Details(int id = 0)
        {
            var order = OrderDataService.GetOrder(id);
            if (order == null)
                return RedirectToAction("Index");

            var details = OrderDataService.ListOrderDetails(id);

            // Xác định các hành động khả dụng dựa trên trạng thái
            var availableActions = new List<string>();
            if (order.Status == Constants.ORDER_INIT )
            {
                availableActions.Add("Accept");
                availableActions.Add("Reject");
                availableActions.Add("Cancel");
            }
            else if (order.Status == Constants.ORDER_ACCEPTED)
            {
                availableActions.Add("Shipping");
                availableActions.Add("Cancel");
                availableActions.Add("Reject");
            }
            else if (order.Status == Constants.ORDER_SHIPPING)
            {
                availableActions.Add("Shipping");
                availableActions.Add("Finish");
                availableActions.Add("Cancel");
            }
          

            var model = new OrderDetailModel()
            {
                Order = order,
                Details = details,
                AvailableActions = availableActions // Truyền danh sách hành động vào view
            };
            return View(model);
        }



        /*  public IActionResult Details(int id = 0)

          {
              var order = OrderDataService.GetOrder(id);
              if (order == null)
                  return RedirectToAction("Index");
              var details = OrderDataService.ListOrderDetails(id);


              var model = new OrderDetailModel()
              {
                  Order = order,
                  Details = details
              };
              return View(model);
          }*/
        public IActionResult EditDetail(int orderId, int productId)
        {
            // Lấy chi tiết đơn hàng từ database theo OrderID và ProductID
            var orderDetail = OrderDataService.GetOrderDetail(orderId, productId);

            // Nếu không tìm thấy chi tiết đơn hàng, trả về lỗi 404
            if (orderDetail == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy đơn hàng
            }

            // Truyền thông tin chi tiết đơn hàng vào view
            return View(orderDetail); // Chắc chắn rằng orderDetail không null
        }

        /*   [HttpPost]
           public IActionResult EditDetail(int orderId, int productId, int quantity, decimal salePrice)
           {

               // Gọi service để lưu thông tin mặt hàng
               var success = OrderDataService.SaveOrderDetail(orderId, productId, quantity, salePrice);

               if (success)
               {

                   return RedirectToAction("Details", new { id = orderId });
               }
               else
               {

                   return View();
               }
           }*/
        [HttpPost]
        public IActionResult EditDetail(int orderId, int productId, int quantity, decimal salePrice)
        {
            // Kiểm tra số lượng và giá bán
            if (quantity == 0)
            {
                ModelState.AddModelError("Quantity", "Số lượng không được bằng 0.");
            }

            if (quantity < 0)
            {
                ModelState.AddModelError("Quantity", "Số lượng không được âm.");
            }
            if (salePrice < 0)
            {
                ModelState.AddModelError("SalePrice", "Giá bán không được âm.");
            }

            // Nếu có lỗi, trả về view với dữ liệu cũ
            if (!ModelState.IsValid)
            {
                var orderDetail = OrderDataService.GetOrderDetail(orderId, productId);
                if (orderDetail == null)
                {
                    return NotFound();
                }

                // Chỉ định lại giá trị nhập sai để view giữ đúng input user
                orderDetail.Quantity = quantity;
                orderDetail.SalePrice = salePrice;

                return View(orderDetail); // Trả lại view với dữ liệu đầy đủ
            }

            // Nếu dữ liệu hợp lệ, lưu thông tin
            var success = OrderDataService.SaveOrderDetail(orderId, productId, quantity, salePrice);

            if (success)
            {
                return RedirectToAction("Details", new { id = orderId });
            }

            // Xử lý trường hợp lưu thất bại
            ModelState.AddModelError("", "Không thể cập nhật chi tiết đơn hàng. Vui lòng thử lại.");
            var existingOrderDetail = OrderDataService.GetOrderDetail(orderId, productId);
            return View(existingOrderDetail);
        }

        public IActionResult DeleteDetail(int orderId, int productId)
        {
            var success = OrderDataService.DeleteOrderDetail(orderId, productId);

            if (success)
            {
                TempData["Message"] = "Mặt hàng đã được xóa khỏi đơn hàng!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa mặt hàng này!";
            }

            return RedirectToAction("Details", new { id = orderId });
        }


        //public IActionResult Shipping(int id = 0)
        //{
        //    return View();
        //}
        public IActionResult Shipping(int id = 0)
        {
            var order = OrderDataService.GetOrder(id);
            if (order == null)
            {
                return NotFound("Đơn hàng không tồn tại.");
            }

            var details = OrderDataService.ListOrderDetails(id);

            // Gọi phương thức để lấy danh sách shipper
            int rowCount;
            var shippers = CommonDataService.ListOfShippers(out rowCount);

            // Truyền danh sách shipper vào view
            var model = new OrderDetailModel
            {
                Order = order,
                Details = details ?? new List<OrderDetail>(),
                Shippers = shippers // Truyền danh sách shipper
            };

            return View(model);
        }
        [HttpPost]
        public IActionResult Shipping(int id, int shipperID)
        {
            if (shipperID <= 0)
            {
                ModelState.AddModelError("ShipperID", "Vui lòng chọn người giao hàng!.");
            }

            if (!ModelState.IsValid)
            {
                var order = OrderDataService.GetOrder(id);
                if (order == null)
                {
                    return NotFound("Đơn hàng không tồn tại.");
                }

                var details = OrderDataService.ListOrderDetails(id);
                int rowCount;
                var shippers = CommonDataService.ListOfShippers(out rowCount);

                var model = new OrderDetailModel
                {
                    Order = order,
                    Details = details ?? new List<OrderDetail>(),
                    Shippers = shippers
                };

                return View(model);
            }

            var success = OrderDataService.ShipOrder(id, shipperID);

            if (success)
            {
                TempData["Message"] = "Đã chuyển giao hàng thành công.";
                return RedirectToAction("Details", "Order", new { id = id });
            }

            TempData["ErrorMessage"] = "Không thể chuyển giao hàng. Vui lòng thử lại.";
            return RedirectToAction("Shipping", new { id = id });
        }


        /* [HttpPost]
         public IActionResult Shipping(int id, int shipperID)
         {
             if (shipperID <= 0)
             {
                 return Json(new { success = false, message = "Vui lòng chọn người giao hàng hợp lệ." });
             }

             var success = OrderDataService.ShipOrder(id, shipperID);

             if (success)
             {
                 // Chuyển hướng về trang chi tiết đơn hàng sau khi chuyển giao hàng thành công
                 TempData["Message"] = "Đã chuyển giao hàng thành công.";  // Hiển thị thông báo thành công trong View
                 return RedirectToAction("Details", "Order", new { id = id });
             }

             return Json(new { success = false, message = "Không thể chuyển giao hàng. Vui lòng thử lại." });
         }*/




        public IActionResult Create()
        {
            

            var condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PRODUCT_PAGE_SIZE,
                    SearchValue = " "
                };
            }
            
            return View(condition);

        }

        public IActionResult SearchProduct(ProductSearchInput condition)
        {
            int rowCount = 0;
            var data = ProductDataService.ListProducts(out rowCount, condition.Page, condition.PageSize, condition.SearchValue ?? "");
            var model = new ProductSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);
            return View(model);
        }
        public List<CartItem> GetShoppingCart()
        {
            var shoppingCart = ApplicationContext.GetSessionData<List<CartItem>>(SHOPPING_CART);
            if (shoppingCart == null)
            {
                shoppingCart = new List<CartItem>();
                ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            }
            return shoppingCart;
        }
        public IActionResult AddToCart(CartItem item)
        {
            if (item.SalePrice < 0 || item.Quantity <= 0)
                return Json("Giá bán và số lượng không hợp lệ");
            var shoppingCart = GetShoppingCart();
            var existsProduct = shoppingCart.FirstOrDefault(m => m.ProductID == item.ProductID);
            if (existsProduct == null)
            {
                shoppingCart.Add(item);
            }
            else
            {
                existsProduct.Quantity += item.Quantity;
                existsProduct.SalePrice += item.SalePrice;
            }
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }
        public IActionResult RemoveFromCart(int id = 0)
        {
            var shoppingCart = GetShoppingCart();
            int index = shoppingCart.FindIndex(m => m.ProductID == id);
            if (index >= 0)
                shoppingCart.RemoveAt(index);
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }
        //public IActionResult ClearCart()
        //{
        //    var shoppingCart = GetShoppingCart();
        //    shoppingCart.Clear();
        //    ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
        //    return Json("");
        //}
        public IActionResult ClearCart()
        {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Clear();
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);

            // Return a success flag or a URL to navigate
            return Json(new { success = true });
        }


        public IActionResult ShoppingCart()
        {
            return View(GetShoppingCart());
        }

        public IActionResult Init(int customerID = 0, string deliveryProvince = "", string deliveryAddress = "")
        {
            var shoppingCart = GetShoppingCart();
            if (shoppingCart.Count == 0)
                return Json("Giỏ hàng trống. Vui lòng chọn mặt hàng cần bán");
            if (customerID == 0 || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
                return Json("Vui lòng nhập đầy đủ thông tin khách hàng và nơi giao hàng");

            //int employeeID = 1; //TODO: Thay bởi ID của nhân viên đang login vào hệ thống
            var userData = User.GetUserData();
            if (userData == null)
                return Json("Không tìm thấy thông tin nhân viên đăng nhập");
            if (!int.TryParse(userData.UserId, out int employeeID))
                return Json("ID nhân viên không hợp lệ");

            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (var item in shoppingCart)
            {
                orderDetails.Add(new OrderDetail()
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice,
                });
            }
            int orderID = OrderDataService.InitOrder(employeeID, customerID, deliveryProvince, deliveryAddress, orderDetails);
            ClearCart();
            return Json(orderID);
        }
        public IActionResult Delete(int id)
        {
            try
            {
                // Attempt to delete the order using the OrderDataService
                bool success = OrderDataService.DeleteOrder(id);

                if (success)
                {
                    // If successful, return a success response
                    return Json(new { success = true });
                }
                else
                {
                    // If the deletion was not successful, return an error message
                    return Json(new { success = false, message = "Không thể xóa đơn hàng này do trạng thái không hợp lệ hoặc đơn hàng không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed and return a generic error message
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa đơn hàng." });
            }
        }
        public IActionResult Accept(int id)
        {
            var result = OrderDataService.AcceptOrder(id);
            if (result)
            {
                TempData["Message"] = "Đơn hàng đã được duyệt, đang chờ vận chuyển.";
                return RedirectToAction("Details", new { id = id });  // Chuyển hướng về trang chi tiết đơn hàng
            }
            else
            {
                TempData["Error"] = "Không thể duyệt đơn hàng này.";
                return RedirectToAction("Details", new { id = id });  // Quay lại trang chi tiết nếu không duyệt được
            }
        }

        // Hủy đơn hàng
        public IActionResult Cancel(int id)
        {
            var success = OrderDataService.CancelOrder(id);

            if (success)
            {
                TempData["Message"] = "Đơn hàng đã được hủy thành công.";
                return RedirectToAction("Details", "Order", new { id = id });
            }

            TempData["Error"] = "Không thể hủy đơn hàng. Vui lòng thử lại.";
            return RedirectToAction("Details", "Order", new { id = id });
        }

        // Từ chối đơn hàng
        public IActionResult Reject(int id)
        {
            var success = OrderDataService.RejectOrder(id);

            if (success)
            {
                TempData["Message"] = "Đơn hàng đã bị từ chối.";
                return RedirectToAction("Details", "Order", new { id = id });
            }

            TempData["Error"] = "Không thể từ chối đơn hàng. Vui lòng thử lại.";
            return RedirectToAction("Details", "Order", new { id = id });
        }

        // Xác nhận hoàn thành đơn hàng
        public IActionResult Finish(int id)
        {
            var success = OrderDataService.FinishOrder(id);

            if (success)
            {
                TempData["Message"] = "Đơn hàng đã được hoàn thành.";
                return RedirectToAction("Details", "Order", new { id = id });
            }

            TempData["Error"] = "Không thể hoàn thành đơn hàng. Vui lòng thử lại.";
            return RedirectToAction("Details", "Order", new { id = id });
        }



    }

}

