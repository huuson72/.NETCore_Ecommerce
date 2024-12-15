using Microsoft.Data.SqlClient;

namespace SV21T1020659.DataLayers.SQLServer
{
    public abstract class BaseDAL
    {
        protected string _connectionString = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">Chuỗi tham số kết nối CSDL</param>
        public BaseDAL(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// Tạp và mở kết nối đến CSDL SQL Server
        /// </summary>
        /// <returns></returns>
        protected SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

    }
}
