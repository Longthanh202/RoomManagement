using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Web.Models;
using Web.Models.ViewModel;

namespace Web.Repositories.EmployeeRepository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly QuanLyPhongTroEntities _db;

        public EmployeeRepository(QuanLyPhongTroEntities db)
        {
            _db = db;
        }

        public EmployeeDashboardViewModel GetDashboard(string username)
        {
            var employee = GetEmployee(username);
            if (employee == null)
            {
                return null;
            }

            var permissions = GetPermissions(employee);
            var today = DateTime.Today;
            var areaIds = GetAssignedAreaIds(employee.Id, today);
            var dashboard = new EmployeeDashboardViewModel
            {
                HoTen = employee.HoTen,
                TenDangNhap = employee.TaiKhoan.TenDangNhap,
                ChucVu = string.IsNullOrWhiteSpace(employee.ChucVu)
                    ? string.Join(", ", permissions.RoleNames)
                    : employee.ChucVu,
                YeuCauDoiMatKhau = employee.TaiKhoan.YeuCauDoiMatKhau,
                CoQuyenXuLySuaChua = permissions.CanHandleRepairs,
                CoQuyenXuLyKhach = permissions.CanHandleGuests,
                CoQuyenXemCuDan = permissions.CanViewResidents,
                CoQuyenXemHoaDon = permissions.CanViewInvoices,
                KhuTro = _db.KhuTro
                    .Where(x => areaIds.Contains(x.Id) && x.TrangThai)
                    .OrderBy(x => x.TenKhuTro)
                    .ToList(),
                ThongBao = _db.NguoiNhanThongBao
                    .Include("ThongBao")
                    .Where(x => x.TaiKhoanId == employee.TaiKhoanId)
                    .OrderByDescending(x => x.ThongBao.NgayTao)
                    .Take(20)
                    .ToList(),
                SoThongBaoChuaDoc = _db.NguoiNhanThongBao.Count(x =>
                    x.TaiKhoanId == employee.TaiKhoanId && !x.DaDoc)
            };

            if (permissions.CanViewResidents)
            {
                dashboard.HopDong = _db.HopDong
                    .Include("Phong.Tang.DayTro.KhuTro")
                    .Include("CuDan")
                    .Include("HopDongThanhVien.CuDan")
                    .Where(x =>
                        areaIds.Contains(x.Phong.Tang.DayTro.KhuTroId) &&
                        x.TrangThai == "DangHieuLuc" &&
                        x.NgayBatDau <= today &&
                        x.NgayKetThuc >= today)
                    .OrderBy(x => x.Phong.Tang.DayTro.KhuTro.TenKhuTro)
                    .ThenBy(x => x.Phong.MaPhong)
                    .Take(100)
                    .ToList();
            }

            if (permissions.CanViewInvoices)
            {
                dashboard.HoaDon = _db.HoaDon
                    .Include("Phong")
                    .Include("ChiTietHoaDon.DichVu")
                    .Where(x => areaIds.Contains(x.Phong.Tang.DayTro.KhuTroId))
                    .OrderByDescending(x => x.NgayLap)
                    .Take(100)
                    .ToList();
            }

            if (permissions.CanHandleRepairs)
            {
                dashboard.YeuCauSuaChua = _db.YeuCauSuaChua
                    .Include("Phong.Tang.DayTro.KhuTro")
                    .Include("CuDan")
                    .Where(x =>
                        areaIds.Contains(x.Phong.Tang.DayTro.KhuTroId) &&
                        !x.PhanCongSuaChua.Any(assignment =>
                            assignment.NhanVienId != employee.Id) &&
                        x.TrangThai != "HoanThanh" &&
                        x.TrangThai != "DaHuy")
                    .OrderByDescending(x => x.NgayTao)
                    .Take(100)
                    .ToList();
            }

            if (permissions.CanHandleGuests)
            {
                dashboard.DangKyKhach = _db.DangKyKhach
                    .Include("Phong.Tang.DayTro.KhuTro")
                    .Include("CuDan")
                    .Where(x => areaIds.Contains(x.Phong.Tang.DayTro.KhuTroId))
                    .OrderByDescending(x => x.ThoiGianDuKienDen)
                    .Take(100)
                    .ToList();
            }

            return dashboard;
        }

        public bool UpdateRepairStatus(string username, UpdateRepairStatusViewModel model)
        {
            var employee = GetEmployee(username);
            if (employee == null || !GetPermissions(employee).CanHandleRepairs)
            {
                return false;
            }

            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var request = _db.YeuCauSuaChua
                    .Include("Phong.Tang.DayTro")
                    .FirstOrDefault(x => x.Id == model.YeuCauSuaChuaId);
                var today = DateTime.Today;
                var assignedAreas = GetAssignedAreaIds(employee.Id, today);
                if (request == null ||
                    !assignedAreas.Contains(request.Phong.Tang.DayTro.KhuTroId) ||
                    !IsValidRepairTransition(request.TrangThai, model.TrangThai))
                {
                    return false;
                }

                var otherAssignmentExists = _db.PhanCongSuaChua.Any(x =>
                    x.YeuCauSuaChuaId == request.Id &&
                    x.NhanVienId != employee.Id);
                if (otherAssignmentExists)
                {
                    return false;
                }

                if (request.TrangThai == "ChoTiepNhan" &&
                    model.TrangThai == "DangXuLy" &&
                    !_db.PhanCongSuaChua.Any(x =>
                        x.YeuCauSuaChuaId == request.Id &&
                        x.NhanVienId == employee.Id))
                {
                    _db.PhanCongSuaChua.Add(new PhanCongSuaChua
                    {
                        YeuCauSuaChuaId = request.Id,
                        NhanVienId = employee.Id,
                        NgayPhanCong = DateTime.Now,
                        GhiChu = "Nhận xử lý yêu cầu."
                    });
                }

                var previousStatus = request.TrangThai;
                request.TrangThai = model.TrangThai;
                request.NgayHoanThanh = model.TrangThai == "HoanThanh"
                    ? DateTime.Now
                    : (DateTime?)null;
                _db.LichSuSuaChua.Add(new LichSuSuaChua
                {
                    YeuCauSuaChuaId = request.Id,
                    NhanVienId = employee.Id,
                    TrangThaiCu = previousStatus,
                    TrangThaiMoi = model.TrangThai,
                    NoiDung = Clean(model.NoiDung) ?? "Cập nhật tiến độ sửa chữa.",
                    NgayCapNhat = DateTime.Now
                });
                _db.SaveChanges();
                transaction.Commit();
            }

            return true;
        }

        public bool UpdateGuestStatus(string username, UpdateGuestStatusViewModel model)
        {
            var employee = GetEmployee(username);
            if (employee == null || !GetPermissions(employee).CanHandleGuests)
            {
                return false;
            }

            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var guest = _db.DangKyKhach
                    .Include("Phong.Tang.DayTro")
                    .FirstOrDefault(x => x.Id == model.DangKyKhachId);
                var today = DateTime.Today;
                var assignedAreas = GetAssignedAreaIds(employee.Id, today);
                if (guest == null ||
                    !assignedAreas.Contains(guest.Phong.Tang.DayTro.KhuTroId) ||
                    !IsValidGuestTransition(guest.TrangThai, model.TrangThai))
                {
                    return false;
                }

                guest.TrangThai = model.TrangThai;
                string eventType = GetGuestEventType(model.TrangThai);
                if (eventType != null)
                {
                    _db.LichSuKhach.Add(new LichSuKhach
                    {
                        DangKyKhachId = guest.Id,
                        LoaiSuKien = eventType,
                        ThoiGian = DateTime.Now,
                        NhanVienId = employee.Id,
                        GhiChu = Clean(model.GhiChu)
                    });
                }

                _db.SaveChanges();
                transaction.Commit();
            }
            return true;
        }

        private NhanVien GetEmployee(string username)
        {
            return _db.NhanVien
                .Include("TaiKhoan.TaiKhoanVaiTro.VaiTro")
                .FirstOrDefault(x =>
                    x.TaiKhoan.TenDangNhap == username &&
                    x.TaiKhoan.TrangThai &&
                    x.TaiKhoan.LoaiTaiKhoan == "NhanVien" &&
                    x.TrangThai &&
                    !x.NgayNghiViec.HasValue &&
                    x.ChuTro.TrangThai);
        }

        private List<long> GetAssignedAreaIds(long employeeId, DateTime today)
        {
            return _db.PhanCongNhanVien
                .Where(x =>
                    x.NhanVienId == employeeId &&
                    x.NgayBatDau <= today &&
                    !x.NgayKetThuc.HasValue &&
                    x.KhuTro.TrangThai &&
                    x.KhuTro.ChuTro.TrangThai)
                .Select(x => x.KhuTroId)
                .Distinct()
                .ToList();
        }

        private static EmployeePermissions GetPermissions(NhanVien employee)
        {
            var roleCodes = employee.TaiKhoan.TaiKhoanVaiTro
                .Where(x => x.VaiTro != null && x.VaiTro.TrangThai)
                .Select(x => x.VaiTro.MaVaiTro.ToUpperInvariant())
                .ToList();
            var roleNames = employee.TaiKhoan.TaiKhoanVaiTro
                .Where(x => x.VaiTro != null && x.VaiTro.TrangThai)
                .Select(x => x.VaiTro.TenVaiTro)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
            bool manager = roleCodes.Contains("QUANLY") || roleCodes.Contains("QUAN_LY");
            bool technician = roleCodes.Contains("KYTHUAT") || roleCodes.Contains("KY_THUAT");
            bool receptionist = roleCodes.Contains("LETAN") || roleCodes.Contains("LE_TAN");
            bool accountant = roleCodes.Contains("KETOAN") || roleCodes.Contains("KE_TOAN");
            return new EmployeePermissions
            {
                RoleNames = roleNames,
                CanHandleRepairs = manager || technician,
                CanHandleGuests = manager || receptionist,
                CanViewResidents = manager || receptionist,
                CanViewInvoices = manager || accountant
            };
        }

        private static bool IsValidRepairTransition(string current, string next)
        {
            return (current == "ChoTiepNhan" &&
                    (next == "DangXuLy" || next == "DaHuy")) ||
                   (current == "DangXuLy" &&
                    (next == "ChoVatTu" || next == "HoanThanh")) ||
                   (current == "ChoVatTu" &&
                    (next == "DangXuLy" || next == "HoanThanh"));
        }

        private static bool IsValidGuestTransition(string current, string next)
        {
            return (current == "ChoDuyet" &&
                    (next == "DaDuyet" || next == "TuChoi")) ||
                   (current == "DaDuyet" &&
                    (next == "DaDen" || next == "DaHuy")) ||
                   (current == "DaDen" && next == "DaVe");
        }

        private static string GetGuestEventType(string status)
        {
            if (status == "DaDen")
            {
                return "CheckIn";
            }

            if (status == "DaVe")
            {
                return "CheckOut";
            }

            if (status == "DaHuy")
            {
                return "Huy";
            }

            return null;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private class EmployeePermissions
        {
            public IList<string> RoleNames { get; set; }
            public bool CanHandleRepairs { get; set; }
            public bool CanHandleGuests { get; set; }
            public bool CanViewResidents { get; set; }
            public bool CanViewInvoices { get; set; }
        }
    }
}
