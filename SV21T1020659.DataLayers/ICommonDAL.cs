namespace SV21T1020659.DataLayers
{
    //Định nghĩa các phép xử lí dữ liệu
    public interface ICommonDAL <T> where T : class

    {
        /// <summary>
        /// Tìm kiếm và lấy 1 danh sách dữ liệu dưới dạng phân trang (pagination)
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">số dòng hiển thị trên mỗi trang ( = 0 tức là không phân trang)</param>
        /// <param name="searchValue">Giá trị cần tìm (chuỗi rỗng tức là lấy toàn bộ dữ liệu)</param>
        /// <returns></returns>

     
        List<T> List(int page = 1,int pageSize = 0, string searchValue = "");
        /// <summary>
        /// Đếm số lượng dòng dữ liệu tìm đc
        /// </summary>
        /// <param name="searchValue">giá trị cần tìm chuỗi(chuối rỗng nếu đếm toàn bộ dữ liệu)</param>
        /// <returns></returns>  
        int Count(string searchValue = "");
        /// <summary>
        ///  Lấy 1 dòng dữ liệu dựa vào khóa chính/id trả về null nếu dữ liệu k tồn tại
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       
        T? Get(int id);
        /// <summary>
        /// Bổ sung 1 bản ghi vào trong CSDL. Hàm trả về ID của CSDL vừa bổ sung
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        
        int Add(T data);
        /// <summary>
        /// Cập nhật 1 bản ghi dữ liệu
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
       
        bool Update(T data);
        /// <summary>
        ///     Xoa 1 dòng dữ liệu dựa vào giá trị của khóa chính/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        bool Delete(int id);
        /// <summary>
        /// Kiểm tra xem 1 dòng dữ liệu có khóa là id hiện có dữ liệu tham chiếu hay k?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool InUsed(int id);



    }
}
