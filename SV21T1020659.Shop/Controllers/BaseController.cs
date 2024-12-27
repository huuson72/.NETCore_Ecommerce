using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SV21T1020629.BusinessLayers;
using SV21T1020659.BusinessLayers;

namespace SV21T1020659.Shop.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Lấy danh sách danh mục và truyền vào ViewBag
            var categories = CommonDataService.ListOfCategories(out _, 1, int.MaxValue, "");
            ViewBag.Categories = categories;
        }
    }
}
