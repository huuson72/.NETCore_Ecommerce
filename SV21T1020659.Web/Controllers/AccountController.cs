using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.Web.Appcodes;
using SV21T1020659.Web.Models;
using System.Security.Claims;

namespace SV21T1020659.Web.Controllers
{
    [Authorize]

    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.Username = username;
            // Kiểm tra thông tin đầu vào
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập đầy đủ tên và mật khẩu");
                return View();
            }
            //TODO: Kiểm tra xem username và password(của Employee) có đúng hay không
            var userAccount = UserAccountService.Authorize(UserTypes.Employee, username, password);


            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();
            }
            // Đăng nhập thành công

            WebUserData userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()

            };
            // Sử dụng CreatePrincipal để tạo ClaimsPrincipal
            var claimsPrincipal = userData.CreatePrincipal();

            // 2.Ghi nhận trạng thái đăng nhập
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userData.CreatePrincipal());

            // 3. Quay về trang chủ
            return RedirectToAction("Index", "Home");

        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            // Kiểm tra thông tin đầu vào
            if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ViewData["Message"] = "Vui lòng nhập đầy đủ thông tin.";
                ViewData["Success"] = false;
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ViewData["Message"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                ViewData["Success"] = false;
                return View(model);
            }

            // Lấy thông tin người dùng hiện tại
            var userData = User.GetUserData();
            if (userData == null)
            {
                return RedirectToAction("Login");
            }

            // Kiểm tra mật khẩu hiện tại
            var userAccount = UserAccountService.Authorize(UserTypes.Employee, userData.UserName, model.CurrentPassword);
            if (userAccount == null)
            {
                ViewData["Message"] = "Mật khẩu hiện tại không đúng.";
                ViewData["Success"] = false;
                return View(model);
            }

            // Thực hiện đổi mật khẩu
            bool result = UserAccountService.ChangePassword(UserTypes.Employee, userData.UserName, model.NewPassword);
            if (result)
            {
                ViewData["Message"] = "Đổi mật khẩu thành công!";
                ViewData["Success"] = true;
                ViewData["RedirectUrl"] = Url.Action("Login", "Account");
            }
            else
            {
                ViewData["Message"] = "Đổi mật khẩu thất bại. Vui lòng thử lại.";
                ViewData["Success"] = false;
            }

            return View(model);
        }



        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult AccessDenined()
        {
            return View();
        }
    }
}
