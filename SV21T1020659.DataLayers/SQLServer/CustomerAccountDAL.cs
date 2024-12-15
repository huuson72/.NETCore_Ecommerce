using Dapper;
using SV21T1020659.DomainModels;

namespace SV21T1020659.DataLayers.SQLServer
{
    public class CustomerAccountDAL : BaseDAL, IUserAccountDAL
    {
        public CustomerAccountDAL(string connectionString) : base(connectionString)
        {
        }

        public UserAccount? Authorize(string username, string password)
        {
            UserAccount? data = null;

            using (var connection = OpenConnection()) // Hàm OpenConnection trả về SqlConnection đã mở
            {
                var sql = @"
                          SELECT 
                                CustomerID AS UserId,
                                Email AS UserName,
                                CustomerName AS DisplayName,
                                N'' as Photo,
                                N'customer' as RoleNames
                          FROM Customers
                          WHERE Email = @Email and Password = @Password";

                // Tham số truy vấn (parameter)
                var parameters = new
                {
                    Email = username,
                    Password = password // Nếu mật khẩu đã mã hóa, hãy xử lý trước khi so sánh
                };

                // Sử dụng Dapper để thực thi truy vấn và lấy thông tin người dùng
                data = connection.QueryFirstOrDefault<UserAccount>(sql, parameters);
            }

            return data; // Trả về thông tin UserAccount hoặc null nếu không tìm thấy
        }

        public bool ChangePassword(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
