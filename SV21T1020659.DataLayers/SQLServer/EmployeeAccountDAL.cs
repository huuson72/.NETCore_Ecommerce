using Dapper;
using SV21T1020659.DomainModels;
using System.Data;

namespace SV21T1020659.DataLayers.SQLServer
{
    public class EmployeeAccountDAL : BaseDAL, IUserAccountDAL
    {
        public EmployeeAccountDAL(string connectionString) : base(connectionString)
        {
        }

        public UserAccount? Authorize(string username, string password)
        {
            UserAccount? data = null;
            
            using (var connection = OpenConnection()) // Hàm OpenConnection trả về SqlConnection đã mở
            {
                var sql = @"
            SELECT 
                EmployeeID AS UserId,
                Email AS UserName,
                FullName AS DisplayName,
                Photo,
                RoleNames
            FROM Employees
            WHERE Email = @Email AND Password = @Password";

                // Tham số truy vấn (parameter)
                var parameters = new
                {
                    Email = username,
                    password = password // Nếu mật khẩu đã mã hóa, hãy xử lý trước khi so sánh
                };

                // Sử dụng Dapper để thực thi truy vấn và lấy thông tin người dùng
                data = connection.QueryFirstOrDefault<UserAccount>(sql :sql,param: parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return data; // Trả về thông tin UserAccount hoặc null nếu không tìm thấy
        }


        public bool ChangePassword(string username, string newPassword)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"
                            UPDATE Employees
                            SET Password = @Password
                            WHERE Email = @Email";

                var parameters = new
                {
                    Email = username,
                    Password = newPassword // Nhớ mã hóa mật khẩu trước khi lưu
                };

                int affectedRows = connection.Execute(sql, parameters, commandType: CommandType.Text);
                return affectedRows > 0; // Trả về true nếu có dòng bị ảnh hưởng
            }
        }

    }
}
