using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Team_Project_4.InterfacesRepositories;
using Team_Project_4.Models;
using Team_Project_4.Repositories;
using Team_Project_4.ViewModels;

namespace Team_Project_4.Controllers
{
    public class NhanvienController : Controller
    {
        private readonly INhanvienRepository nhanvienRepo;

        public NhanvienController(INhanvienRepository nhanvienRepo_)
        {
            this.nhanvienRepo = nhanvienRepo_;
        }

        public async Task<IActionResult> NhanvienList(string searchString, string SortOrder, string sortColumn, int pageNumber, string currentFilter)
        {
            ViewData["sortColumn"] = sortColumn;
            ViewData["sortOrder"] = SortOrder;
            ViewData["ManvSortParam"] = sortColumn == "Manv" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
            ViewData["HotenSortParam"] = sortColumn == "Hoten" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
            ViewData["PhaiSortParam"] = sortColumn == "Phai" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
            ViewData["NgaysinhSortParam"] = sortColumn == "Ngaysinh" ? (SortOrder == "asc" ? "desc" : "asc") : "asc";

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

            if (!string.IsNullOrEmpty(searchString))
            {
                nhanviens = nhanviens.Where(n => n.Hoten != null && n.Hoten.ToLower().Contains(searchString.ToLower()));
            }

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

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            int pageSize = 7;
            return View(await PaginatedList<Nhanvien>.CreateAsync(nhanviens, pageNumber, pageSize));
        }

        public async Task<IActionResult> Create()
        {
            return View(new Nhanvien()); // Pass model rỗng để tránh null
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật CSRF
        public async Task<IActionResult> Create(Nhanvien nhanvien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check email unique trước khi add
                    var existing = await nhanvienRepo.GetByEmailAsync(nhanvien.Email);
                    if (existing != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng");
                    }
                    else
                    {
                        await nhanvienRepo.AddAsync(nhanvien);
                        TempData["CreateSuccess"] = "Thêm nhân viên thành công!"; // FIX: Key riêng cho Create
                        return RedirectToAction("NhanvienList");
                    }
                }
                catch (Exception ex)
                {
                    // Catch lỗi DB
                    TempData["CreateError"] = "Lỗi khi lưu: " + ex.Message; // FIX: Key riêng cho Create
                }
            }
            return View(nhanvien); // Return view với model để hiển thị lỗi
        }

        public async Task<IActionResult> Update(string nhanvienid)
        {
            if (string.IsNullOrEmpty(nhanvienid) || !int.TryParse(nhanvienid, out int id))
            {
                return NotFound("ID không hợp lệ");
            }

            var nhanvien = await nhanvienRepo.GetByIdAsync(id);

            if (nhanvien == null)
            {
                return NotFound("Không tìm thấy nhân viên");
            }

            return View(nhanvien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật CSRF
        public async Task<IActionResult> Update(Nhanvien nhanvien)
        {
            if (nhanvien.Manv <= 0) // Check ID từ model
            {
                return BadRequest("ID nhân viên không hợp lệ");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check email unique (ngoại trừ chính record này)
                    var existingEmail = await nhanvienRepo.GetByEmailAsync(nhanvien.Email);
                    if (existingEmail != null && existingEmail.Manv != nhanvien.Manv)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi nhân viên khác");
                    }
                    else
                    {
                        await nhanvienRepo.UpdateAsync(nhanvien, nhanvien.Manv); // Update với ID từ model
                        TempData["UpdateSuccess"] = "Cập nhật nhân viên thành công!"; // FIX: Key riêng cho Update
                        return RedirectToAction("NhanvienList");
                    }
                }
                catch (Exception ex)
                {
                    // Catch lỗi DB
                    TempData["UpdateError"] = "Lỗi khi cập nhật: " + ex.Message; // FIX: Key riêng cho Update
                }
            }

            // Nếu invalid, return view với model để hiển thị lỗi
            return View(nhanvien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật CSRF
        public async Task<IActionResult> Delete(string nhanvienid)
        {
            if (!int.TryParse(nhanvienid, out int id))
            {
                return BadRequest("ID không hợp lệ");
            }

            try
            {
                await nhanvienRepo.DeleteAsync(id);
                TempData["DeleteSuccess"] = "Xóa nhân viên thành công!"; // Thêm key riêng cho Delete nếu cần
            }
            catch (Exception ex)
            {
                TempData["DeleteError"] = "Lỗi khi xóa: " + ex.Message;
            }

            return RedirectToAction("NhanvienList");
        }
    }
}