using Team_Project_4.Models;
using Team_Project_4.InterfacesRepositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace Team_Project_4.Repositories
{
    public class NhanvienRepository : INhanvienRepository
    {
        private readonly HotelDbContext _dbContext;
        private readonly ITaikhoanRepository _tkrepo;

        public NhanvienRepository(HotelDbContext dbContext, ITaikhoanRepository tkrepo)
        {
            _dbContext = dbContext;
            _tkrepo = tkrepo;
        }

        public async Task AddAsync(Nhanvien nhanvien)
        {
            await _dbContext.Nhanviens.AddAsync(nhanvien);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int Id)
        {
            Debug.WriteLine("id nhan vien: " + Id);
            // FIX: Check existing trước delete để tránh exception
            var nhanvien = await _dbContext.Nhanviens.FindAsync(Id);
            if (nhanvien == null)
            {
                Debug.WriteLine("Không tìm thấy nhân viên ID: " + Id);
                return; // Không throw, chỉ log
            }

            await _tkrepo.DeleteByManv(Id); // Delete liên quan Taikhoan trước
            _dbContext.Nhanviens.Remove(nhanvien);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IQueryable<Nhanvien>> GetAllAsync()
        {
            var nhanviens = _dbContext.Nhanviens
                .Select(nhanvien => new Nhanvien
                {
                    Manv = nhanvien.Manv,
                    Hoten = nhanvien.Hoten,
                    Phai = nhanvien.Phai,
                    Ngaysinh = nhanvien.Ngaysinh,
                    Sdt = nhanvien.Sdt,
                    Email = nhanvien.Email,
                    Diachi = nhanvien.Diachi,
                });

            return nhanviens;
        }

        public async Task<Nhanvien> GetByIdAsync(int id)
        {
            var nhanvien = await _dbContext.Nhanviens.FindAsync(id);
            return nhanvien;
        }

        // FIX: UpdateAsync - Fetch existing, update properties an toàn, sau đó SaveChanges
        public async Task UpdateAsync(Nhanvien nhanvienUpdate, int nhanvienid)
        {
            // FIX: Fetch existing entity để update (tránh insert new)
            var existingNhanvien = await _dbContext.Nhanviens.FindAsync(nhanvienid);
            if (existingNhanvien == null)
            {
                Debug.WriteLine("Không tìm thấy nhân viên ID: " + nhanvienid);
                return; // Không throw, chỉ log
            }

            // Update từng property từ nhanvienUpdate (an toàn, không overwrite ID)
            existingNhanvien.Hoten = nhanvienUpdate.Hoten;
            existingNhanvien.Phai = nhanvienUpdate.Phai;
            existingNhanvien.Ngaysinh = nhanvienUpdate.Ngaysinh;
            existingNhanvien.Sdt = nhanvienUpdate.Sdt;
            existingNhanvien.Email = nhanvienUpdate.Email;
            existingNhanvien.Diachi = nhanvienUpdate.Diachi;

            // THÊM: Update Taikhoan liên quan nếu có (đồng bộ Email → Tentknv)
            await _tkrepo.UpdateByNv(nhanvienid, nhanvienUpdate.Email);

            // Mark as modified và Save
            _dbContext.Entry(existingNhanvien).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Nhanvien> GetByEmailAsync(string email)
        {
            return await _dbContext.Nhanviens.FirstOrDefaultAsync(n => n.Email == email);
        }

        public async Task<Nhanvien> CheckEmailExist(string email, int nhanvienid)
        {
            // THÊM: Null check nếu email null
            if (string.IsNullOrEmpty(email)) return null;
            return await _dbContext.Nhanviens.FirstOrDefaultAsync(x => x.Email == email && x.Manv != nhanvienid);
        }

        public async Task<Nhanvien> GetEmployeeByIdAsync(int id)
        {
            return await _dbContext.Nhanviens
                .FirstOrDefaultAsync(nhanvien => nhanvien.Manv == id);
        }

        public async Task<IQueryable<Nhanvien>> GetAllEmployeesAsync()
        {
            return _dbContext.Nhanviens.AsQueryable();
        }

        public async Task<IQueryable<Nhanvien>> GetEmployeeNoAccount()
        {
            var employeesWithAccounts = await _dbContext.Taikhoans.Select(t => t.Manv).ToListAsync();

            var employeesWithoutAccounts = _dbContext.Nhanviens
                .Where(nv => !employeesWithAccounts.Contains(nv.Manv)) // Filter employees without accounts
                .Select(nhanvien => new Nhanvien
                {
                    Manv = nhanvien.Manv,
                    Hoten = nhanvien.Hoten,
                    Phai = nhanvien.Phai,
                    Ngaysinh = nhanvien.Ngaysinh,
                    Sdt = nhanvien.Sdt,
                    Email = nhanvien.Email,
                    Diachi = nhanvien.Diachi,
                });

            return employeesWithoutAccounts;
        }

        public async Task<IQueryable<Nhanvien>> GetAllEmAsync()
        {
            var nhanviens = _dbContext.Nhanviens
                .Select(nhanvien => new Nhanvien
                {
                    Manv = nhanvien.Manv,
                    Hoten = nhanvien.Hoten,
                    Phai = nhanvien.Phai,
                    Ngaysinh = nhanvien.Ngaysinh,
                    Sdt = nhanvien.Sdt,
                    Email = nhanvien.Email,
                    Diachi = nhanvien.Diachi,
                });

            return nhanviens;
        }
    }
}