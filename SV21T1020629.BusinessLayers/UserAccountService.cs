using SV21T1020659.DataLayers;
using SV21T1020659.DataLayers.SQLServer;
using SV21T1020659.DomainModels;


namespace SV21T1020629.BusinessLayers
{
    public class UserAccountService
    {
        private static readonly IUserAccountDAL employeeAccountDB;
        private static readonly IUserAccountDAL customerAccountDB;

        static UserAccountService()
        {
            string connectionString = Configuration.ConnectionString;
            employeeAccountDB = new SV21T1020659.DataLayers.SQLServer.EmployeeAccountDAL(connectionString);
            customerAccountDB = new SV21T1020659.DataLayers.SQLServer.CustomerAccountDAL(connectionString);

        }
        public static UserAccount? Authorize(UserTypes userTypes, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Tên người dùng hoặc mật khẩu không được để trống.");

            UserAccount? userAccount = null;
            string hashedPassword = HashPassword(password);
            if (userTypes == UserTypes.Employee)
            {
                userAccount = employeeAccountDB.Authorize(username, password);
            }
            else if (userTypes == UserTypes.Customer)
            {
                //userAccount = customerAccountDB.Authorize(username, password);
                userAccount = customerAccountDB.Authorize(username, hashedPassword);
            }

            // Ghi log cho debug
            Console.WriteLine($"Authorize - UserType: {userTypes}, Username: {username}, Success: {userAccount != null}");

            return userAccount;
        }
        /* public static UserAccount? Authorize(UserTypes userTypes,string username, string password)
         {
             var userAccount = employeeAccountDB.Authorize(username, password);
             Console.WriteLine($"Authorize - Photo: {userAccount?.Photo}");

             if (userTypes == UserTypes.Employee)
                  return employeeAccountDB.Authorize(username, password);
             else
                  return customerAccountDB.Authorize(username, password);
         }*/
        /* public static bool ChangePassword(UserTypes userType, string username, string newPassword)
         {
             if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
                 throw new ArgumentException("Tên người dùng hoặc mật khẩu mới không hợp lệ.");

             if (userType == UserTypes.Employee)
                 return employeeAccountDB.ChangePassword(username, newPassword);
             else if (userType == UserTypes.Customer)
                 return customerAccountDB.ChangePassword(username, newPassword);

             return false; // Nếu không phải loại người dùng hợp lệ
         }
     }*/
        public static bool ChangePassword(UserTypes userType, string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Tên người dùng hoặc mật khẩu mới không hợp lệ.");

            // Mã hóa mật khẩu mới trước khi lưu vào DB
            string hashedPassword = HashPassword(newPassword);

            bool result = false;

            if (userType == UserTypes.Employee)
            {
                result = employeeAccountDB.ChangePassword(username, hashedPassword);
            }
            else if (userType == UserTypes.Customer)
            {
                result = customerAccountDB.ChangePassword(username, hashedPassword);
            }

            // Ghi log cho kết quả đổi mật khẩu
            Console.WriteLine($"ChangePassword - UserType: {userType}, Username: {username}, Success: {result}");

            return result;
        }

        public static bool Register(UserTypes userType, CustomerRegisterViewModel model)
        {
            if (userType != UserTypes.Customer)
                throw new NotSupportedException("Chỉ hỗ trợ đăng ký cho khách hàng.");

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                throw new ArgumentException("Email hoặc mật khẩu không hợp lệ.");

            if (model.Password != model.ConfirmPassword)
                throw new ArgumentException("Mật khẩu và xác nhận mật khẩu không khớp.");

            // Mã hóa mật khẩu trước khi lưu vào DB
            model.Password = HashPassword(model.Password);

            // Gọi phương thức Register từ DAL
            var result = customerAccountDB.Register(model);

            Console.WriteLine($"Register - UserType: {userType}, Email: {model.Email}, Success: {result}");

            return result;
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        public static bool IsEmailExist(string email)
        {
            return customerAccountDB.IsEmailExist(email);
        }
        public static bool UpdateUserProfile(int userId, UpdateProfileViewModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Thông tin cập nhật không hợp lệ.");

            bool result = false;
            string photoFileName = null;

            // Kiểm tra nếu có ảnh, lưu ảnh vào thư mục và lấy tên file ảnh
            if (model.UploadPhoto != null)
            {
                // Tạo tên file ảnh duy nhất
                photoFileName = $"{DateTime.Now.Ticks}-{model.UploadPhoto.FileName}";

                // Lưu ảnh vào thư mục wwwroot/images/customers/
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "customers", photoFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.UploadPhoto.CopyTo(stream);
                }
            }

            // Cập nhật thông tin cho khách hàng và tên ảnh nếu có
            result = customerAccountDB.UpdateProfile(userId, model.DisplayName, model.Email, model.PhoneNumber, model.Address, model.Province, photoFileName);

            // Ghi log cho kết quả cập nhật thông tin
            Console.WriteLine($"UpdateUserProfile - UserId: {userId}, Success: {result}");

            return result;
        }
      

    }


        public enum UserTypes
    {
        Employee,
        Customer
    }
}

