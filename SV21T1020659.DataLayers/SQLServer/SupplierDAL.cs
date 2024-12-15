

using Dapper;
using SV21T1020659.DomainModels;
using System.Data;

namespace SV21T1020659.DataLayers.SQLServer
{
    public class SupplierDAL : BaseDAL, ICommonDAL<Supplier>
    {
        public SupplierDAL(string connectionString) : base(connectionString)
        {
        }
        /*
                public int Add(Supplier data)

                {
                    int id = 0;
                    using (var connection = OpenConnection())
                    {
                        var sql = @"if exists (select * from Suppliers where Email = @Email)
                               select -1;
                            else
                               begin
                                  insert into Suppliers(SupplierName, ContactName, Address, Province, Phone, Email)
                                  values (@SupplierName, @ContactName, @Address, @Province, @Phone, @Email);
                                  select SCOPE_IDENTITY();
                               end";
                        var parameters = new
                        {
                            data.SupplierName,
                            data.ContactName,
                            data.Address,
                            data.Province,
                            data.Phone,
                            data.Email
                        };

                        return connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                    }
                }
        */

        public List<Supplier> List()
        {
            List<Supplier> data = new List<Supplier>();
            using (var connection = OpenConnection())
            {
                var sql = @"select	* from Suppliers";
                var parameter = new
                {
                };
                data = connection.Query<Supplier>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text).ToList();
            }

            return data;
        }
        public int Add(Supplier data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists (select * from Suppliers where Email = @Email)
                       select -1;
                    else
                       begin
                          insert into Suppliers(SupplierName, ContactName, Address, Province, Phone, Email)
                          values (@SupplierName, @ContactName, @Address, @Province, @Phone, @Email);
                          select SCOPE_IDENTITY();
                       end";
                var parameters = new
                {
                    SupplierName = data.SupplierName ?? "",
                    ContactName = data.ContactName ?? "",
                    Address = data.Address ?? "",
                    Province = data.Province ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? ""
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
                            FROM Suppliers 
                            WHERE (SupplierName LIKE @searchValue 
                                    OR ContactName LIKE @searchValue 
                                    OR Province LIKE @searchValue 
                                    OR Address LIKE @searchValue 
                                    OR Phone LIKE @searchValue 
                                    OR Email LIKE @searchValue)";
                var parameters = new
                {
                    searchValue
                };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
            return count;
        }


        public bool Delete(int id)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Suppliers WHERE SupplierID = @id";
                var parameters = new { id };
                return connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0; // Trả về true nếu xóa thành công
            }
        }

        public Supplier? Get(int id)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Suppliers WHERE SupplierID = @id";
                var parameters = new { id };
                return connection.QuerySingleOrDefault<Supplier>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }

        public bool InUsed(int id)
        {
            using (var connection = OpenConnection())
            {
                // Kiểm tra trong bảng Products
                var sql = @"SELECT COUNT(*) FROM Products WHERE SupplierID = @id";
                var parameters = new { id };
                return connection.ExecuteScalar<int>(sql, parameters) > 0; // Nếu có bản ghi thì đang được sử dụng
            }
        }



        public List<Supplier> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Supplier> data = new List<Supplier>();
            searchValue = $"%{searchValue.Trim()}%";

            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * 
                            FROM (
                                SELECT *, ROW_NUMBER() OVER(ORDER BY SupplierName) AS RowNumber
                                FROM Suppliers 
                                WHERE (SupplierName LIKE @searchValue 
                                       OR ContactName LIKE @searchValue 
                                       OR Province LIKE @searchValue 
                                       OR Address LIKE @searchValue 
                                       OR Phone LIKE @searchValue 
                                       OR Email LIKE @searchValue)
                            ) AS t
                            WHERE (@pageSize = 0) 
                            OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                            ORDER BY RowNumber";

                var parameters = new
                {
                    page,
                    pageSize,
                    searchValue
                };

                data = connection.Query<Supplier>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return data;
        }

        /*public bool Update(Supplier data)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE Suppliers 
                            SET SupplierName = @SupplierName, 
                                ContactName = @ContactName, 
                                Address = @Address, 
                                Province = @Province, 
                                Phone = @Phone, 
                                Email = @Email 
                            WHERE SupplierID = @SupplierID";

                var parameters = new
                {
                    data.SupplierID, 
                    data.SupplierName,
                    data.ContactName,
                    data.Address,
                    data.Province,
                    data.Phone,
                    data.Email
                };

                return connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0; // Trả về true nếu cập nhật thành công
            }
        }*/
        public bool Update(Supplier data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists (select * from Suppliers where SupplierID <> @SupplierID and Email = @Email)
                    begin
                       update Suppliers 
                       set    SupplierName = @SupplierName,
                              ContactName = @ContactName,
                              Address = @Address,
                              Province = @Province,
                              Phone = @Phone,
                              Email = @Email
                       where SupplierID = @SupplierID
                    end";
                var parameters = new
                {
                    SupplierID = data.SupplierID,
                    SupplierName = data.SupplierName ?? "",
                    ContactName = data.ContactName ?? "",
                    Address = data.Address ?? "",
                    Province = data.Province ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? ""
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

    }
}
