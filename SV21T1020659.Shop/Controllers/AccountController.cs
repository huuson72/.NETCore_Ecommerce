using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.Shop.Appcodes;
using SV21T1020659.Shop.Models;
using SV21T1020659.DomainModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SV21T1020659.Shop.Controllers
{
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
            // Lưu giá trị nhập lại để hiển thị nếu có lỗi
            ViewBag.Username = username;

            // Kiểm tra lỗi riêng: Không nhập tên đăng nhập hoặc mật khẩu
            if (string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError("username", "Tên đăng nhập không được để trống");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("password", "Mật khẩu không được để trống");
            }

            // Nếu có lỗi riêng, hiển thị lại form đăng nhập
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Kiểm tra lỗi chung: Sai tên đăng nhập hoặc mật khẩu
            var userAccount = UserAccountService.Authorize(UserTypes.Customer, username, password);

            if (userAccount == null)
            {
                // Thêm lỗi chung khi cả hai trường đã được nhập nhưng không đúng
                ModelState.AddModelError("Error", "Tên đăng nhập hoặc mật khẩu không đúng!");
                return View();
            }

            // Tạo dữ liệu người dùng và đăng nhập
            WebUserData userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()
            };

            var claimsPrincipal = userData.CreatePrincipal();
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }



        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //return RedirectToAction("Login");
            return RedirectToAction("Index", "Home");

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
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Vui lòng nhập mật khẩu hiện tại.");
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                    ModelState.AddModelError(nameof(model.NewPassword), "Vui lòng nhập mật khẩu mới.");
                if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                    ModelState.AddModelError(nameof(model.ConfirmPassword), "Vui lòng xác nhận mật khẩu.");
            }

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Mật khẩu mới và xác nhận mật khẩu không khớp.");
            }

            // Lấy thông tin người dùng hiện tại
            var userData = User.GetUserData();
            if (userData == null)
            {
                return RedirectToAction("Login");
            }

            // Kiểm tra mật khẩu hiện tại
            var userAccount = UserAccountService.Authorize(UserTypes.Customer, userData.UserName, model.CurrentPassword);
            if (userAccount == null)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không đúng.");
            }

            // Nếu có lỗi thì không thực hiện đổi mật khẩu
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Thực hiện đổi mật khẩu
            bool result = UserAccountService.ChangePassword(UserTypes.Customer, userData.UserName, model.NewPassword);
            if (result)
            {
                // Cập nhật thông báo thành công và redirect đến trang đổi mật khẩu
                TempData["SuccessMessage1"] = "Đổi mật khẩu thành công!";
                TempData["RedirectUrl"] = Url.Action("Login", "Account");
                return RedirectToAction("ChangePassword");
            }
            else
            {
                ModelState.AddModelError("", "Đổi mật khẩu thất bại. Vui lòng thử lại.");
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            // Lấy danh sách tỉnh/thành phố từ CommonDataService
            ViewBag.Provinces = CommonDataService.ListOfProvinces();
            return View(new DomainModels.CustomerRegisterViewModel());
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(DomainModels.CustomerRegisterViewModel model)
        {
            // Truyền lại danh sách tỉnh vào View
            ViewBag.Provinces = CommonDataService.ListOfProvinces();

            // Kiểm tra lỗi riêng
            if (string.IsNullOrWhiteSpace(model.CustomerName))
            {
                ModelState.AddModelError("CustomerName", "Tên khách hàng không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email không được để trống.");
            }
            else if (!new EmailAddressAttribute().IsValid(model.Email))
            {
                ModelState.AddModelError("Email", "Email không đúng định dạng.");
            }
            else if (UserAccountService.IsEmailExist(model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng sử dụng email khác.");
            }
            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại không được để trống.");
            }
            else if (!Regex.IsMatch(model.Phone, @"^\d{10,15}$"))
            {
                ModelState.AddModelError("Phone", "Số điện thoại phải từ 10 đến 15 chữ số.");
            }
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Mật khẩu không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError("ConfirmPassword", "Vui lòng xác nhận mật khẩu.");
            }
            else if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
            }
            if (string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError("Address", "Địa chỉ không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(model.Province))
            {
                ModelState.AddModelError("Province", "Vui lòng chọn tỉnh thành.");
            }

            // Nếu có lỗi, trả về form với thông báo lỗi riêng
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tiến hành đăng ký
            try
            {
                bool result = UserAccountService.Register(UserTypes.Customer, model);

                if (result)
                {
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay bây giờ.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("Error", "Đăng ký thất bại. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", $"Lỗi: {ex.Message}");
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult UpdateProfile()
        {
            var userData = User.GetUserData();
            if (userData == null)
                return RedirectToAction("Login");

            var model = new UpdateProfileViewModel
            {
                DisplayName = userData.DisplayName,
                Email = userData.Email,
                PhoneNumber = userData.PhoneNumber,
                Address = userData.Address,
                Province = userData.Province
            };

            // Lấy danh sách tỉnh/thành phố từ CommonDataService
            var provinces = CommonDataService.ListOfProvinces();
            if (provinces == null || !provinces.Any())
            {
                ModelState.AddModelError("", "Không thể tải danh sách tỉnh/thành phố.");
                provinces = new List<Province> // Cung cấp danh sách mặc định nếu không có dữ liệu
        {
            new Province { ProvinceName = "Hà Nội" },
            new Province { ProvinceName = "TP Hồ Chí Minh" }
        };
            }
            ViewBag.Provinces = provinces;

            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateProfile(UpdateProfileViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Provinces = CommonDataService.ListOfProvinces();
        //        return View(model);
        //    }

        //    var userData = User.GetUserData();
        //    if (userData == null)
        //        return RedirectToAction("Login");

        //    if (!int.TryParse(userData.UserId, out int customerId))
        //    {
        //        ModelState.AddModelError("", "ID khách hàng không hợp lệ.");
        //        ViewBag.Provinces = CommonDataService.ListOfProvinces();
        //        return View(model);
        //    }

        //    try
        //    {
        //        // Kiểm tra và lưu ảnh nếu có
        //        if (model.UploadPhoto != null)
        //        {
        //            string fileName = $"{DateTime.Now.Ticks}-{model.UploadPhoto.FileName}";
        //            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "customers", fileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                model.UploadPhoto.CopyTo(stream);
        //            }

        //            // Cập nhật tên file ảnh vào thông tin người dùng
        //            userData.Photo = fileName;
        //        }

        //        // Cập nhật thông tin người dùng
        //        bool result = UserAccountService.UpdateUserProfile(customerId, model);
        //        if (result)
        //        {
        //            TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
        //            return RedirectToAction("Profile", "Account");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Email đã tồn tại hoặc cập nhật thất bại. Vui lòng thử lại.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", $"Lỗi: {ex.Message}");
        //    }

        //    ViewBag.Provinces = CommonDataService.ListOfProvinces();
        //    return View(model);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Provinces = CommonDataService.ListOfProvinces();
                return View(model);
            }

            var userData = User.GetUserData();
            if (userData == null)
                return RedirectToAction("Login");

            if (!int.TryParse(userData.UserId, out int customerId))
            {
                ModelState.AddModelError("", "ID khách hàng không hợp lệ.");
                ViewBag.Provinces = CommonDataService.ListOfProvinces();
                return View(model);
            }

            try
            {
                // Kiểm tra và lưu ảnh nếu có
                if (model.UploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}-{model.UploadPhoto.FileName}";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "customers", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.UploadPhoto.CopyTo(stream);
                    }

                    // Cập nhật tên file ảnh vào thông tin người dùng
                    userData.Photo = fileName;
                }

                // Cập nhật thông tin người dùng
                bool result = UserAccountService.UpdateUserProfile(customerId, model);
                if (result)
                {
                    // Cập nhật ClaimsPrincipal với thông tin mới
                    var identity = (ClaimsIdentity)User.Identity;

                    // Cập nhật lại claim Photo
                    var photoClaim = identity.FindFirst(nameof(WebUserData.Photo));
                    if (photoClaim != null)
                    {
                        identity.RemoveClaim(photoClaim);
                    }
                    identity.AddClaim(new Claim(nameof(WebUserData.Photo), userData.Photo ?? ""));

                    // Nếu cần thêm DisplayName hoặc các thông tin khác
                    var displayNameClaim = identity.FindFirst(nameof(WebUserData.DisplayName));
                    if (displayNameClaim != null)
                    {
                        identity.RemoveClaim(displayNameClaim);
                    }
                    identity.AddClaim(new Claim(nameof(WebUserData.DisplayName), userData.DisplayName ?? ""));

                    TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                    return RedirectToAction("Profile", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Email đã tồn tại hoặc cập nhật thất bại. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            }

            ViewBag.Provinces = CommonDataService.ListOfProvinces();
            return View(model);
        }



        [HttpGet]
        public IActionResult Profile()
        {
            var userData = User.GetUserData();
            if (userData == null)
            {
                return RedirectToAction("Login");
            }

            if (!int.TryParse(userData.UserId, out int customerId))
            {
                
                return NotFound("Khách hàng không tồn tại.");
            }

            var customer = CommonDataService.GetCustomer(customerId);
            if (customer == null)
            {
            
                return NotFound("Khách hàng không tồn tại.");
            }

            return View(customer);
        }


    }
}
