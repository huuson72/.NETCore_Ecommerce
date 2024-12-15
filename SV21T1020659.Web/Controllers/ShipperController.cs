using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.DomainModels;
using SV21T1020659.Web.Appcodes;
using SV21T1020659.Web.Models;

namespace SV21T1020659.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.ADMIN},{WebUserRoles.MANAGER}")]
    public class ShipperController : Controller
    {
        public const int PAGE_SIZE = 5;
        private const string SHIPPER_SEARCH_CONDITION = "ShipperSearchCondition";
        public IActionResult Index()
        {
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(SHIPPER_SEARCH_CONDITION);
            if (condition == null)
                condition = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            return View(condition);
        }
        public IActionResult Search(PaginationSearchInput condition)
        {
            int rowCount;
            var data = CommonDataService.ListOfShippers(out rowCount, condition.Page, condition.PageSize, condition.SearchValue ?? "");
            ShipperSearchResult model = new ShipperSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(SHIPPER_SEARCH_CONDITION, condition);
            return View( model);
        }
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà người giao hàng";
            var data = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", data);
          
         
        }
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhà vận chuyển";
            var data = CommonDataService.GetShipper(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }
        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                // Kiểm tra xem nhà vận chuyển có đang được sử dụng hay không
                if (CommonDataService.IsShipperInUse(id))
                {
                    TempData["Message"] = "Nhà vận chuyển này đang được sử dụng và không thể xóa!";
                    return RedirectToAction("Index");
                }

                CommonDataService.DeleteShipper(id);
                TempData["Message"] = "Nhà vận chuyển đã được xóa!";
                return RedirectToAction("Index");
            }

            var data = CommonDataService.GetShipper(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }
        [HttpPost]
        public IActionResult Save(Shipper data)
        {
            ViewBag.Title = data.ShipperID == 0 ? "Bổ sung nhà vận chuyển" : "Cập nhật thông tin nhà vận chuyển";

            // Kiểm tra dữ liệu đầu vào, nếu không hợp lệ thì tạo ra thông báo lỗi
            if (string.IsNullOrWhiteSpace(data.ShipperName))
                ModelState.AddModelError(nameof(data.ShipperName), "Tên nhà vận chuyển không được để trống");
            if (string.IsNullOrWhiteSpace(data.Phone))
                ModelState.AddModelError(nameof(data.Phone), "Vui lòng nhập số điện thoại của nhà vận chuyển");

            // Kiểm tra ModelState để biết có tồn tại lỗi nào không
            if (!ModelState.IsValid)
            {
                return View("Edit", data); // Trả dữ liệu về cho View kèm theo các thông báo lỗi
            }

            // Nếu là thêm mới
            if (data.ShipperID == 0)
            {
                int id = CommonDataService.AddShiper(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không được trùng");
                    return View("Edit", data);
                }
                
            }
            // Nếu là cập nhật
            else
            {
                bool result = CommonDataService.UpdateShipper(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không được trùng");
                    return View("Edit", data);
                }
              
            }

            return RedirectToAction("Index");
        }

    }
}
