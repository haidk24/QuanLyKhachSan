using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Team_Project_4.InterfacesRepositories;
using Team_Project_4.Models;
using Team_Project_4.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Text.Json; // Để serialize JSON
using System.Text.RegularExpressions; // Để xử lý tên
using System.Text;
using System.Globalization;

namespace Team_Project_4.Controllers
{
    public class AccountController : Controller
    {
        private readonly INhanvienRepository _nvrepo;
        private readonly ITaikhoanRepository _taikhoanRepo;

        public AccountController(INhanvienRepository nvrepo, ITaikhoanRepository taikhoanRepo)
        {
            _nvrepo = nvrepo;
            _taikhoanRepo = taikhoanRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Register()
        {
            var nhanVienId = await _nvrepo.GetEmployeeNoAccount();
            ViewBag.EmployeeIdList = new SelectList(nhanVienId, "Manv", "Manv");
            ViewBag.EmployeeEmailList = new SelectList(nhanVienId, "Manv", "Email");

            ViewBag.EmployeeList = JsonSerializer.Serialize(nhanVienId, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            ViewBag.RoleList = new SelectList(new[]
            {
                new { Value = "NhanVien", Text = "Nhân viên (@staff.hotel)" },
                new { Value = "QuanLy", Text = "Quản lý (@manager.hotel)" }
            }, "Value", "Text");

            // Lấy danh sách tài khoản đã tạo với AccountViewModel
            ViewBag.AccountList = await GetAccounts();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Taikhoan taikhoan, string selectedRole)
        {
            if (taikhoan.Manv == 0)
            {
                try
                {
                    var nhanVienList = await _nvrepo.GetEmployeeNoAccount();
                    if (!nhanVienList.Any())
                    {
                        TempData["Error"] = "Tất cả nhân viên đều đã có tài khoản";
                        return View();
                    }

                    var newAccounts = new List<Taikhoan>();
                    string suffix = selectedRole == "NhanVien" ? "@staff.hotel" : "@manager.hotel";

                    foreach (var nv in nhanVienList)
                    {
                        var generatedTk = await GenerateTentknv(nv, suffix);
                        if (string.IsNullOrEmpty(generatedTk))
                        {
                            TempData["Error"] = "Lỗi generate Tentknv cho nhân viên " + nv.Manv;
                            return View();
                        }

                        var newTk = new Taikhoan
                        {
                            Manv = nv.Manv,
                            Tentknv = generatedTk,
                            Mktk = "123456789"
                        };

                        newAccounts.Add(newTk);
                    }

                    await _taikhoanRepo.CreateAccountForAllEmployee(newAccounts);
                    TempData["Success"] = "Cấp tài khoản thành công cho tất cả nhân viên!";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Lỗi CreateAccountForAllEmployee: " + ex.Message);
                    TempData["Error"] = "Lỗi khi cấp tài khoản: " + ex.Message;
                }
            }
            else
            {
                try
                {
                    var selectedNv = await _nvrepo.GetByIdAsync(taikhoan.Manv);
                    if (selectedNv == null)
                    {
                        TempData["Error"] = "Không tìm thấy nhân viên";
                        return View();
                    }

                    string suffix = selectedRole == "NhanVien" ? "@staff.hotel" : "@manager.hotel";
                    taikhoan.Tentknv = await GenerateTentknv(selectedNv, suffix);
                    taikhoan.Mktk = "123456789";

                    if (string.IsNullOrEmpty(taikhoan.Tentknv))
                    {
                        TempData["Error"] = "Lỗi generate Tentknv";
                        return View();
                    }

                    taikhoan.Manv = selectedNv.Manv;

                    if (ModelState.IsValid)
                    {
                        await _taikhoanRepo.AddAsync(taikhoan);
                        TempData["Success"] = "Cấp tài khoản thành công cho " + selectedNv.Hoten;
                    }
                    else
                    {
                        TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Lỗi AddAsync Taikhoan: " + ex.Message);
                    TempData["Error"] = "Lỗi khi cấp tài khoản: " + ex.Message;
                }
            }

            // Reload ViewBag nếu error
            var nhanVienId = await _nvrepo.GetEmployeeNoAccount();
            ViewBag.EmployeeIdList = new SelectList(nhanVienId, "Manv", "Manv");
            ViewBag.EmployeeEmailList = new SelectList(nhanVienId, "Manv", "Email");
            ViewBag.EmployeeList = JsonSerializer.Serialize(nhanVienId, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            ViewBag.RoleList = new SelectList(new[]
            {
                new { Value = "NhanVien", Text = "Nhân viên (@staff.hotel)" },
                new { Value = "QuanLy", Text = "Quản lý (@manager.hotel)" }
            }, "Value", "Text");
            ViewBag.AccountList = await GetAccounts();

            return View(taikhoan);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int manv)
        {
            try
            {
                await _taikhoanRepo.DeleteByManv(manv);
                TempData["Success"] = "Xóa tài khoản thành công!";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi DeleteAccount: " + ex.Message);
                TempData["Error"] = "Lỗi khi xóa tài khoản: " + ex.Message;
            }
            return RedirectToAction("Register");
        }

        private async Task<List<AccountViewModel>> GetAccounts()
        {
            var accounts = await _taikhoanRepo.GetAllAsync();
            var accountList = new List<AccountViewModel>();
            foreach (var tk in accounts)
            {
                var nv = await _nvrepo.GetByIdAsync(tk.Manv);
                if (nv != null)
                {
                    accountList.Add(new AccountViewModel
                    {
                        Manv = tk.Manv,
                        Hoten = nv.Hoten,
                        Sdt = nv.Sdt,
                        Gmail = tk.Tentknv, // Tentknv chứa @domain
                        Tentknv = tk.Tentknv,
                        Mktk = tk.Mktk
                    });
                }
            }
            return accountList;
        }

        private async Task<string> GenerateTentknv(Nhanvien nv, string suffix)
        {
            string fullNameNormalized = RemoveVietnameseAccents(nv.Hoten).ToLower().Replace(" ", "");
            string lastNameNormalized = fullNameNormalized.Split(new[] { ' ', '.', '-' }).Last();
            string dobStr = nv.Ngaysinh.ToString("ddMMyyyy");
            string baseUsername = lastNameNormalized + dobStr;

            string tentknv = baseUsername + suffix;

            var existingTk = await _taikhoanRepo.GetByUsernameAsync(tentknv);
            if (existingTk != null)
            {
                tentknv = fullNameNormalized + baseUsername + suffix;
            }

            return tentknv;
        }

        private string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Chuẩn hoá về FormD (tách ký tự base và dấu)
            string normalized = text.Normalize(NormalizationForm.FormD);

            var sb = new System.Text.StringBuilder();

            foreach (var c in normalized)
            {
                // Loại bỏ các ký tự thuộc loại NonSpacingMark (dấu)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            // Chuẩn hoá lại về FormC và chuyển đ/Đ -> d/D
            string result = sb.ToString().Normalize(NormalizationForm.FormC)
                              .Replace('đ', 'd')
                              .Replace('Đ', 'D');

            return result;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Taikhoan tk)
        {
            var check = await _taikhoanRepo.GetByUsernameAndPasswordAsync(tk.Tentknv, tk.Mktk);

            if (check != null)
            {
                var role = check.Tentknv.Contains("@staff.hotel") ? "Staff" : "Manager";
                var claims = new List<Claim>
                {
                    new Claim("Role", role)
                };

                var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(userIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                HttpContext.Session.SetString("accname", check.Tentknv);
                HttpContext.Session.SetString("UserRole", role); // Thêm dòng này để lưu UserRole

                if (role == "Staff")
                {
                    return RedirectToAction("Index", "Staff");
                }
                else
                {
                    return RedirectToAction("Index", "Manager");
                }
            }

            ViewBag.error = "Đăng nhập thất bại";
            return View();
        }

        public IActionResult Logout()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("accname")))
            {
                HttpContext.Session.SetString("accname", "");
                HttpContext.Session.SetString("UserRole", ""); // Xóa UserRole khỏi session
                return RedirectToAction("Login", "Account");
            }
            return RedirectToAction("Login", "Account");
        }
    }
}