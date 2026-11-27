using System;
using System.Data.Entity;
using System.Linq;
using Web.Models;
using Web.Models.ViewModel;

namespace Web.Repositories.ResidentRepository
{
    public class ResidentRepository : IResidentRepository
    {
        private readonly QuanLyPhongTroEntities _db;

        public ResidentRepository(QuanLyPhongTroEntities db)
        {
            _db = db;
        }

        public ResidentDashboardViewModel GetDashboard(string username)
        {
            var account = _db.TaiKhoan.FirstOrDefault(x =>
                x.TenDangNhap == username &&
                x.TrangThai &&
                x.LoaiTaiKhoan == "CuDan");
            if (account == null)
            {
                return null;
            }

            var resident = _db.CuDan.FirstOrDefault(x =>
                x.TaiKhoanId == account.Id && x.TrangThai);
            if (resident == null)
            {
                return null;
            }

            var today = DateTime.Today;
            var currentContracts = _db.HopDong
                .Include("Phong.Tang.DayTro.KhuTro")
                .Where(x =>
                    x.TrangThai == "DangHieuLuc" &&
                    x.NgayBatDau <= today &&
                    x.NgayKetThuc >= today &&
                    (x.CuDanDaiDienId == resident.Id ||
                     x.HopDongThanhVien.Any(m =>
                         m.CuDanId == resident.Id &&
                         m.TrangThai &&
                         m.NgayBatDau <= today &&
                         (!m.NgayKetThuc.HasValue || m.NgayKetThuc >= today))))
                .OrderByDescending(x => x.NgayBatDau)
                .ToList();
            var contractIds = _db.HopDong
                .Where(x =>
                    x.CuDanDaiDienId == resident.Id ||
                    x.HopDongThanhVien.Any(m => m.CuDanId == resident.Id))
                .Select(x => x.Id)
                .ToList();
            var residentContracts = _db.HopDong
                .Include("Phong.Tang.DayTro.KhuTro")
                .Include("CuDan")
                .Include("HopDongThanhVien.CuDan")
                .Where(x =>
                    x.CuDanDaiDienId == resident.Id ||
                    x.HopDongThanhVien.Any(m => m.CuDanId == resident.Id))
                .OrderByDescending(x => x.NgayBatDau)
                .Take(50)
                .ToList();
            var currentRoomContracts = currentContracts
                .GroupBy(x => x.PhongId)
                .Select(x => x.First())
                .ToList();
            var roomIds = currentRoomContracts
                .Select(x => x.PhongId)
                .ToList();
            var propertyIds = currentRoomContracts
                .Select(x => x.Phong.Tang.DayTro.KhuTroId)
                .Distinct()
                .ToList();
            var roomPrices = _db.DichVuPhong
                .Include("DichVu")
                .Where(x =>
                    roomIds.Contains(x.PhongId) &&
                    x.TrangThai &&
                    x.NgayBatDau <= today &&
                    (!x.NgayKetThuc.HasValue || x.NgayKetThuc >= today) &&
                    x.DichVu.TrangThai)
                .ToList();
            var propertyPrices = _db.BangGiaDichVu
                .Include("DichVu")
                .Where(x =>
                    propertyIds.Contains(x.KhuTroId) &&
                    x.TrangThai &&
                    x.TuNgay <= today &&
                    (!x.DenNgay.HasValue || x.DenNgay >= today) &&
                    x.DichVu.TrangThai)
                .ToList();
            var serviceRates = new System.Collections.Generic.List<ResidentServicePriceViewModel>();
            foreach (var contract in currentRoomContracts)
            {
                var roomOverrides = roomPrices
                    .Where(x => x.PhongId == contract.PhongId)
                    .GroupBy(x => x.DichVuId)
                    .Select(x => x.OrderByDescending(price => price.NgayBatDau).First())
                    .ToList();
                foreach (var price in roomOverrides)
                {
                    serviceRates.Add(new ResidentServicePriceViewModel
                    {
                        TenDichVu = price.DichVu.TenDichVu,
                        LoaiTinhPhi = price.DichVu.LoaiTinhPhi,
                        DonViTinh = price.DichVu.DonViTinh,
                        TenKhuTro = contract.Phong.Tang.DayTro.KhuTro.TenKhuTro,
                        MaPhong = contract.Phong.MaPhong,
                        DonGia = price.DonGia,
                        LaGiaRiengPhong = true
                    });
                }

                var overriddenServiceIds = roomOverrides
                    .Select(x => x.DichVuId)
                    .ToList();
                foreach (var price in propertyPrices
                    .Where(x =>
                        x.KhuTroId == contract.Phong.Tang.DayTro.KhuTroId &&
                        !overriddenServiceIds.Contains(x.DichVuId))
                    .GroupBy(x => x.DichVuId)
                    .Select(x => x.OrderByDescending(rate => rate.TuNgay).First()))
                {
                    serviceRates.Add(new ResidentServicePriceViewModel
                    {
                        TenDichVu = price.DichVu.TenDichVu,
                        LoaiTinhPhi = price.DichVu.LoaiTinhPhi,
                        DonViTinh = price.DichVu.DonViTinh,
                        TenKhuTro = contract.Phong.Tang.DayTro.KhuTro.TenKhuTro,
                        MaPhong = contract.Phong.MaPhong,
                        DonGia = price.DonGia,
                        LaGiaRiengPhong = false
                    });
                }
            }
            var currentRoom = currentContracts.FirstOrDefault();

            var dashboard = new ResidentDashboardViewModel
            {
                HoTen = resident.HoTen,
                TenDangNhap = account.TenDangNhap,
                YeuCauDoiMatKhau = account.YeuCauDoiMatKhau,
                CoHopDongHienTai = currentContracts.Count > 0,
                HopDong = residentContracts,
                LichSuCuTru = _db.LichSuCuTru
                    .Include("Phong.Tang.DayTro.KhuTro")
                    .Where(x => x.CuDanId == resident.Id)
                    .OrderByDescending(x => x.NgayVao)
                    .Take(50)
                    .ToList(),
                HoaDon = _db.HoaDon
                    .Include("ChiTietHoaDon.DichVu")
                    .Include("Phong")
                    .Where(x => contractIds.Contains(x.HopDongId))
                    .OrderByDescending(x => x.NgayLap)
                    .Take(50)
                    .ToList(),
                DichVuGia = serviceRates
                    .OrderBy(x => x.TenKhuTro)
                    .ThenBy(x => x.MaPhong)
                    .ThenBy(x => x.TenDichVu)
                    .ToList(),
                YeuCauSuaChua = _db.YeuCauSuaChua
                    .Include("Phong")
                    .Where(x => x.CuDanId == resident.Id)
                    .OrderByDescending(x => x.NgayTao)
                    .Take(30)
                    .ToList(),
                DangKyKhach = _db.DangKyKhach
                    .Include("Phong")
                    .Where(x => x.CuDanId == resident.Id)
                    .OrderByDescending(x => x.ThoiGianDuKienDen)
                    .Take(30)
                    .ToList(),
                ThongBao = GetNotifications(account.Id),
                SoThongBaoChuaDoc = _db.NguoiNhanThongBao.Count(x =>
                    x.TaiKhoanId == account.Id && !x.DaDoc)
            };

            if (currentRoom != null)
            {
                dashboard.TenPhong = currentRoom.Phong.MaPhong;
                dashboard.TenDayTro = currentRoom.Phong.Tang.DayTro.TenDayTro;
                dashboard.TenKhuTro = currentRoom.Phong.Tang.DayTro.KhuTro.TenKhuTro;
            }

            return dashboard;
        }

        public bool CreateRepairRequest(string username, CreateRepairRequestViewModel model)
        {
            var resident = GetResidentWithActiveRoom(username);
            if (resident == null)
            {
                return false;
            }

            var contract = GetCurrentContract(resident.Id);
            if (contract == null)
            {
                return false;
            }

            var request = new YeuCauSuaChua
            {
                MaYeuCau = "RC-" + Guid.NewGuid().ToString("N"),
                PhongId = contract.PhongId,
                CuDanId = resident.Id,
                TieuDe = model.TieuDe.Trim(),
                NoiDung = model.NoiDung.Trim(),
                MucDoUuTien = model.MucDoUuTien,
                TrangThai = "ChoTiepNhan",
                NgayTao = DateTime.Now
            };
            using (var transaction = _db.Database.BeginTransaction())
            {
                _db.YeuCauSuaChua.Add(request);
                _db.SaveChanges();
                _db.LichSuSuaChua.Add(new LichSuSuaChua
                {
                    YeuCauSuaChuaId = request.Id,
                    TrangThaiMoi = request.TrangThai,
                    NoiDung = "Cư dân gửi yêu cầu sửa chữa.",
                    NgayCapNhat = DateTime.Now
                });
                _db.SaveChanges();
                transaction.Commit();
            }
            return true;
        }

        public bool RegisterGuest(string username, RegisterGuestViewModel model)
        {
            var resident = GetResidentWithActiveRoom(username);
            if (resident == null)
            {
                return false;
            }

            var contract = GetCurrentContract(resident.Id);
            if (contract == null)
            {
                return false;
            }

            _db.DangKyKhach.Add(new DangKyKhach
            {
                PhongId = contract.PhongId,
                CuDanId = resident.Id,
                TenKhach = model.TenKhach.Trim(),
                SoDienThoai = Clean(model.SoDienThoai),
                SoCCCD = Clean(model.SoCCCD),
                SoLuongKhach = model.SoLuongKhach,
                ThoiGianDuKienDen = model.ThoiGianDuKienDen,
                ThoiGianDuKienVe = model.ThoiGianDuKienVe,
                GhiChu = Clean(model.GhiChu),
                TrangThai = "ChoDuyet"
            });
            _db.SaveChanges();
            return true;
        }

        private CuDan GetResidentWithActiveRoom(string username)
        {
            var account = _db.TaiKhoan.FirstOrDefault(x =>
                x.TenDangNhap == username &&
                x.TrangThai &&
                x.LoaiTaiKhoan == "CuDan");
            return account == null
                ? null
                : _db.CuDan.FirstOrDefault(x =>
                    x.TaiKhoanId == account.Id && x.TrangThai);
        }

        private HopDong GetCurrentContract(long residentId)
        {
            var today = DateTime.Today;
            return _db.HopDong
                .Where(x =>
                    x.TrangThai == "DangHieuLuc" &&
                    x.NgayBatDau <= today &&
                    x.NgayKetThuc >= today &&
                    (x.CuDanDaiDienId == residentId ||
                     x.HopDongThanhVien.Any(m =>
                         m.CuDanId == residentId &&
                         m.TrangThai &&
                         m.NgayBatDau <= today &&
                         (!m.NgayKetThuc.HasValue || m.NgayKetThuc >= today))))
                .OrderByDescending(x => x.NgayBatDau)
                .FirstOrDefault();
        }

        private System.Collections.Generic.IList<NguoiNhanThongBao> GetNotifications(long accountId)
        {
            return _db.NguoiNhanThongBao
                .Include("ThongBao")
                .Where(x => x.TaiKhoanId == accountId)
                .OrderByDescending(x => x.ThongBao.NgayTao)
                .Take(20)
                .ToList();
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
