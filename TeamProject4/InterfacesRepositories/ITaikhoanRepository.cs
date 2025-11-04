using System.Collections.Generic;
using System.Threading.Tasks;
using Team_Project_4.Models;

namespace Team_Project_4.InterfacesRepositories
{
    public interface ITaikhoanRepository
    {
        Task AddAsync(Taikhoan taikhoan);
        Task UpdateByNv(int manv, string newEmail);
        Task<bool> ToggleAccountStatus(int manv, bool activate);
        Task<Taikhoan> GetByUsernameAndPasswordAsync(string tentknv, string mktk);
        Task CreateAccountForAllEmployee(IEnumerable<Taikhoan> accounts);
        Task<Taikhoan> GetByUsernameAsync(string tentknv);
        Task<List<Taikhoan>> GetAllAsync();
        Task<Taikhoan> GetByManvAsync(int manv); // Thêm phương thức mới
    }
}