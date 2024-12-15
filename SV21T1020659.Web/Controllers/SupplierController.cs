using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.DomainModels;
using SV21T1020659.Web.Appcodes;
using SV21T1020659.Web.Models;

namespace SV21T1020659.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.ADMIN},{WebUserRoles.MANAGER}")]
    public class SupplierController : Controller
    {
        private const int PAGE_SIZE = 5;
        private const string SUPPLIER_SEARCH_CONDITION = "SupplierSearchCondition";
        public IActionResult Index()
        {
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPLIER_SEARCH_CONDITION);
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
            var data = CommonDataService.ListOfSuppliers(out rowCount, condition.Page, condition.PageSize, condition.SearchValue ?? "");
            SupplierSearchResult model = new SupplierSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(SUPPLIER_SEARCH_CONDITION, condition);
            return View( model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";
            var data = new Supplier
            {
                SupplierID = 0 
            };
            return View("Edit", data);
        }
        public IActionResult Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
            var data = CommonDataService.GetSupplier(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }
        public IActionResult Delete(int id)
        {
            if (Request.Method == "POST")
            {
                // Kiểm tra xem nhà cung cấp có đang được sử dụng hay không
                if (CommonDataService.IsSupplierInUse(id))
                {
                    TempData["Message"] = "Nhà cung cấp này đang được sử dụng và không thể xóa!";
                    return RedirectToAction("Index");
                }

                CommonDataService.DeleteSupplier(id);
                TempData["Message"] = "Nhà cung cấp đã được xóa!";
                return RedirectToAction("Index");
            }

            var data = CommonDataService.GetSupplier(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }
        [HttpPost]
        public IActionResult Save(Supplier data)

        {

            ViewBag.Title = data.SupplierID == 0 ? "Bổ sung nhà cung cấp" : "Cập nhật thông tin nhà cung cấp";
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(data.SupplierName))
                ModelState.AddModelError(nameof(data.SupplierName), "Tên nhà cung cấp không được để trống");
            if (string.IsNullOrWhiteSpace(data.ContactName))
                ModelState.AddModelError(nameof(data.ContactName), "Tên giao dịch không được để trống");
          
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập Email");
            if (string.IsNullOrWhiteSpace(data.Address))
                ModelState.AddModelError(nameof(data.Address), "Vui lòng nhập địa chỉ");
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Bạn chưa chọn tỉnh/thành");

            // Nếu có lỗi thì trả dữ liệu về cho View kèm theo thông báo lỗi
            if (!ModelState.IsValid)
            {
                return View("Edit", data);
            }


            if (data.SupplierID == 0)
            {
                int id = CommonDataService.AddSupplier(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.Email), "Email không được trùng");
                    return View("Edit", data);
                }

            }
            else
            {
                bool result = CommonDataService.UpdateSupplier(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.Email), "Email không được trùng");
                    return View("Edit", data);
                }

            }

            return RedirectToAction("Index");
        }
    }
}
