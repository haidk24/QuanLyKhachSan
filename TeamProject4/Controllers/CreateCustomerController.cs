using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Policy;
using Team_Project_4.InterfacesRepositories;
using Team_Project_4.Models;
using Team_Project_4.Repositories;

namespace Team_Project_4.Controllers
{
    public class CreateCustomerController : Controller
    {
        private readonly IKhachhangRepository clientRepo;
        private readonly IPhongRepository roomRepo;
        public CreateCustomerController(IKhachhangRepository clientRepo_, IPhongRepository roomRepo_)
        {
            this.clientRepo = clientRepo_;
            this.roomRepo = roomRepo_;
        }
        public async Task<IActionResult> Create(string id, string value1,int manager)
        {
            int ID = int.Parse(id);
            TempData["Manager"] = manager;
            var clientTypes = await clientRepo.GetAllLoaikhach();
            var rooms = await roomRepo.GetByIdAsync(ID);
            ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");
            ViewData["TenPhong"] = rooms.Tenphong;
            TempData["TempMapt"] = int.Parse(value1); // Thay đổi tên biến
            TempData["TempMap"] = int.Parse(id);
 
            return View();
        }

        // POST: Customer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Khachhang khach, int id, int value1, IFormFile AnhCccdMatTruocFile, IFormFile AnhCccdMatSauFile)
        {
            // Xử lý upload ảnh CCCD mặt trước
            if (AnhCccdMatTruocFile != null && AnhCccdMatTruocFile.Length > 0)
            {
                var result = await ProcessImageUpload(AnhCccdMatTruocFile, "mặt trước");
                if (result.IsError)
                {
                    ModelState.AddModelError("AnhCccdMatTruoc", result.ErrorMessage);
                    await ReloadViewBags(id);
                    return View(khach);
                }
                khach.AnhCccdMatTruoc = result.FilePath;
            }
            
            // Xử lý upload ảnh CCCD mặt sau
            if (AnhCccdMatSauFile != null && AnhCccdMatSauFile.Length > 0)
            {
                var result = await ProcessImageUpload(AnhCccdMatSauFile, "mặt sau");
                if (result.IsError)
                {
                    ModelState.AddModelError("AnhCccdMatSau", result.ErrorMessage);
                    
                    // Xóa ảnh mặt trước đã upload nếu có lỗi ở mặt sau
                    if (!string.IsNullOrEmpty(khach.AnhCccdMatTruoc))
                    {
                        DeleteImageFile(khach.AnhCccdMatTruoc);
                    }
                    
                    await ReloadViewBags(id);
                    return View(khach);
                }
                khach.AnhCccdMatSau = result.FilePath;
            }

            khach.Map = id;
            await clientRepo.AddAsync(khach);
            return RedirectToAction("Details", "Rent", new { id = value1 });
        }

        // Helper method to process image upload
        private async Task<(bool IsError, string ErrorMessage, string FilePath)> ProcessImageUpload(IFormFile imageFile, string imageName)
        {
            // Kiểm tra kích thước file (tối đa 5MB)
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                return (true, $"File ảnh {imageName} quá lớn. Vui lòng chọn file nhỏ hơn 5MB.", null);
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return (true, $"Chỉ chấp nhận file ảnh {imageName} có định dạng JPG, PNG hoặc GIF.", null);
            }

            // Tạo thư mục uploads nếu chưa tồn tại
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cccd");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file unique
            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return (false, null, "/uploads/cccd/" + uniqueFileName);
        }

        // Helper method to delete image file
        private void DeleteImageFile(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    try
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    catch (Exception)
                    {
                        // Log error but continue
                    }
                }
            }
        }

        // Helper method to reload ViewBags
        private async Task ReloadViewBags(int id)
        {
            var clientTypes = await clientRepo.GetAllLoaikhach();
            var rooms = await roomRepo.GetByIdAsync(id);
            ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");
            ViewData["TenPhong"] = rooms.Tenphong;
        }
        
    }
}
