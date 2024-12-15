using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.DomainModels;
using SV21T1020659.Web.Appcodes;
using SV21T1020659.Web.Models;

namespace SV21T1020659.Web.Controllers
{
    [Authorize(Roles =$"{WebUserRoles.ADMIN},{WebUserRoles.MANAGER}")]
    public class CategoryController : Controller
    {
        public const int PAGE_SIZE = 5;
        private const string CATEGORY_SEARCH_CONDITION = "CategorySearchCondition";
        public IActionResult Index()
        {
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(CATEGORY_SEARCH_CONDITION);
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

            // Lưu giá trị tìm kiếm mà không có dấu %
            var originalSearchValue = condition.SearchValue?.Trim(); // Thêm Trim() để loại bỏ khoảng trắng

            // Thêm dấu % vào searchValue chỉ khi thực hiện truy vấn
            if (!string.IsNullOrWhiteSpace(originalSearchValue))
            {
                condition.SearchValue = $"%{originalSearchValue}%";
            }
            else
            {
                condition.SearchValue = ""; // Nếu không có giá trị tìm kiếm thì để trống
            }

            var data = CommonDataService.ListOfCategories(out rowCount, condition.Page, condition.PageSize, condition.SearchValue ?? "");
            CategorySearchResult model = new CategorySearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = originalSearchValue, // Sử dụng giá trị thực tế không có dấu %
                RowCount = rowCount,
                Data = data
            };

            // Lưu điều kiện tìm kiếm vào session với giá trị không có dấu %
            ApplicationContext.SetSessionData(CATEGORY_SEARCH_CONDITION, new PaginationSearchInput
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = originalSearchValue // Chỉ lưu giá trị thực tế vào session
            });

            return PartialView( model);
        }



        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            var data = new Category()
            {
                CategoryID = 0
            };
            return View("Edit", data);
        }
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin loại hàng";
            var data = CommonDataService.GetCategory(id);
            if (data == null)
                return RedirectToAction("Index");
            return View(data);
        }
        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                // Kiểm tra xem danh mục có đang được sử dụng hay không
                if (CommonDataService.IsCategoryInUse(id))
                {
                    TempData["Message"] = "Loại hàng này đang được sử dụng và không thể xóa!";
                    return RedirectToAction("Index");
                }

                CommonDataService.DeleteCategory(id);
                TempData["Message"] = "Loại hàng đã được xóa!";
                return RedirectToAction("Index");
            }

            var data = CommonDataService.GetCategory(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }
        [HttpPost]
        public IActionResult Save(Category data)
        {
            ViewBag.Title = data.CategoryID == 0 ? "Bổ sung loại hàng" : "Cập nhật thông tin loại hàng";

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được để trống");
            if (string.IsNullOrWhiteSpace(data.Description))
                ModelState.AddModelError(nameof(data.Description), "Mô tả không được để trống");

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                return View("Edit", data); // Trả dữ liệu về cho View, kèm theo các thông báo lỗi 
            }

            // Thêm hoặc cập nhật danh mục
            if (data.CategoryID == 0)
            {
                int id = CommonDataService.AddCategory(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được trùng");
                    return View("Edit", data);
                }
                
            }
            else
            {
                bool result = CommonDataService.UpdateCategory(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được trùng");
                    return View("Edit", data);
                }
               
            }

            return RedirectToAction("Index");
        }


    }
}
