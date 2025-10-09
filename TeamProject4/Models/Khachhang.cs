using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Team_Project_4.Models
{
    public partial class Khachhang
    {
        public Khachhang()
        {
            Phieuthues = new HashSet<Phieuthue>();
        }

        public int Makh { get; set; }
        [Required(ErrorMessage="Vui lòng nhập tên")]
        public string Tenkh { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tuổi")]
        [Range(18, 100, ErrorMessage = "Tuổi phải từ 18 đến 100")]
        public int? Tuoi { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập sdt")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0[1-9][0-9]{8,9})$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10-11 số")]
        public string Tel { get; set; }
        public string? Diachikh { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập CCCD hoặc hộ chiếu")]
        public string Cmndkh { get; set; }
        public int Maloaikhach { get; set; }
        public int Map { get; set; }

        public virtual Loaikhach MaloaikhachNavigation { get; set; } = null!;
        public virtual Phong MapNavigation { get; set; } = null!;
        public virtual ICollection<Phieuthue> Phieuthues { get; set; }
    }
}
