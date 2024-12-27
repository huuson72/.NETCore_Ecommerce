using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.BusinessLayers;
using SV21T1020659.Shop.Models;
using SV21T1020659.Shop.Appcodes;
using System.Diagnostics;
using SV21T1020659.DomainModels;

namespace SV21T1020659.Shop.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }



        private const int PAGE_SIZE = 12;

        // Trang Index: Hiển thị toàn bộ sản phẩm mặc định với phân trang
        public IActionResult Index(int page = 1, int pageSize = PAGE_SIZE, string searchValue = "", int categoryId = 0)
        {
            var categories = CommonDataService.ListOfCategories(out _, 1, int.MaxValue, "");

            // Lấy danh sách sản phẩm
            int rowCount;
            var data = ProductDataService.ListProducts(out rowCount, page, pageSize, searchValue, categoryId);

            var model = new ProductSearchResult()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue,
                RowCount = rowCount,
                Data = data,
                CategoryID = categoryId
            };

            // Lấy thông tin người dùng và ảnh
            var userData = User.GetUserData();
            if (userData != null)
            {
                // Truyền thông tin ảnh của người dùng vào ViewBag
                ViewBag.UserPhoto = userData.Photo ?? "avatar.jpg"; // Nếu không có ảnh, sử dụng ảnh mặc định
            }

            ViewBag.Categories = categories; // Danh sách danh mục
            ViewBag.SelectedCategoryId = categoryId; // ID danh mục hiện tại
            ViewBag.SearchValue = searchValue; // Từ khóa tìm kiếm hiện tại

            return View(model); // Trả về kết quả ban đầu
        }


        // Tìm kiếm qua Ajax: Xử lý tìm kiếm
        public IActionResult Search(PaginationSearchInput condition)
        {
            int rowCount;

            // Chuẩn hóa giá trị tìm kiếm
            var originalSearchValue = condition.SearchValue?.Trim();
            condition.SearchValue = !string.IsNullOrWhiteSpace(originalSearchValue)
                ? $"%{originalSearchValue}%"
                : "";

            // Lấy danh sách sản phẩm
            var data = ProductDataService.ListProducts(
                out rowCount,
                condition.Page,
                condition.PageSize,
                condition.SearchValue ?? "",
                condition.CategoryID
            );

            var model = new ProductSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = originalSearchValue,
                RowCount = rowCount,
                Data = data,
                CategoryID = condition.CategoryID
            };


            return PartialView("Search", model);
        }


        public IActionResult AboutUs() 
        {
            return View();
        }

       
        public IActionResult Details(int id)
        {
            // Đặt giá trị mặc định cho ViewBag
            ViewBag.Categories = CommonDataService.ListOfCategories();
            ViewBag.CurrentCategoryID = 0;
            ViewBag.PageSize = 12; // Giá trị mặc định
            ViewBag.SearchValue = ""; // Không có từ khóa tìm kiếm mặc định
            ViewBag.SelectedCategoryId = 0; // Không có danh mục được chọn mặc định

            // Lấy thông tin sản phẩm
            var product = ProductDataService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách ảnh liên quan
            var relatedPhotos = ProductDataService.GetPhotosByProductId(id)
                              .Where(p => !p.IsHidden) // Loại bỏ ảnh ẩn
                              .OrderBy(p => p.DisplayOrder) // Sắp xếp theo thứ tự
                              .ToList();
            // Lấy thuộc tính sản phẩm
            var productAttributes = ProductDataService.ListProductAttributes(id);

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Photos = relatedPhotos,
                CategoryID = product.CategoryID,
                Attributes = productAttributes
            };

            return View(viewModel);
        }





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
