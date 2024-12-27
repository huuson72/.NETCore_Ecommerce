using SV21T1020659.DomainModels;
namespace SV21T1020659.DataLayers
{
    public interface IUserAccountDAL
    {
        /// <summary>
        /// Kiểm tra xem tên đăng nhập và mật khẩu có đúng hay không?
        /// Nếu đúng trả về thông tin User, sai trả về null
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        UserAccount? Authorize(string username, string password);
        /// <summary>
        /// Đổi mk
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool ChangePassword(string username, string password);
        public bool Register(CustomerRegisterViewModel model);
        public bool UpdateProfile(int customerId, string displayName, string email, string phoneNumber, string address, string province, string? photoFileName);
        bool IsEmailExist(string email);
    }
}
