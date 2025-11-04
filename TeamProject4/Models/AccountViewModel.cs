using System;

namespace Team_Project_4.ViewModels
{
    public class AccountViewModel
    {
        public int Manv { get; set; }
        public string Hoten { get; set; } = null!;
        public string Sdt { get; set; } = null!;
        public string Gmail { get; set; } = null!;
        public string Tentknv { get; set; } = null!;
        public string Mktk { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}