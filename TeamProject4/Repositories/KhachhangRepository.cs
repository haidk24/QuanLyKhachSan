using Microsoft.EntityFrameworkCore;
using System;
using Team_Project_4.InterfacesRepositories;
using Team_Project_4.Models;

namespace Team_Project_4.Repositories
{
    public class KhachhangRepository : IKhachhangRepository
    {
        private readonly HotelDbContext _dbContext;
        private readonly ILoaiKhachRepository _loaikhachRepo;
        public KhachhangRepository(HotelDbContext dbContext, ILoaiKhachRepository lkrepo)
        {
            _dbContext = dbContext;
            _loaikhachRepo = lkrepo;
        }
        public async Task<int> CountClientsByRoomId(int map)
        {
            return await _dbContext.Khachhangs.CountAsync(k => k.Map == map);
        }
        public async Task AddAsync(Khachhang khach)
        {
            khach.MaloaikhachNavigation = await _loaikhachRepo.GetByIdAsync(khach.Maloaikhach);
            await _dbContext.Khachhangs.AddAsync(khach);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int Id)
        {
            Khachhang khach = await _dbContext.Khachhangs.FindAsync(Id);
            _dbContext.Khachhangs.Remove(khach);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<Khachhang> GetAllAsync()
        {
            var khachs = _dbContext.Khachhangs
            .Select(khach => new Khachhang
            {
                Makh = khach.Makh,
                Tenkh = khach.Tenkh,
                MaloaikhachNavigation = khach.MaloaikhachNavigation,
                Tel = khach.Tel,
                Tuoi = khach.Tuoi,
                Diachikh = khach.Diachikh,
                Cmndkh = khach.Cmndkh,
                AnhCccdMatTruoc = khach.AnhCccdMatTruoc,
                AnhCccdMatSau = khach.AnhCccdMatSau,
                Phieuthues = khach.Phieuthues,
            });

            return khachs;
        }

        public async Task<Khachhang> GetByIdAsync(int id)
        {
            return await _dbContext.Khachhangs
                .Include(k => k.MaloaikhachNavigation)
                .Include(k => k.MapNavigation)
                .FirstOrDefaultAsync(k => k.Makh == id);
        }

        public async Task UpdateAsync(Khachhang khachUpdate, int id)
        {
            // Fetch the existing Khachhang entity by id
            Khachhang existingKhach = await _dbContext.Khachhangs.FindAsync(id);

            if (existingKhach != null)
            {
                // Cập nhật từng property một cách rõ ràng, tránh navigation properties
                existingKhach.Tenkh = khachUpdate.Tenkh;
                existingKhach.Tuoi = khachUpdate.Tuoi;
                existingKhach.Tel = khachUpdate.Tel;
                existingKhach.Diachikh = khachUpdate.Diachikh;
                existingKhach.Cmndkh = khachUpdate.Cmndkh;
                existingKhach.Maloaikhach = khachUpdate.Maloaikhach;
                existingKhach.Map = khachUpdate.Map;
                existingKhach.AnhCccdMatTruoc = khachUpdate.AnhCccdMatTruoc;
                existingKhach.AnhCccdMatSau = khachUpdate.AnhCccdMatSau;

                // Không cần update navigation properties, chỉ cần foreign key
                // EF sẽ tự động load navigation khi cần

                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task<List<Loaikhach>> GetAllLoaikhach()
        {
            return await _dbContext.Loaikhaches.ToListAsync();
        }
        public async Task<Loaikhach> GetClientTypeById(int id)
        {
            return await _dbContext.Loaikhaches.FindAsync(id); // Assuming _context is your DbContext
        }
        public async Task<List<string>> GetDistinctClientTypeAsync()
        {
            return await _loaikhachRepo.GetDistinctClientTypeAsync();
        }
        // Trong CustomerRepository.cs hoặc một lớp tương tự

        public async Task<List<Khachhang>> GetCustomersByRoomIdAsync(int roomId)
        {
            // Sử dụng LINQ để truy vấn danh sách khách hàng có mã phòng trùng khớp
            var customers = await _dbContext.Khachhangs
                .Where(c => c.Map == roomId)
                .Include(c => c.MaloaikhachNavigation)
                .ToListAsync();

            return customers;
        }
        public async Task<int> GetClientIdByNameAndId(string tenKhachHang, string cccd)
        {
            // Thực hiện truy vấn trong database để lấy mã khách hàng từ tên và ID khách hàng
            var clientId = await _dbContext.Khachhangs
                .Where(c => c.Tenkh == tenKhachHang && c.Cmndkh == cccd)
                .Select(c => c.Makh)
                .FirstOrDefaultAsync();
            return clientId;
        }
        public async Task<int> GetClientIdByName(string tenKhachHang)
        {
            // Thực hiện truy vấn trong database để lấy mã khách hàng từ tên và ID khách hàng
            var clientId = await _dbContext.Khachhangs
                .Where(c => c.Tenkh == tenKhachHang)
                .Select(c => c.Makh)
                .FirstOrDefaultAsync();
            return clientId;
        }
        public async Task<Khachhang> GetClientByCCCDAsync(string cccd)
        {
            return await _dbContext.Khachhangs.FirstOrDefaultAsync(c => c.Cmndkh == cccd);
        }
        public async Task<Khachhang> GetClientByIDAsync(int id)
        {
            return await _dbContext.Khachhangs.FirstOrDefaultAsync(c => c.Makh == id);
        }
        public async Task<int> GetSoLuongKhachByMapAsync(int map)
        {

            var soLuongKhach = await _dbContext.Khachhangs
                .Where(k => k.Map == map)
                .CountAsync();

            return soLuongKhach;
        }

        public async Task<IEnumerable<Khachhang>> GetCustomersByRoomNameAsync(string tenPhong)
        {
            // Lấy danh sách khách hàng dựa trên tên phòng
            var customers = await _dbContext.Khachhangs
                .Where(c => c.MapNavigation.Tenphong == tenPhong)
                .ToListAsync();

            return customers;
        }
        public async Task DeleteCustomersByRoomAsync(string tenPhong, string CCCD)
        {
            // Lấy danh sách khách hàng theo tên phòng
            var customersToDelete = await GetCustomersByRoomNameAsync(tenPhong);



            // Xóa các khách hàng còn lại
            var otherCustomersToDelete = customersToDelete.Where(c => c.Cmndkh != CCCD);
            foreach (var customer in otherCustomersToDelete)
            {
                await DeleteAsync(customer.Makh);
            }
        }
        }
}
