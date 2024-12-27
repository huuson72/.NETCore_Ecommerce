using Dapper;
using Microsoft.Data.SqlClient;
using SV21T1020659.DomainModels;
using System.Data;

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

        public bool ChangePassword(string username, string newPassword)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"
                UPDATE Customers
                SET Password = @Password
                WHERE Email = @Email";

                var parameters = new
                {
                    Email = username,
                    Password = newPassword
                };

                int affectedRows = connection.Execute(sql, parameters, commandType: CommandType.Text);
                return affectedRows > 0;
            }
        }
        public bool Register(CustomerRegisterViewModel model)
        {
            using (var connection = OpenConnection())
            {
                // Kiểm tra email đã tồn tại hay chưa
                var checkEmailSql = "SELECT COUNT(*) FROM Customers WHERE Email = @Email";
                var emailExists = connection.ExecuteScalar<int>(checkEmailSql, new { Email = model.Email }) > 0;

                if (emailExists)
                {
                    return false; // Email đã tồn tại
                }

                var sql = @"
INSERT INTO Customers (CustomerName, ContactName, Email, Password, Phone, Address, Province, IsLocked)
VALUES (@CustomerName, @ContactName, @Email, @Password, @Phone, @Address, @Province, 0)";

                var parameters = new
                {
                    model.CustomerName,
                    ContactName = model.CustomerName, // Sử dụng CustomerName làm giá trị cho ContactName
                    model.Email,
                    Password = model.Password, // Mã hóa mật khẩu nếu cần trước khi truyền
                    model.Phone,
                    model.Address,
                    model.Province
                };

                int affectedRows = connection.Execute(sql, parameters, commandType: CommandType.Text);
                return affectedRows > 0;

            }
        }

        public bool UpdateProfile(int customerId, string displayName, string email, string phoneNumber, string address, string province, string? photoFileName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Mở kết nối đến cơ sở dữ liệu
                connection.Open();

                // Kiểm tra email đã tồn tại với khách hàng khác
                var checkEmailSql = "SELECT COUNT(*) FROM Customers WHERE Email = @Email AND CustomerID != @CustomerID";
                var emailExists = connection.ExecuteScalar<int>(checkEmailSql, new { Email = email, CustomerID = customerId }) > 0;

                if (emailExists)
                {
                    return false; // Email đã tồn tại
                }

                // Câu lệnh SQL cập nhật thông tin người dùng, bao gồm cả tên ảnh nếu có
                var sql = @"
        UPDATE Customers
        SET CustomerName = @DisplayName,
            Email = @Email,
            Phone = @PhoneNumber,
            Address = @Address,
            Province = @Province,
            Photo = @PhotoFileName
        WHERE CustomerID = @CustomerID";

                var parameters = new
                {
                    CustomerID = customerId,
                    DisplayName = displayName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    Province = province,
                    PhotoFileName = photoFileName // Trường này sẽ chứa tên ảnh nếu có
                };

                // Thực thi câu lệnh SQL và trả về số dòng bị ảnh hưởng
                int affectedRows = connection.Execute(sql, parameters, commandType: CommandType.Text);
                return affectedRows > 0;
            }
        }
        // Phương thức kiểm tra email tồn tại
        public bool IsEmailExist(string email)
        {
            using (var connection = OpenConnection())
            {
                var sql = "SELECT COUNT(*) FROM Customers WHERE Email = @Email";

                // Sử dụng Dapper để kiểm tra số lượng
                int count = connection.ExecuteScalar<int>(sql, new { Email = email });

                return count > 0; // Trả về true nếu email tồn tại
            }
        }

    }
}
