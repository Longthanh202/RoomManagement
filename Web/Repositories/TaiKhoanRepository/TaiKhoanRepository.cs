using System.Data.Entity;
using System.Linq;
using Web.Models;
using Web.Models.ViewModel;

namespace Web.Repositories.TaiKhoanRepository
{
    public class TaiKhoanRepository : ITaiKhoanRepository
    {
        private readonly QuanLyPhongTroEntities _db;

        public TaiKhoanRepository(QuanLyPhongTroEntities db)
        {
            _db = db;
        }

        public bool IsUsernameExists(string username)
        {
            return _db.TaiKhoan.Any(x => x.TenDangNhap == username);
        }

        public bool IsEmailExists(string email)
        {
            return _db.TaiKhoan.Any(x => x.Email == email);
        }

        public string CreateOwner(TaiKhoan taiKhoan, ChuTro chuTro)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                if (_db.TaiKhoan.Any(x =>
                    x.TenDangNhap == taiKhoan.TenDangNhap))
                {
                    return "Tên đăng nhập đã tồn tại";
                }

                var vaiTro = _db.VaiTro.FirstOrDefault(x =>
                    (x.MaVaiTro == "CHU_TRO" || x.MaVaiTro == "CHUTRO") &&
                    x.TrangThai);
                if (vaiTro == null)
                {
                    return "Chưa khởi tạo vai trò CHU_TRO";
                }

                _db.TaiKhoan.Add(taiKhoan);
                _db.SaveChanges();

                _db.TaiKhoanVaiTro.Add(new TaiKhoanVaiTro
                {
                    TaiKhoanId = taiKhoan.Id,
                    VaiTroId = vaiTro.Id
                });
                _db.ChuTro.Add(chuTro);
                _db.SaveChanges();
                transaction.Commit();
            }

            return null;
        }

        public TaiKhoan GetActiveOwnerAccount(string username)
        {
            return _db.TaiKhoan.FirstOrDefault(x =>
                x.TenDangNhap == username &&
                x.TrangThai &&
                x.LoaiTaiKhoan == "ChuTro");
        }

        public TaiKhoan GetActiveAccount(string username)
        {
            return _db.TaiKhoan
                .Include("TaiKhoanVaiTro.VaiTro")
                .FirstOrDefault(x =>
                    x.TenDangNhap == username &&
                    x.TrangThai);
        }

        public PortalDashboardViewModel GetPortal(string username)
        {
            var account = GetActiveAccount(username);
            if (account == null)
            {
                return null;
            }

            var portal = new PortalDashboardViewModel
            {
                TenDangNhap = account.TenDangNhap,
                LoaiTaiKhoan = account.LoaiTaiKhoan,
                YeuCauDoiMatKhau = account.YeuCauDoiMatKhau,
                VaiTro = string.Join(", ", account.TaiKhoanVaiTro
                    .Where(x => x.VaiTro != null && x.VaiTro.TrangThai)
                    .Select(x => x.VaiTro.TenVaiTro))
            };
            var today = System.DateTime.Today;

            if (account.LoaiTaiKhoan == "ChuTro")
            {
                var owner = _db.ChuTro.FirstOrDefault(x =>
                    x.Email == account.Email && x.TrangThai);
                if (owner == null)
                {
                    return null;
                }

                portal.HoTen = owner.HoTen;
            }
            else if (account.LoaiTaiKhoan == "NhanVien")
            {
                var employee = _db.NhanVien.FirstOrDefault(x =>
                    x.TaiKhoanId == account.Id &&
                    x.TrangThai &&
                    x.ChuTro.TrangThai);
                if (employee == null)
                {
                    return null;
                }

                portal.HoTen = employee.HoTen;
                portal.VaiTro = string.IsNullOrWhiteSpace(portal.VaiTro)
                    ? employee.ChucVu
                    : portal.VaiTro;
                portal.KhuTroPhuTrach = string.Join(", ", _db.PhanCongNhanVien
                    .Where(x =>
                        x.NhanVienId == employee.Id &&
                        !x.NgayKetThuc.HasValue &&
                        x.NgayBatDau <= today &&
                        x.KhuTro.TrangThai)
                    .Select(x => x.KhuTro.TenKhuTro)
                    .ToList());
            }
            else if (account.LoaiTaiKhoan == "CuDan")
            {
                var resident = _db.CuDan.FirstOrDefault(x =>
                    x.TaiKhoanId == account.Id && x.TrangThai);
                if (resident == null)
                {
                    return null;
                }

                portal.HoTen = resident.HoTen;
                var occupancy = _db.HopDongThanhVien
                    .Where(x =>
                        x.CuDanId == resident.Id &&
                        x.TrangThai &&
                        x.HopDong.TrangThai == "DangHieuLuc" &&
                        x.HopDong.NgayBatDau <= today &&
                        x.HopDong.NgayKetThuc >= today &&
                        x.NgayBatDau <= today &&
                        (!x.NgayKetThuc.HasValue ||
                         x.NgayKetThuc >= today))
                    .OrderByDescending(x => x.NgayBatDau)
                    .Select(x => new
                    {
                        x.HopDong.Phong.MaPhong,
                        x.HopDong.Phong.Tang.DayTro.TenDayTro,
                        x.HopDong.Phong.Tang.DayTro.KhuTro.TenKhuTro
                    })
                    .FirstOrDefault();
                if (occupancy != null)
                {
                    portal.TenPhong = occupancy.MaPhong;
                    portal.TenDayTro = occupancy.TenDayTro;
                    portal.TenKhuTro = occupancy.TenKhuTro;
                }
            }
            else
            {
                return null;
            }

            return portal;
        }

        public bool HasActiveOwnerProfile(string email)
        {
            return _db.ChuTro.Any(x => x.Email == email && x.TrangThai);
        }

        public bool IsEmailUsedByAnotherAccount(long accountId, string email)
        {
            return _db.TaiKhoan.Any(x => x.Id != accountId && x.Email == email);
        }

        public bool IsOwnerEmailUsed(string email)
        {
            return _db.ChuTro.Any(x => x.Email == email);
        }

        public void CompleteOwnerProfile(TaiKhoan account, ChuTro chuTro)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                account.Email = chuTro.Email;
                account.SoDienThoai = chuTro.SoDienThoai;
                _db.ChuTro.Add(chuTro);
                _db.SaveChanges();
                transaction.Commit();
            }
        }

        public bool MarkNotificationRead(string username, long recipientId)
        {
            var recipient = _db.NguoiNhanThongBao.FirstOrDefault(x =>
                x.Id == recipientId &&
                x.TaiKhoan.TenDangNhap == username &&
                x.TaiKhoan.TrangThai);
            if (recipient == null)
            {
                return false;
            }

            recipient.DaDoc = true;
            recipient.ThoiGianDoc = System.DateTime.Now;
            _db.SaveChanges();
            return true;
        }
        public void Update(TaiKhoan account)
        {
            _db.Entry(account).State = EntityState.Modified;
            _db.SaveChanges();
        }
    }
}
