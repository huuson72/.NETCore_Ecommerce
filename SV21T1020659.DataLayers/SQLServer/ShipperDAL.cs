
using Dapper;
using SV21T1020659.DomainModels;
using System.Data;
namespace SV21T1020659.DataLayers.SQLServer
{
    public class ShipperDAL : BaseDAL, ICommonDAL<Shipper>
    {
        public ShipperDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Shipper data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS (SELECT * FROM Shippers WHERE Phone = @Phone)
                       SELECT -1;
                    ELSE
                       BEGIN
                          INSERT INTO Shippers (ShipperName, Phone)
                          VALUES (@ShipperName, @Phone);
                          SELECT SCOPE_IDENTITY();
                       END";

                var parameters = new
                {
                    ShipperName = data.ShipperName ?? "",
                    Phone = data.Phone ?? ""
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
                            FROM Shippers 
                            WHERE (ShipperName LIKE @searchValue)";
                var parameters = new
                {
                    searchValue = searchValue
                };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
            return count;
        }

        public bool Delete(int id)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Shippers WHERE ShipperID = @id";
                var parameters = new { id };
                return connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0; // Trả về true nếu xóa thành công
            }
        }

        public Shipper? Get(int id)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Shippers WHERE ShipperID = @id";
                var parameters = new { id };
                return connection.QuerySingleOrDefault<Shipper>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }

        public bool InUsed(int id)
        {
            using (var connection = OpenConnection())
            {
                
                var sql = @"SELECT COUNT(*) FROM Orders WHERE ShipperID = @id";
                var parameters = new { id };
                return connection.ExecuteScalar<int>(sql, parameters) > 0; // Trả về true nếu Shipper đang được sử dụng
            }
        }
        /*public List<Shipper> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Shipper> data = new List<Shipper>();
            searchValue = $"%{searchValue}%";

            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * 
                            FROM (
                                SELECT *, ROW_NUMBER() OVER(ORDER BY ShipperName) AS RowNumber
                                FROM Shippers 
                                WHERE (ShipperName LIKE @searchValue)
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

                data = connection.Query<Shipper>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
            }
            return data;
        }*/
        public List<Shipper> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Shipper> data = new List<Shipper>();
            
            searchValue = $"%{searchValue.Trim()}%";

            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * 
                    FROM (
                        SELECT *, ROW_NUMBER() OVER(ORDER BY ShipperName) AS RowNumber
                        FROM Shippers 
                        WHERE (ShipperName LIKE @searchValue) -- Tìm kiếm có chứa từ khóa
                    ) AS t
                    WHERE (@pageSize = 0) 
                    OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                    ORDER BY RowNumber";

                var parameters = new
                {
                    page,        // Trang hiện tại
                    pageSize,   // Kích thước trang
                    searchValue  // Tìm kiếm tên người giao hàng
                };

                // Thực hiện truy vấn và lấy danh sách kết quả
                data = connection.Query<Shipper>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
            }
            return data;
        }


        public bool Update(Shipper data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF NOT EXISTS (SELECT * FROM Shippers WHERE ShipperID <> @ShipperID AND Phone = @Phone)
                    BEGIN
                        UPDATE Shippers 
                        SET ShipperName = @ShipperName,
                            Phone = @Phone
                        WHERE ShipperID = @ShipperID
                    END";

                var parameters = new
                {
                    ShipperID = data.ShipperID,
                    ShipperName = data.ShipperName ?? "",
                    Phone = data.Phone ?? ""
                };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
    }
}
