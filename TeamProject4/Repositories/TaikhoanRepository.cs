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
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(Taikhoan taikhoan)
        {
            try
            {
                if (taikhoan == null)
                {
                    Debug.WriteLine("[ERROR] Taikhoan null, không add");
                    return;
                }

                _dbContext.Taikhoans.Add(taikhoan);
                await _dbContext.SaveChangesAsync();
                Debug.WriteLine($"[DEBUG] Add Taikhoan thành công: {taikhoan.Tentknv}, Matknv: {taikhoan.Matknv}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] AddAsync Taikhoan: {ex.Message} | Inner: {ex.InnerException?.Message} | StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task UpdateByNv(int manv, string newEmail)
        {
            var tk = await _dbContext.Taikhoans.FirstOrDefaultAsync(tk => tk.Manv == manv);
            if (tk != null)
            {
                tk.Tentknv = newEmail;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> ToggleAccountStatus(int manv, bool activate)
        {
            try
            {
                Debug.WriteLine($"[DEBUG] Bắt đầu {(activate ? "kích hoạt" : "vô hiệu hóa")} Taikhoan với Manv = {manv}");
                var taikhoans = await _dbContext.Taikhoans
                    .Where(tk => tk.Manv == manv)
                    .ToListAsync();
                if (taikhoans == null || !taikhoans.Any())
                {
                    Debug.WriteLine($"[DEBUG] Không tìm thấy Taikhoan với Manv = {manv}");
                    return false;
                }

                Debug.WriteLine($"[DEBUG] Tìm thấy {taikhoans.Count} bản ghi: {string.Join(", ", taikhoans.Select(tk => tk.Matknv))}");
                foreach (var tk in taikhoans)
                {
                    tk.IsActive = activate; // Kích hoạt hoặc vô hiệu hóa
                }
                int rowsAffected = await _dbContext.SaveChangesAsync();
                Debug.WriteLine($"[DEBUG] {(activate ? "Kích hoạt" : "Vô hiệu hóa")} thành công, ảnh hưởng {rowsAffected} hàng với Manv = {manv}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"[ERROR] DbUpdateException khi {(activate ? "kích hoạt" : "vô hiệu hóa")}: {ex.Message} | Inner: {ex.InnerException?.Message} | StackTrace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Lỗi {(activate ? "kích hoạt" : "vô hiệu hóa")}: {ex.Message} | Inner: {ex.InnerException?.Message} | StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Taikhoan> GetByUsernameAndPasswordAsync(string tentknv, string mktk)
        {
            return await _dbContext.Taikhoans
                .FirstOrDefaultAsync(t => t.Tentknv == tentknv && t.Mktk == mktk && t.IsActive);
        }

        public async Task CreateAccountForAllEmployee(IEnumerable<Taikhoan> accounts)
        {
            try
            {
                if (accounts == null || !accounts.Any())
                {
                    Debug.WriteLine("[ERROR] Danh sách accounts rỗng, không create");
                    return;
                }

                _dbContext.Taikhoans.AddRange(accounts);
                await _dbContext.SaveChangesAsync();
                Debug.WriteLine($"[DEBUG] CreateAccountForAllEmployee thành công: {accounts.Count()} accounts");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] CreateAccountForAllEmployee: {ex.Message} | Inner: {ex.InnerException?.Message} | StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Taikhoan> GetByUsernameAsync(string tentknv)
        {
            return await _dbContext.Taikhoans.FirstOrDefaultAsync(t => t.Tentknv == tentknv);
        }

        public async Task<List<Taikhoan>> GetAllAsync()
        {
            return await _dbContext.Taikhoans.ToListAsync();
        }

        public async Task<Taikhoan> GetByManvAsync(int manv)
        {
            return await _dbContext.Taikhoans
                .Where(tk => tk.Manv == manv)
                .FirstOrDefaultAsync();
        }
    }
}