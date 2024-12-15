using SV21T1020659.DataLayers;
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
            customerAccountDB = new SV21T1020659.DataLayers.SQLServer.EmployeeAccountDAL(connectionString);

        }

        public static UserAccount? Authorize(UserTypes userTypes,string username, string password)
        {
            var userAccount = employeeAccountDB.Authorize(username, password);
            Console.WriteLine($"Authorize - Photo: {userAccount?.Photo}");

            if (userTypes == UserTypes.Employee)
                 return employeeAccountDB.Authorize(username, password);
            else
                 return customerAccountDB.Authorize(username, password);
        }
        public static bool ChangePassword(UserTypes userType, string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Tên người dùng hoặc mật khẩu mới không hợp lệ.");

            if (userType == UserTypes.Employee)
                return employeeAccountDB.ChangePassword(username, newPassword);
            else if (userType == UserTypes.Customer)
                return customerAccountDB.ChangePassword(username, newPassword);

            return false; // Nếu không phải loại người dùng hợp lệ
        }
    }
    public enum UserTypes
    {
        Employee,
        Customer
    }
}
