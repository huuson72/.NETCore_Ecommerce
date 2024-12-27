using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SV21T1020629.BusinessLayers;
using SV21T1020659.BusinessLayers;
using SV21T1020659.DomainModels;
using SV21T1020659.Shop.Models;

namespace SV21T1020659.Shop.Controllers
{
    public class CartController : Controller
    {
        private const string CART_SESSION_KEY = "ShoppingCart";

        // Phương thức lưu giỏ hàng vào session
        private void SaveCartToSession(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CART_SESSION_KEY, JsonConvert.SerializeObject(cart));
        }

        // Phương thức lấy giỏ hàng từ session
        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CART_SESSION_KEY);
            return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }

     

        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    success = false,
                    redirectToLogin = true,
                    message = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng."
                });
            }

            var product = ProductDataService.GetProduct(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var cart = GetCartFromSession();
            var cartItem = cart.FirstOrDefault(x => x.ProductID == id);

            string message;
            bool isNewProduct = false; // Flag để kiểm tra nếu đây là sản phẩm mới

            if (cartItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ, chỉ tăng số lượng
                cartItem.Quantity++;
                message = "Cập nhật số lượng sản phẩm thành công!";
            }
            else
            {
                // Thêm sản phẩm mới vào giỏ
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Photo = product.Photo ?? "product1.jpg",
                    Quantity = 1,
                    SalePrice = product.Price,
                    Unit = product.Unit
                });
                message = "Thêm sản phẩm vào giỏ hàng thành công!";
                isNewProduct = true; // Đánh dấu là sản phẩm mới
            }

            // Cập nhật tổng số lượng sản phẩm trong giỏ nếu là sản phẩm mới
            int cartCount = HttpContext.Session.GetInt32("CartCount") ?? 0;
            if (isNewProduct)
            {
                cartCount++;
                HttpContext.Session.SetInt32("CartCount", cartCount);
            }

            // Lưu giỏ hàng vào session
            SaveCartToSession(cart);

            return Json(new
            {
                success = true,
                cartCount = cartCount,
                message = message
            });
        }

       


        public IActionResult ViewCart()
        {
            var cart = GetCartFromSession();
            ViewBag.Categories = CommonDataService.ListOfCategories();

           
            ViewBag.CategoryID = cart.FirstOrDefault()?.CategoryID ?? 0; // Ví dụ: lấy CategoryID của sản phẩm đầu tiên

            return View(cart);
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int change)
        {
            var cart = GetCartFromSession();
            var cartItem = cart.FirstOrDefault(x => x.ProductID == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += change;

                if (cartItem.Quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
            }

            SaveCartToSession(cart);

            // Trả về partial HTML giỏ hàng đã cập nhật
            return PartialView("_CartPartial", cart);
        }

     
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartFromSession();
            var cartItem = cart.FirstOrDefault(x => x.ProductID == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            // Lưu giỏ hàng cập nhật vào session
            SaveCartToSession(cart);

            // Cập nhật số lượng giỏ hàng
            int cartCount = cart.Sum(x => x.Quantity);

            HttpContext.Session.SetInt32("CartCount", cartCount);

            return Json(new
            {
                success = true,
                cartCount = cartCount
            });
        }


        // Hàm tiện ích để lấy CustomerID của người dùng hiện tại
        private int GetCurrentCustomerId()
        {
            var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (customerIdClaim == null)
            {
                throw new InvalidOperationException("Người dùng chưa đăng nhập.");
            }

            if (int.TryParse(customerIdClaim.Value, out int customerId))
            {
                return customerId;
            }

            throw new InvalidOperationException("Không thể xác định ID của khách hàng.");
        }
        [HttpGet]
        public IActionResult Checkout()
        {
            // Lấy giỏ hàng từ session
            var cart = GetCartFromSession();

            // Kiểm tra giỏ hàng có sản phẩm nào không
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("ViewCart");
            }

            // Tính tổng giá trị giỏ hàng
            decimal totalPrice = cart.Sum(item => item.Quantity * item.SalePrice);
            // Lấy danh sách tỉnh thành từ CommonDataService
            ViewBag.Provinces = CommonDataService.ListOfProvinces();
            // Truyền dữ liệu vào ViewBag
          
            var paymentInfo = new PaymentInfo
            {
                Provinces = CommonDataService.ListOfProvinces(), // Danh sách tỉnh/thành phố
            };
              ViewBag.Cart = cart;
            ViewBag.TotalPrice = totalPrice;

            // Hiển thị form thanh toán
            return View(paymentInfo);
        }

        [HttpPost]
        public IActionResult Checkout(PaymentInfo paymentInfo)
        {
            // Lấy giỏ hàng từ session
            var cart = GetCartFromSession();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("ViewCart");
            }

            try
            {
                // Lấy thông tin khách hàng hiện tại
                int customerId = GetCurrentCustomerId();

                // Chuyển đổi giỏ hàng thành danh sách OrderDetail
                var orderDetails = cart.Select(item => new OrderDetail
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                }).ToList();

                // Gọi InitOrder với employeeID = null
                int orderId = OrderDataService.InitOrderCustomer(
                    null, // Khách hàng tự đặt hàng, không có EmployeeID
                    customerId,
                    paymentInfo.Province,
                    paymentInfo.Address,
                    orderDetails,
                    paymentInfo.PhoneNumber
                );

                if (orderId > 0)
                {
                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    HttpContext.Session.Remove(CART_SESSION_KEY);
                    HttpContext.Session.SetInt32("CartCount", 0);
                   
                    return RedirectToAction("Details", "Cart", new { orderId });

                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tạo đơn hàng. Vui lòng thử lại.";
                    return RedirectToAction("ViewCart");
                }
            }
            catch (Exception ex)
            {
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thực hiện thanh toán. Vui lòng thử lại.";
                return RedirectToAction("ViewCart");
            }
        }
        public IActionResult OrderConfirmation(int orderId)
        {
            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("ViewCart");
            }

            // Điều hướng tới Details với orderId
            return RedirectToAction("Details", "Cart", new { orderId = orderId });
        }



        //[HttpGet]
        //public IActionResult Details(int orderId)
        //{
        //    Console.WriteLine($"Fetching order with ID: {orderId}");

        //    if (orderId <= 0)
        //    {
        //        TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
        //        return View();
        //    }

        //    // Lưu orderId vào Session
        //    HttpContext.Session.SetInt32("OrderId", orderId);

        //    var order = OrderDataService.GetOrder(orderId);

        //    if (order == null)
        //    {
        //        Console.WriteLine($"Order not found for ID: {orderId}");
        //        TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
        //        return View();
        //    }

        //    Console.WriteLine($"Order found: {order.CustomerName}");
        //    ViewBag.PhoneNumber = order.PhoneNumber;

        //    var orderDetails = OrderDataService.ListOrderDetails(orderId);

        //    ViewBag.Order = order;
        //    ViewBag.OrderId = orderId;

        //    return View(orderDetails);
        //}
        [HttpGet]
        public IActionResult Details(int orderId)
        {
            Console.WriteLine($"Fetching order with ID: {orderId}");

            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index", "OrderHistory"); // Quay lại trang lịch sử nếu không tìm thấy đơn hàng
            }

            // Lấy thông tin đơn hàng từ Service
            var order = OrderDataService.GetOrder(orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index", "OrderHistory");
            }

            Console.WriteLine($"Order found: {order.CustomerName}");
            ViewBag.PhoneNumber = order.PhoneNumber;

            // Lấy chi tiết sản phẩm của đơn hàng
            var orderDetails = OrderDataService.ListOrderDetails(orderId);

            ViewBag.Order = order;
            ViewBag.OrderId = orderId;

            return View(orderDetails); // Trả về view chi tiết đơn hàng
        }




    }
}
