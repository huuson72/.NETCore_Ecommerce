using Microsoft.AspNetCore.Mvc.Rendering;
using SV21T1020659.DomainModels;

namespace SV21T1020659.Shop.Models
{
    public class HeaderViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>(); // Danh sách gốc
        public int CurrentCategoryID { get; set; } = 0;                        // Danh mục hiện tại
        public SelectList CategorySelectList { get; set; }                    // SelectList cho dropdown
    }
}
