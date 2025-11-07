using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Team_Project_4.InterfacesRepositories;
using Team_Project_4.Models;
using Team_Project_4.Repositories;
using Team_Project_4.ViewModels;

namespace Team_Project_4.Controllers
{
    public class ClientController : Controller
    {
        private readonly IKhachhangRepository clientRepo;
        private readonly IPhongRepository roomRepo;
        private readonly IRentRepository rentRepo;
        public ClientController(IKhachhangRepository clientRepo_, IPhongRepository roomRepo_, IRentRepository rentrepo)
        {
            this.clientRepo = clientRepo_;
            this.roomRepo = roomRepo_;
            this.rentRepo = rentrepo;
        }

        public async Task<IActionResult> ClientList(int manager,string searchString, string clientType, string SortOrder, string sortColumn, int pageNumber, string currentFilter, string currentFilter2,int rentid)
        {
            if (rentid !=0)
            {
                TempData["Manager"] = manager;
                return RedirectToAction("Details", "Rent", new { id = rentid });
            }
            TempData["Manager"] = manager;
            ViewData["sortColumn"] = sortColumn;
            ViewData["sortOrder"] = SortOrder;
            ViewData["MaSortParam"] = sortColumn == "Makh" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
            ViewData["TenSortParam"] = sortColumn == "Tenkh" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
            ViewData["TinhSortParam"] = sortColumn == "Tuoi" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            if (clientType != null)
            {
                pageNumber = 1;
            }
            else
            {
                clientType = currentFilter2;
            }

            ViewData["CurrentFilter2"] = clientType;
            ViewData["CurrentFilter"] = searchString;
            var khachs = clientRepo.GetAllAsync();
            if (!string.IsNullOrEmpty(searchString))
            {
                khachs = khachs.Where(cl => cl.Tenkh != null && cl.Tenkh.ToLower().Contains(searchString.ToLower()));
            }
            if (!string.IsNullOrEmpty(clientType))
            {
                khachs = khachs.Where(r => r.MaloaikhachNavigation.Tenloaikhach == clientType);
            }
            switch (sortColumn)
            {
                case "Makh":
                    khachs = SortOrder == "desc" ? khachs.OrderByDescending(cl => cl.Makh) : khachs.OrderBy(cl => cl.Makh);
                    break;
                case "Tenkh":
                    khachs = SortOrder == "desc" ? khachs.OrderByDescending(cl => cl.Tenkh) : khachs.OrderBy(cl => cl.Tenkh);
                    break;
                case "Tuoi":
                    khachs = SortOrder == "desc" ? khachs.OrderByDescending(cl => cl.Tuoi) : khachs.OrderBy(cl => cl.Tuoi);
                    break;


                default:
                    khachs = khachs.OrderBy(cl => cl.Makh);
                    break;
            }

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }
            int pageSize = 7;
            var clientTypes = await clientRepo.GetDistinctClientTypeAsync();
            var clientTypeItems = clientTypes.Select(ct => new SelectListItem { Value = ct, Text = ct }).ToList();
            clientTypeItems.Insert(0, new SelectListItem { Value = "", Text = "Loại Khách" });
            ViewBag.ClientTypeList = clientTypeItems;

            var errorMessage = TempData["ErrorMessage"] as string;
            ViewBag.ErrorMessage = errorMessage;
            return View(await PaginatedList<Khachhang>.CreateAsync(khachs, pageNumber, pageSize));

        }

        public async Task<IActionResult> Create(int manager)
        {
            TempData["Manager"] = manager;
            var clientTypes = await clientRepo.GetAllLoaikhach();
            var rooms = await roomRepo.GetAllAsync().ToListAsync();
            ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");
            ViewBag.ClientRoom = new SelectList(rooms, "Map", "Tenphong");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Khachhang khach, int manager, IFormFile AnhCccdMatTruocFile, IFormFile AnhCccdMatSauFile)
        {
            TempData["Manager"] = manager;
            
            // Xử lý upload ảnh CCCD mặt trước
            if (AnhCccdMatTruocFile != null && AnhCccdMatTruocFile.Length > 0)
            {
                var result = await ProcessImageUpload(AnhCccdMatTruocFile, "mặt trước");
                if (result.IsError)
                {
                    ModelState.AddModelError("AnhCccdMatTruoc", result.ErrorMessage);
                    await ReloadViewBags();
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
                    
                    await ReloadViewBags();
                    return View(khach);
                }
                khach.AnhCccdMatSau = result.FilePath;
            }
            
            khach.MaloaikhachNavigation = await clientRepo.GetClientTypeById(khach.Maloaikhach);

            if (string.IsNullOrEmpty(khach.Tenkh))
            {
                var clientTypes = await clientRepo.GetAllLoaikhach();
                var roomList = await roomRepo.GetAllAsync().ToListAsync();
                ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");
                ViewBag.ClientRoom = new SelectList(roomList, "Map", "Tenphong");
                return View(khach);
            }

            var room = await roomRepo.GetByIdAsync(khach.Map);
            if (room == null)
            {
                ModelState.AddModelError("", "Phòng không tồn tại.");
            }
            else
            {
                var currentClientCount = await clientRepo.CountClientsByRoomId(khach.Map);

                if (currentClientCount >= room.Soluongkhachtoida)
                {
                    ModelState.AddModelError("", "Phòng đã đạt số lượng khách tối đa, không thể thêm khách mới.");

                    var clientTypes = await clientRepo.GetAllLoaikhach();
                    var roomList = await roomRepo.GetAllAsync().ToListAsync();
                    ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");
                    ViewBag.ClientRoom = new SelectList(roomList, "Map", "Tenphong");
                    return View(khach);
                }

                await clientRepo.AddAsync(khach);
                return RedirectToAction("ClientList", new { manager });
            }

            var clientTypesReload = await clientRepo.GetAllLoaikhach();
            var roomReload = await roomRepo.GetAllAsync().ToListAsync();
            ViewBag.ClientType = new SelectList(clientTypesReload, "Maloaikhach", "Tenloaikhach");
            ViewBag.ClientRoom = new SelectList(roomReload, "Map", "Tenphong");
            return View(khach);
        }


        public async Task<IActionResult> Update(int clientid,int manager,int value1)
        {
            Debug.WriteLine("id :" + value1);
            TempData["TempMapt"] = value1;
            TempData["Manager"] = manager;
            var client = await clientRepo.GetByIdAsync(clientid);
            var clientTypes = await clientRepo.GetAllLoaikhach(); // Fetch client types again
            ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");

            var rooms = roomRepo.GetAllAsync();
            ViewBag.ClientRoom = new SelectList(rooms, "Map", "Tenphong");
            return View(client);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Khachhang khach, int manager, int clientid, int value1, IFormFile AnhCccdMatTruocFile, IFormFile AnhCccdMatSauFile)
        {
            khach.Makh = clientid;

            // Lấy thông tin khách hàng hiện tại để có ảnh cũ
            var existingClient = await clientRepo.GetByIdAsync(clientid);
            
            // Xử lý upload ảnh CCCD mặt trước mới
            if (AnhCccdMatTruocFile != null && AnhCccdMatTruocFile.Length > 0)
            {
                var result = await ProcessImageUpload(AnhCccdMatTruocFile, "mặt trước");
                if (result.IsError)
                {
                    ModelState.AddModelError("AnhCccdMatTruoc", result.ErrorMessage);
                    TempData["TempMapt"] = value1;
                    TempData["Manager"] = manager;
                    var clientTypes = await clientRepo.GetAllLoaikhach();
                    ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");
                    var rooms = roomRepo.GetAllAsync();
                    ViewBag.ClientRoom = new SelectList(rooms, "Map", "Tenphong");
                    return View(khach);
                }

                // Xóa ảnh mặt trước cũ nếu tồn tại
                if (!string.IsNullOrEmpty(existingClient.AnhCccdMatTruoc))
                {
                    DeleteImageFile(existingClient.AnhCccdMatTruoc);
                }

                khach.AnhCccdMatTruoc = result.FilePath;
            }
            else
            {
                // Giữ nguyên ảnh mặt trước cũ nếu không upload ảnh mới
                khach.AnhCccdMatTruoc = existingClient.AnhCccdMatTruoc;
            }

            // Xử lý upload ảnh CCCD mặt sau mới
            if (AnhCccdMatSauFile != null && AnhCccdMatSauFile.Length > 0)
            {
                var result = await ProcessImageUpload(AnhCccdMatSauFile, "mặt sau");
                if (result.IsError)
                {
                    ModelState.AddModelError("AnhCccdMatSau", result.ErrorMessage);
                    TempData["TempMapt"] = value1;
                    TempData["Manager"] = manager;
                    var clientTypes = await clientRepo.GetAllLoaikhach();
                    ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");
                    var rooms = roomRepo.GetAllAsync();
                    ViewBag.ClientRoom = new SelectList(rooms, "Map", "Tenphong");
                    return View(khach);
                }

                // Xóa ảnh mặt sau cũ nếu tồn tại
                if (!string.IsNullOrEmpty(existingClient.AnhCccdMatSau))
                {
                    DeleteImageFile(existingClient.AnhCccdMatSau);
                }

                khach.AnhCccdMatSau = result.FilePath;
            }
            else
            {
                // Giữ nguyên ảnh mặt sau cũ nếu không upload ảnh mới
                khach.AnhCccdMatSau = existingClient.AnhCccdMatSau;
            }

            await clientRepo.UpdateAsync(khach,clientid);
            return RedirectToAction("ClientList", new { rentid=value1});
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int clientid, int manager, int value1)
        {
            var rent = await rentRepo.GetAllAsync();
            if (rent.Any(r => r.Makh == clientid))
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng đã có trong danh sách thuê.";
            }
            else
            {
                // Lấy thông tin khách hàng trước khi xóa để có đường dẫn ảnh
                var client = await clientRepo.GetByIdAsync(clientid);
                
                // Xóa ảnh CCCD nếu tồn tại
                if (client != null)
                {
                    if (!string.IsNullOrEmpty(client.AnhCccdMatTruoc))
                    {
                        DeleteImageFile(client.AnhCccdMatTruoc);
                    }
                    
                    if (!string.IsNullOrEmpty(client.AnhCccdMatSau))
                    {
                        DeleteImageFile(client.AnhCccdMatSau);
                    }
                }
                
                // Xóa khách hàng
                await clientRepo.DeleteAsync(clientid);
            }

            return RedirectToAction("ClientList", new { manager = manager, rentid = value1 });
        }



        public async Task<IActionResult> Details(int clientid, int manager)
        {

            TempData["Manager"] = manager;
            var client = await clientRepo.GetByIdAsync(clientid);

            if (client == null)
            {
                return NotFound(); // Handle not found client
            }

            return View(client);
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
        private async Task ReloadViewBags()
        {
            var clientTypes = await clientRepo.GetAllLoaikhach();
            var roomList = await roomRepo.GetAllAsync().ToListAsync();
            ViewBag.ClientType = new SelectList(clientTypes, "Maloaikhach", "Tenloaikhach");
            ViewBag.ClientRoom = new SelectList(roomList, "Map", "Tenphong");
        }
    }
}

