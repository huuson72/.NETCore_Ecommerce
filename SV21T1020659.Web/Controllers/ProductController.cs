using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020629.BusinessLayers;
using SV21T1020659.BusinessLayers;
using SV21T1020659.DomainModels;
using SV21T1020659.Web.Appcodes;
using SV21T1020659.Web.Models;
namespace SV21T1020659.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.ADMIN},{WebUserRoles.MANAGER}")]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 10;

        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";

        public IActionResult Index(int page = 1)
        {
            ProductSearchInput condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION) ?? new ProductSearchInput();

            // Thiết lập giá trị PageSize
            condition.PageSize = PAGE_SIZE;

            return View(CreateProductSearchResult(page, condition));
        }

        public IActionResult Search(ProductSearchInput condition)
        {
            // Lưu điều kiện tìm kiếm vào session
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);

            return PartialView(CreateProductSearchResult(condition.Page, condition));
        }

        private ProductSearchResult CreateProductSearchResult(int page, ProductSearchInput condition)
        {
            int totalProducts;
            var products = ProductDataService.ListProducts(out totalProducts, page, condition.PageSize, condition.SearchValue, condition.CategoryID, condition.SupplierID, condition.MinPrice, condition.MaxPrice);
            var categories = CommonDataService.ListOfCategories(out _, 1, int.MaxValue, "") ?? new List<Category>();
            var suppliers = CommonDataService.ListOfSuppliers(out _, 1, int.MaxValue, "") ?? new List<Supplier>();
            return new ProductSearchResult
            {
                Data = products,
                RowCount = totalProducts,
                PageSize = condition.PageSize,
                CurrentPage = page,
                CategoryID = condition.CategoryID,
                SupplierID = condition.SupplierID,
                MinPrice = condition.MinPrice,
                SearchValue = condition.SearchValue,
                MaxPrice = condition.MaxPrice,
                //Categories = categories,
                //Suppliers = suppliers
            };
        }

        /*        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";

                public IActionResult Index(int page = 1)
                {
                    ProductSearchInput condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION) ?? new ProductSearchInput();

                    // Thiết lập giá trị PageSize
                    condition.PageSize = condition.PageSize > 0 ? condition.PageSize : 10; // Đặt giá trị mặc định nếu PageSize không hợp lệ

                    int totalProducts;
                    var products = ProductDataService.ListProducts(out totalProducts, page, condition.PageSize, condition.SearchValue, condition.CategoryID, condition.SupplierID, condition.MinPrice, condition.MaxPrice);

                    // Tạo đối tượng ProductSearchResult
                    var searchResult = new ProductSearchResult
                    {
                        Data = products,
                        RowCount = totalProducts, // Đảm bảo RowCount được thiết lập từ tổng số sản phẩm
                        PageSize = condition.PageSize, // Đặt PageSize cho pagination
                        Page = page, // Thiết lập trang hiện tại
                        CategoryID = condition.CategoryID,
                        SupplierID = condition.SupplierID,
                        MinPrice = condition.MinPrice,
                        SearchValue = condition.SearchValue,
                        MaxPrice = condition.MaxPrice,
                        Categories = CommonDataService.ListOfCategories(out _, 1, int.MaxValue, ""),
                        Suppliers = CommonDataService.ListOfSuppliers(out _, 1, int.MaxValue, "")
                    };

                    return View(searchResult);
                }

                public IActionResult Search(ProductSearchInput condition)
                {
                    // Lưu điều kiện tìm kiếm vào session
                    ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);

                    // Gọi phương thức để lấy danh sách sản phẩm
                    int totalProducts;
                    var products = ProductDataService.ListProducts(out totalProducts, condition.Page, condition.PageSize, condition.SearchValue, condition.CategoryID, condition.SupplierID, condition.MinPrice, condition.MaxPrice);

                    // Tạo đối tượng ProductSearchResult
                    var searchResult = new ProductSearchResult
                    {
                        Data = products,
                        RowCount = totalProducts,
                        PageSize = condition.PageSize,
                        CurrentPage = condition.Page,
                        CategoryID = condition.CategoryID,
                        SupplierID = condition.SupplierID,
                        MinPrice = condition.MinPrice,
                        MaxPrice = condition.MaxPrice,
                        Categories = CommonDataService.ListOfCategories(out _, 1, int.MaxValue, ""),
                        Suppliers = CommonDataService.ListOfSuppliers(out _, 1, int.MaxValue, "")
                    };

                    return PartialView(searchResult);
                }

        */


        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung sản phẩm";
            var data = new Product() // Tạo đối tượng sản phẩm mới
            {
                ProductID = 0,
                IsSelling = true,
                Photo = "noproduct.jpg"
                // Các thuộc tính khác có thể được khởi tạo tại đây nếu cần
            };


            return View("Edit", data); // Trả về view "Edit" với thông tin sản phẩm mới
        }


        public IActionResult Edit(int id)
        {
            var product = ProductDataService.GetProduct(id);

            if (product == null)
            {
                return NotFound(); // Trả về mã 404 nếu không tìm thấy sản phẩm
            }


            // Lấy danh sách ảnh của sản phẩm
            var photos = ProductDataService.ListPhotos(product.ProductID) ?? new List<ProductPhoto>(); // Đảm bảo không null
            ViewBag.Photos = photos; // Gán danh sách ảnh vào ViewBag

            // Lấy danh sách thuộc tính của sản phẩm
            var attributes = ProductDataService.ListAttributes(product.ProductID) ?? new List<ProductAttribute>(); // Đảm bảo không null
            ViewBag.Attributes = attributes; // Gán danh sách thuộc tính vào ViewBag



            return View(product); // Trả về view "Edit" với sản phẩm
        }



        /*  public IActionResult Photo(int id = 0, string method = "", int photoId = 0)
          {
              switch (method)
              {
                  case "add":
                      ViewBag.Title = "Bổ sung ảnh";
                      return View();
                  case "edit":
                      ViewBag.Title = "Thay đổi ảnh";
                      return View();
                  case "delete":

                      return RedirectToAction("Edit", new { id = id });
                  default:
                      return RedirectToAction("Index");
              }

          }
  
        */
        public IActionResult Photo(int id = 0, string method = "", long photoId = 0, ProductPhoto photo = null, IFormFile? uploadPhoto = null)
        {
            ViewBag.ProductID = id;

            switch (method.ToLower())
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh";
                    photo = new ProductPhoto { ProductID = id };
                    return View(photo);

                case "edit":
                    photo = ProductDataService.GetPhoto(photoId);
                    if (photo == null)
                    {
                        return NotFound();
                    }
                    ViewBag.Title = "Thay đổi ảnh";
                    break;

                case "delete":
                    bool deleteResult = ProductDataService.DeletePhoto(photoId);
                    return RedirectToAction("Edit", new { id = id });

                case "save":
                    return SavePhoto(photo, uploadPhoto);

                default:
                    break;
            }

            return View(photo);
        }

        private IActionResult SavePhoto(ProductPhoto photo, IFormFile? uploadPhoto)
        {
            // Kiểm tra dữ liệu đầu vào
            if (photo == null || photo.ProductID <= 0)
            {
                return View(photo);
            }

            // Kiểm tra tính hợp lệ của các trường cần thiết
            ValidatePhoto(photo, uploadPhoto);

            // Thực hiện xử lý file ảnh nếu có
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                string fileName = $"{DateTime.Now.Ticks}-{Path.GetFileName(uploadPhoto.FileName)}";
                string filePath = Path.Combine(ApplicationContext.WebRootPath, "images/products", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                photo.Photo = fileName;
            }
            else if (string.IsNullOrEmpty(photo.Photo))
            {
                var existingPhoto = ProductDataService.GetPhoto(photo.PhotoID);
                if (existingPhoto != null)
                {
                    photo.Photo = existingPhoto.Photo;
                }
            }

            // Nếu ModelState hợp lệ, thêm hoặc cập nhật ảnh
            if (ModelState.IsValid)
            {
                if (photo.PhotoID == 0)
                {
                    long newPhotoId = ProductDataService.AddPhoto(photo);
                }
                else
                {
                    ProductDataService.UpdatePhoto(photo);
                }

                return RedirectToAction("Edit", new { id = photo.ProductID });
            }

            return View("Photo", photo);
        }

        /* public IActionResult Photo(int id = 0, string method = "", long photoId = 0, ProductPhoto photo = null, IFormFile? uploadPhoto = null)
         {
             ViewBag.ProductID = id;

             switch (method.ToLower())
             {
                 case "add":
                     ViewBag.Title = "Bổ sung ảnh";
                     photo = new ProductPhoto { ProductID = id };
                     return View(photo);

                 case "edit":
                     photo = ProductDataService.GetPhoto(photoId);
                     if (photo == null)
                     {
                         return NotFound();
                     }
                     ViewBag.Title = "Thay đổi ảnh";
                     break;

                 case "delete":
                     bool deleteResult = ProductDataService.DeletePhoto(photoId);

                     return RedirectToAction("Edit", new { id = id });
                 // Phương thức "save" của bạn
                 case "save":
                     // Kiểm tra dữ liệu đầu vào
                     if (photo == null || photo.ProductID <= 0)
                     {
                         return View(photo);
                     }

                     // Kiểm tra tính hợp lệ của các trường cần thiết
                     ValidatePhoto(photo, uploadPhoto);

                     // Thực hiện xử lý file ảnh nếu có
                     if (uploadPhoto != null && uploadPhoto.Length > 0)
                     {
                         // Tạo tên ảnh mới và lưu vào thư mục
                         string fileName = $"{DateTime.Now.Ticks}-{Path.GetFileName(uploadPhoto.FileName)}";
                         string filePath = Path.Combine(ApplicationContext.WebRootPath, "images/products", fileName);

                         using (var stream = new FileStream(filePath, FileMode.Create))
                         {
                             uploadPhoto.CopyTo(stream);
                         }

                         // Cập nhật lại ảnh mới
                         photo.Photo = fileName;
                     }
                     // Nếu không thay đổi ảnh, giữ nguyên ảnh cũ
                     else if (string.IsNullOrEmpty(photo.Photo))
                     {
                         // Nếu không thay đổi ảnh và ảnh chưa được lưu, giữ lại ảnh cũ (đã có trong database)
                         var existingPhoto = ProductDataService.GetPhoto(photo.PhotoID);
                         if (existingPhoto != null)
                         {
                             photo.Photo = existingPhoto.Photo; // Giữ lại ảnh cũ nếu không thay đổi
                         }
                     }

                     // Nếu ModelState hợp lệ, thêm hoặc cập nhật ảnh
                     if (ModelState.IsValid)
                     {
                         if (photo.PhotoID == 0)
                         {
                             long newPhotoId = ProductDataService.AddPhoto(photo);
                         }
                         else
                         {
                             ProductDataService.UpdatePhoto(photo);
                         }

                         return RedirectToAction("Edit", new { id = photo.ProductID });
                     }
                     break;


                 default:
                     break;
             }

             return View(photo);
         }
 */



        // Phương thức kiểm tra các trường hợp hợp lệ cho ProductPhoto
        private void ValidatePhoto(ProductPhoto photo, IFormFile? uploadPhoto)
        {
            if (string.IsNullOrWhiteSpace(photo.Description))
            {
                ModelState.AddModelError("Description", "Vui lòng nhập mô tả cho ảnh.");
            }

            if (photo.DisplayOrder < 1)
            {
                ModelState.AddModelError("DisplayOrder", "Thứ tự hiển thị phải lớn hơn 0.");
            }

            // Nếu không có ảnh mới và không có ảnh cũ, yêu cầu tải ảnh lên
            if (uploadPhoto == null && string.IsNullOrEmpty(photo.Photo) && photo.PhotoID == 0)
            {
                ModelState.AddModelError("uploadPhoto", "Vui lòng tải ảnh lên.");
            }
        }



        public IActionResult Attribute(int id = 0, string method = "", int attributeId = 0, ProductAttribute attribute = null)
        {
            ViewBag.ProductID = id;

            switch (method.ToLower())
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính sản phẩm";
                    attribute = new ProductAttribute { ProductID = id }; // Tạo mới thuộc tính
                    break;

                case "edit":
                    attribute = ProductDataService.GetAttribute(attributeId);
                    if (attribute == null)
                    {
                        return NotFound();
                    }
                    ViewBag.Title = "Thay đổi thuộc tính sản phẩm";
                    break;

                case "delete":
                    bool deleteResult = ProductDataService.DeleteAttribute(attributeId);
                    return RedirectToAction("Edit", new { id = id }); // Quay lại trang chỉnh sửa sản phẩm

                case "save":
                    return SaveAttribute(attribute); // Gọi phương thức SaveAttribute

                default:
                    break;
            }

            return View(attribute); // Trả về view với attribute
        }

        private IActionResult SaveAttribute(ProductAttribute attribute)
        {
            if (attribute == null || attribute.ProductID <= 0)
            {
                ModelState.AddModelError("", "Thông tin thuộc tính không hợp lệ.");
                return View(attribute);
            }

            // Validate các trường dữ liệu của attribute
            if (string.IsNullOrWhiteSpace(attribute.AttributeName))
            {
                ModelState.AddModelError("AttributeName", "Tên thuộc tính không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(attribute.AttributeValue))
            {
                ModelState.AddModelError("AttributeValue", "Giá trị thuộc tính không được để trống.");
            }

            if (attribute.DisplayOrder <= 0)
            {
                ModelState.AddModelError("DisplayOrder", "Thứ tự hiển thị phải lớn hơn 0.");
            }

            // Kiểm tra modelState trước khi lưu
            if (ModelState.IsValid)
            {
                if (attribute.AttributeID == 0)
                {
                    // Thêm mới thuộc tính
                    long newAttributeId = ProductDataService.AddAttribute(attribute);
                }
                else
                {
                    // Cập nhật thuộc tính
                    ProductDataService.UpdateAttribute(attribute);
                }
                return RedirectToAction("Edit", new { id = attribute.ProductID });
            }

            return View(attribute); // Trả về view với attribute nếu có lỗi
        }

        /* public IActionResult Attribute(int id = 0, string method = "", int attributeId = 0, ProductAttribute attribute = null)
         {
             ViewBag.ProductID = id;



             switch (method.ToLower())
             {
                 case "add":
                     ViewBag.Title = "Bổ sung thuộc tính sản phẩm";
                     attribute = new ProductAttribute { ProductID = id }; // Tạo mới thuộc tính
                     break;

                 case "edit":
                     attribute = ProductDataService.GetAttribute(attributeId);
                     if (attribute == null)
                     {
                         return NotFound();
                     }
                     ViewBag.Title = "Thay đổi thuộc tính sản phẩm";
                     break;

                 case "delete":
                     bool deleteResult = ProductDataService.DeleteAttribute(attributeId);
                     return RedirectToAction("Edit", new { id = id }); // Quay lại trang chỉnh sửa sản phẩm

                 case "save":
                     if (attribute == null || attribute.ProductID <= 0)
                     {
                         ModelState.AddModelError("", "Thông tin thuộc tính không hợp lệ.");
                         return View(attribute);
                     }

                     // Validate các trường dữ liệu của attribute
                     if (string.IsNullOrWhiteSpace(attribute.AttributeName))
                     {
                         ModelState.AddModelError("AttributeName", "Tên thuộc tính không được để trống.");
                     }

                     if (string.IsNullOrWhiteSpace(attribute.AttributeValue))
                     {
                         ModelState.AddModelError("AttributeValue", "Giá trị thuộc tính không được để trống.");
                     }
                     if (attribute.DisplayOrder <= 0)
                     {
                         ModelState.AddModelError("DisplayOrder", "Thứ tự hiển thị phải lớn hơn 0.");
                     }
                     // Kiểm tra modelState trước khi lưu
                     if (ModelState.IsValid)
                     {
                         if (attribute.AttributeID == 0)
                         {

                             // Thêm mới thuộc tính
                             long newAttributeId = ProductDataService.AddAttribute(attribute);
                             *//*if (newAttributeId == -1)
                             {
                                 // Lỗi trùng tên thuộc tính
                                 ModelState.AddModelError("AttributeName", "Tên thuộc tính này đã tồn tại.");
                                 return View(attribute);
                             }*//*
                         }
                         else
                         {
                             // Cập nhật thuộc tính
                             ProductDataService.UpdateAttribute(attribute);
                           *//*  bool result = ProductDataService.UpdateAttribute(attribute);
                             if (!result )
                             {
                                 // Lỗi trùng tên thuộc tính
                                 ModelState.AddModelError("AttributeName", "Tên thuộc tính này đã tồn tại.");
                                 return View(attribute);
                             }*//*

                         }
                         return RedirectToAction("Edit", new { id = attribute.ProductID }); // Quay lại trang chỉnh sửa
                     }
                     break;

                 default:

                     break;
             }

             return View(attribute); // Trả về view với attribute
         }



 */


        [HttpPost]
        public IActionResult Save(Product data, IFormFile? uploadPhoto)
        {
            ViewBag.Title = data.ProductID == 0 ? "Bổ sung sản phẩm" : "Cập nhật thông tin sản phẩm";

            // Kiểm tra thông tin đầu vào
            if (string.IsNullOrWhiteSpace(data.ProductName))
                ModelState.AddModelError(nameof(data.ProductName), "Vui lòng nhập tên mặt hàng");
            if (string.IsNullOrWhiteSpace(data.ProductDescription))
                ModelState.AddModelError(nameof(data.ProductDescription), "Vui lòng nhập mô tả mặt hàng");
            if (string.IsNullOrWhiteSpace(data.Unit))
                ModelState.AddModelError(nameof(data.Unit), "Vui lòng nhập đơn vị tính");

            if (data.CategoryID == 0)
            {
                ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại sản phẩm");
            }


            if (data.SupplierID == 0)
            {
                ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");
            }

            if (data.Price <= 0)
                ModelState.AddModelError(nameof(data.Price), "Vui lòng nhập giá sản phẩm phải lớn hơn 0");

            // Xử lý upload ảnh nếu có
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                // Kiểm tra định dạng file ảnh (nếu cần thiết)
                var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!validImageTypes.Contains(uploadPhoto.ContentType))
                {
                    ModelState.AddModelError(nameof(uploadPhoto), "Định dạng ảnh không hợp lệ. Vui lòng chọn ảnh JPG, PNG hoặc GIF.");
                }
                else
                {
                    // Lưu ảnh vào thư mục
                    string fileName = $"{DateTime.Now.Ticks}-{uploadPhoto.FileName}";
                    string filePath = Path.Combine(ApplicationContext.WebRootPath, @"images\products", fileName);

                    // Tạo thư mục nếu chưa có
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadPhoto.CopyTo(stream);
                    }

                    data.Photo = fileName; // Lưu tên file ảnh vào thuộc tính Photo của sản phẩm
                }
            }

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {

                return View("Edit", data); // Trả về form với thông báo lỗi
            }

            // Lưu dữ liệu vào database
            if (data.ProductID == 0)
            {
                // Thêm sản phẩm mới
                int id = ProductDataService.AddProduct(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.ProductName), "Tên sản phẩm không được trùng!");
                    return View("Edit", data);
                }
                return RedirectToAction("Edit",new {id = id });
            }
            else
            {
                // Cập nhật thông tin sản phẩm
                bool result = ProductDataService.UpdateProduct(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.ProductName), "Tên sản phẩm bị trùng!");
                    return View("Edit", data);
                }
            }

            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id = 0)
        {

            if (Request.Method == "POST")
            {
                // Xóa tất cả ảnh liên quan đến sản phẩm trước khi xóa sản phẩm
                ProductDataService.DeleteProductPhoto(id);
                ProductDataService.DeleteProductAttribute(id);

                // Gọi phương thức xóa sản phẩm từ ProductDataService
                bool result = ProductDataService.DeleteProduct(id);

                // Chuyển hướng về trang danh sách sản phẩm sau khi xóa
                return RedirectToAction("Index");
            }

            // Lấy thông tin sản phẩm 
            var product = ProductDataService.GetProduct(id);

            // Nếu không tìm thấy sản phẩm, chuyển hướng về trang danh sách
            if (product == null)
            {
                return RedirectToAction("Index");
            }
            bool isProductInUse = ProductDataService.InUsedProduct(id);

            // Truyền thông tin vào ViewBag
            ViewBag.IsProductInUse = isProductInUse;

            // Trả về view với thông tin sản phẩm để xác nhận xóa
            return View(product);
        }

    }
}



