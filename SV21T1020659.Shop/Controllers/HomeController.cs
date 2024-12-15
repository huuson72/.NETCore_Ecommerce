using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.BusinessLayers;
using SV21T1020659.Shop.Models;
using SV21T1020659.Web.Appcodes;
using SV21T1020659.Web.Models;
using System.Diagnostics;

namespace SV21T1020659.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        /* public IActionResult Index(int page = 1, int pageSize = 10, string searchValue = "")
         {
             int rowCount;
             var products = ProductDataService.ListProducts(
                 out rowCount,
                 page,
                 pageSize,
                 searchValue
             );

             int totalPages = (int)Math.Ceiling((double)rowCount / pageSize);

             ViewBag.RowCount = rowCount;
             ViewBag.Page = page;
             ViewBag.PageSize = pageSize;

             ViewBag.SearchValue = searchValue;
             ViewBag.TotalPages = totalPages;

             return View(products);
         }

 */

        private const int PAGE_SIZE = 20;

        public IActionResult Index(int page = 1, int pageSize = PAGE_SIZE, string searchValue = "")
        {
            PaginationSearchInput condition = new PaginationSearchInput()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue
            };
            return View(condition);
        }

        public IActionResult Search(PaginationSearchInput condition)
        {
            int rowCount;

            // Chuẩn hóa giá trị tìm kiếm
            var originalSearchValue = condition.SearchValue?.Trim();
            if (!string.IsNullOrWhiteSpace(originalSearchValue))
            {
                condition.SearchValue = $"%{originalSearchValue}%";
            }
            else
            {
                condition.SearchValue = "";
            }

            var data = ProductDataService.ListProducts(out rowCount, condition.Page, condition.PageSize, condition.SearchValue ?? "");

            ProductSearchResult model = new ProductSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = originalSearchValue,
                RowCount = rowCount,
                Data = data
            };

            return PartialView("Search", model);
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
