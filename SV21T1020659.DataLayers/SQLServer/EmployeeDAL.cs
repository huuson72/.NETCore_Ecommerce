using Dapper;
using SV21T1020659.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020659.DataLayers.SQLServer
{
    public class EmployeeDAL : BaseDAL, ICommonDAL<Employee>
    {
        public EmployeeDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Employee data)
        {
            int id = 0; // Khởi tạo biến để lưu ID
            using (var connection = OpenConnection())
            {
                var sql = @"if exists (select * from Employees where Email = @Email)
                        select -1
                    else
                        begin
                            INSERT INTO Employees (FullName, Phone, Email, BirthDate, Address, Photo, IsWorking)
                            VALUES (@FullName, @Phone, @Email, @BirthDate, @Address, @Photo, @IsWorking);
                            SELECT CAST(SCOPE_IDENTITY() as int);
                        end";

                var parameters = new
                {
                    FullName = data.FullName ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    BirthDate = data.BirthDate,
                    Address = data.Address ?? "",
                    Photo = data.Photo ?? "",
                    IsWorking = data.IsWorking
                };

                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                
                connection.Close();
            }
            return id;
        }


        public int Count(string searchValue = "")
        {
            int count = 0;
            searchValue = $"%{searchValue.Trim()}%";
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT COUNT(*) 
                            FROM Employees 
                            WHERE (FullName LIKE @searchValue 
                                   OR Phone LIKE @searchValue 
                                   OR Email LIKE @searchValue)";
                var parameters = new { searchValue };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
            return count;
        }

        public bool Delete(int id)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Employees 
                            WHERE EmployeeID = @id"; 
                var parameters = new { id };
                var rowsAffected = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text);
                return rowsAffected > 0; // Trả về true nếu có dòng bị xóa
            }
        }

        public Employee? Get(int id)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Employees 
                            WHERE EmployeeID = @id"; // Giả sử EmployeeID là khóa chính
                var parameters = new { id };
                return connection.QueryFirstOrDefault<Employee>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }

    

        public bool InUsed(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists (select * from Orders where EmployeeID = @EmployeeID)

                   select 1
                       else
                   select 0";
                var parameters = new
                {
                    EmployeeID = id
                };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
            return result;
        }


        public List<Employee> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Employee> data = new List<Employee>();
            searchValue = $"%{searchValue}%";

            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * 
                    FROM (
                        SELECT *, ROW_NUMBER() OVER(ORDER BY FullName) AS RowNumber
                        FROM Employees 
                        WHERE (FullName LIKE @searchValue 
                               OR Phone LIKE @searchValue 
                               OR Email LIKE @searchValue)
                    ) AS t
                    WHERE RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize
                    ORDER BY RowNumber";

                var parameters = new
                {
                    page,
                    pageSize,
                    searchValue
                };

                data = connection.Query<Employee>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
            }
            return data;
        }



        public bool Update(Employee data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from Employees where EmployeeID <> @EmployeeID and Email = @Email)
                    begin
                        UPDATE Employees 
                        SET FullName = @FullName, 
                            BirthDate = @BirthDate, 
                            Photo = @Photo, 
                            Phone = @Phone, 
                            Email = @Email,
                            IsWorking = @IsWorking 
                        WHERE EmployeeID = @EmployeeID
                    end";

                var parameters = new
                {
                    EmployeeID = data.EmployeeID,
                    FullName = data.FullName ?? "",
                    BirthDate = data.BirthDate,
                    Photo = data.Photo,
                    Phone = data.Phone,
                    Email = data.Email,
                    IsWorking = data.IsWorking
                };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }




    }
}
