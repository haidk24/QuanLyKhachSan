using Microsoft.AspNetCore.Identity;
using System;
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

        // Thêm cột IsActive
        public bool IsActive { get; set; } = true;

        // Giữ navigation property nhưng đánh dấu NotMapped để tránh binding trực tiếp
        [NotMapped]
        public virtual Nhanvien? ManvNavigation { get; set; } = null;
    }
}