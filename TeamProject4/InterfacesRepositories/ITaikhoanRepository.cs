using System.Threading.Tasks;
using System.Collections.Generic;
using Team_Project_4.Models;

namespace Team_Project_4.InterfacesRepositories
{
    public interface ITaikhoanRepository
    {
        Task<Taikhoan> GetByUsernameAndPasswordAsync(string tentknv, string mktk);
        Task AddAsync(Taikhoan taikhoan);
        Task DeleteByManv(int manv);
        Task UpdateByNv(int manv, string newEmail);
        Task CreateAccountForAllEmployee(IEnumerable<Taikhoan> accounts);
        Task<Taikhoan> GetByUsernameAsync(string tentknv);
        Task<List<Taikhoan>> GetAllAsync(); // Thêm method lấy toàn bộ tài khoản
    }
}