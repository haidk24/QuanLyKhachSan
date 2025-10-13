using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Team_Project_4.InterfacesRepositories;
using Team_Project_4.Models;

namespace Team_Project_4.Repositories
{
    public class TaikhoanRepository : ITaikhoanRepository
    {
        private readonly HotelDbContext _dbContext;

        public TaikhoanRepository(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Taikhoan taikhoan)
        {
            try
            {
                if (taikhoan == null)
                {
                    Debug.WriteLine("Taikhoan null, không add");
                    return;
                }

                _dbContext.Taikhoans.Add(taikhoan);
                await _dbContext.SaveChangesAsync();
                Debug.WriteLine("Add Taikhoan thành công: " + taikhoan.Tentknv);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi AddAsync Taikhoan: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
                throw;
            }
        }

        public async Task UpdateByNv(int manv, string newEmail)
        {
            Taikhoan tk = await _dbContext.Taikhoans.FirstOrDefaultAsync(tk => tk.Manv == manv);
            if (tk != null)
            {
                tk.Tentknv = newEmail;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteByManv(int manv)
        {
            Taikhoan tk = await _dbContext.Taikhoans.FirstOrDefaultAsync(tk => tk.Manv == manv);
            if (tk != null)
            {
                Debug.WriteLine("id tk: " + tk.Matknv);
                _dbContext.Remove(tk);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Taikhoan> GetByUsernameAndPasswordAsync(string tentknv, string mktk)
        {
            return await _dbContext.Taikhoans.FirstOrDefaultAsync(t => t.Tentknv == tentknv && t.Mktk == mktk);
        }

        public async Task CreateAccountForAllEmployee(IEnumerable<Taikhoan> accounts)
        {
            try
            {
                if (accounts == null || !accounts.Any())
                {
                    Debug.WriteLine("Danh sách accounts rỗng, không create");
                    return;
                }

                _dbContext.Taikhoans.AddRange(accounts);
                await _dbContext.SaveChangesAsync();
                Debug.WriteLine("CreateAccountForAllEmployee thành công: " + accounts.Count() + " accounts");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi CreateAccountForAllEmployee: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
                throw;
            }
        }

        public async Task<Taikhoan> GetByUsernameAsync(string tentknv)
        {
            return await _dbContext.Taikhoans.FirstOrDefaultAsync(t => t.Tentknv == tentknv);
        }

        // Thêm method lấy toàn bộ tài khoản
        public async Task<List<Taikhoan>> GetAllAsync()
        {
            return await _dbContext.Taikhoans.ToListAsync();
        }
    }
}