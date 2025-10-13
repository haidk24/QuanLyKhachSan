using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Team_Project_4.Models
{
    public partial class Nhanvien
    {
        public Nhanvien()
        {
            Hoadons = new HashSet<Hoadon>();
            Taikhoans = new HashSet<Taikhoan>();
        }

        public int Manv { get; set; }

        [Required(ErrorMessage = "Họ tên không thể thiếu")]
        public string Hoten { get; set; } = null!;

        [Required(ErrorMessage = "Phái không thể thiếu")]
        public string Phai { get; set; } = null!;

        [Required(ErrorMessage = "Ngày sinh không thể thiếu")]
        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không hợp lệ")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]  // Force dd/MM/yyyy cho input/display/edit
        public DateTime Ngaysinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại không thể thiếu")]
        // Chỉ chấp nhận đúng 10 chữ số (0-9). Không cho phép dấu +.
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số")]
        public string Sdt { get; set; } = null!;

        [Required(ErrorMessage = "Email không thể thiếu")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        // Đảm bảo thêm hạn chế nếu cần (EmailAddress đã kiểm tra @ và cấu trúc cơ bản)
        public string Email { get; set; } = null!;

        public string? Diachi { get; set; } = null!;

        public virtual ICollection<Hoadon> Hoadons { get; set; }
        public virtual ICollection<Taikhoan> Taikhoans { get; set; }
    }
}