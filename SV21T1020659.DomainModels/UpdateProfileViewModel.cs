

using Microsoft.AspNetCore.Http;

namespace SV21T1020659.DomainModels
{
    public class UpdateProfileViewModel
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public IFormFile? UploadPhoto { get; set; }
    }
}
