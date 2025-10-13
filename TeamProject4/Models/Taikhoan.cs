using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Team_Project_4.Models
{
    public partial class Taikhoan
    {
        public int Matknv { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        public string Tentknv { get; set; } = null!;
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu tài khoản")]
        public string Mktk { get; set; } = null!;
        public int Manv { get; set; }

        // FIX: Làm nullable, bỏ null! để không required validation. [NotMapped] nếu không cần EF load navigation khi insert
        [NotMapped] // Bỏ qua validation/insert cho navigation (chỉ dùng FK Manv)
        public virtual Nhanvien? ManvNavigation { get; set; } = null; // Nullable, không required
    }
}