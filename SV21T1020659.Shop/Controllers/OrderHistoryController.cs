using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;

namespace SV21T1020659.Shop.Controllers
{
    public class OrderHistoryController : BaseController
    {
    
        [HttpGet]
        public IActionResult Index()
        {
            // Lấy ID khách hàng hiện tại
            int customerId = GetCurrentCustomerId();

            // Gọi Service để lấy danh sách đơn hàng
            var orders = OrderDataService.GetOrdersByCustomerId(customerId);

            return View(orders); // Truyền danh sách đơn hàng đến view
        }

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
    }
}
