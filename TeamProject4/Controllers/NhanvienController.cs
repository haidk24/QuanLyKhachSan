using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Team_Project_4.InterfacesRepositories;
using Team_Project_4.Models;
using Team_Project_4.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace Team_Project_4.Controllers
{
    public class NhanvienController : Controller
    {
        private readonly INhanvienRepository nhanvienRepo;

        public NhanvienController(INhanvienRepository nhanvienRepo_)
        {
            this.nhanvienRepo = nhanvienRepo_;
        }

        public async Task<IActionResult> NhanvienList(string searchString, string SortOrder, string sortColumn, int pageNumber = 1, string currentFilter = "")
        {
            ViewData["sortColumn"] = sortColumn;
            ViewData["sortOrder"] = SortOrder;
            ViewData["ManvSortParam"] = sortColumn == "Manv" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
            ViewData["HotenSortParam"] = sortColumn == "Hoten" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
            ViewData["PhaiSortParam"] = sortColumn == "Phai" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
            ViewData["NgaysinhSortParam"] = sortColumn == "Ngaysinh" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";

            // Reset pageNumber = 1 khi có search mới, giữ currentFilter
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var nhanviensList = await nhanvienRepo.GetAllAsync();
            var nhanviens = nhanviensList.AsQueryable();

            // Áp dụng filter trên toàn bộ dữ liệu
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                nhanviens = nhanviens.Where(n =>
                    n.Hoten != null && n.Hoten.ToLower().Contains(searchString) ||
                    n.Sdt != null && n.Sdt.ToLower().Contains(searchString) ||
                    n.Email != null && n.Email.ToLower().Contains(searchString) ||
                    n.Manv.ToString().ToLower().Contains(searchString));
            }

            // Sort logic
            switch (sortColumn)
            {
                case "Manv":
                    nhanviens = SortOrder == "desc" ? nhanviens.OrderByDescending(n => n.Manv) : nhanviens.OrderBy(n => n.Manv);
                    break;
                case "Hoten":
                    nhanviens = SortOrder == "desc" ? nhanviens.OrderByDescending(n => n.Hoten) : nhanviens.OrderBy(n => n.Hoten);
                    break;
                case "Phai":
                    nhanviens = SortOrder == "desc" ? nhanviens.OrderByDescending(n => n.Phai) : nhanviens.OrderBy(n => n.Phai);
                    break;
                case "Ngaysinh":
                    nhanviens = SortOrder == "desc" ? nhanviens.OrderByDescending(n => n.Ngaysinh) : nhanviens.OrderBy(n => n.Ngaysinh);
                    break;
                default:
                    nhanviens = nhanviens.OrderBy(n => n.Manv);
                    break;
            }

            // Phân trang thủ công
            int pageSize = 7;
            int totalItems = nhanviens.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages) pageNumber = totalPages;
            var pagedNhanviens = nhanviens.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(pagedNhanviens);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Nhanvien nhanvien)
        {
            if (!ModelState.IsValid)
            {
                return View(nhanvien);
            }
            var existingNhanvien = await nhanvienRepo.GetByEmailAsync(nhanvien.Email);

            if (existingNhanvien != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng");
                return View(nhanvien);
            }
            await nhanvienRepo.AddAsync(nhanvien);
            TempData["CreateSuccess"] = "Thêm nhân viên thành công!";
            return RedirectToAction("NhanvienList");
        }

        public async Task<IActionResult> Update(string nhanvienid)
        {
            var nhanvien = await nhanvienRepo.GetByIdAsync(int.Parse(nhanvienid));
            return View(nhanvien);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Nhanvien nhanvien, string nhanvienid)
        {
            if (!ModelState.IsValid)
            {
                return View(nhanvien);
            }
            int id = int.Parse(nhanvienid);
            var existingNhanvien = await nhanvienRepo.CheckEmailExist(nhanvien.Email, id);

            if (existingNhanvien != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng");
                return View(nhanvien);
            }

            await nhanvienRepo.UpdateAsync(nhanvien, id);
            TempData["UpdateSuccess"] = "Cập nhật nhân viên thành công!";
            return RedirectToAction("NhanvienList");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string nhanvienid)
        {
            await nhanvienRepo.DeleteAsync(int.Parse(nhanvienid));
            TempData["DeleteSuccess"] = "Xóa nhân viên thành công!";
            return RedirectToAction("NhanvienList");
        }
    }
}