using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Web.Helpers;
using Web.Models;
using Web.Models.ViewModel;
using Web.Repositories.OwnerRepository;

namespace Web.Services.OwnerService
{
    public class OwnerService : IOwnerService
    {
        private readonly IOwnerRepository _repository;

        public OwnerService(IOwnerRepository repository)
        {
            _repository = repository;
        }

        public OwnerDashboardViewModel GetDashboard(string username)
        {
            var owner = _repository.GetOwnerByUsername(username);
            var account = _repository.GetOwnerAccount(username);
            return owner == null
                ? null
                : _repository.GetDashboard(owner.Id, account.Id, owner.HoTen);
        }

        public bool IsOwnerActive(string username)
        {
            return _repository.GetOwnerByUsername(username) != null;
        }

        public string CreateKhuTro(string username, CreateKhuTroViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            if (owner == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            string code = model.MaKhuTro.Trim();
            if (_repository.IsKhuTroCodeUsed(owner.Id, code))
            {
                return "Mã khu trọ đã được sử dụng.";
            }

            try
            {
                _repository.CreateKhuTro(new KhuTro
                {
                    ChuTroId = owner.Id,
                    MaKhuTro = code,
                    TenKhuTro = model.TenKhuTro.Trim(),
                    DiaChi = Clean(model.DiaChi),
                    MoTa = Clean(model.MoTa),
                    TrangThai = true,
                    NgayTao = DateTime.Now
                });
            }
            catch (DbUpdateException)
            {
                return "Không thể lưu khu trọ. Hãy kiểm tra mã khu trọ và thử lại.";
            }

            return null;
        }

        public string CreateDayTro(string username, CreateDayTroViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            var khuTro = owner == null
                ? null
                : _repository.GetOwnedKhuTro(owner.Id, model.KhuTroId);
            if (khuTro == null)
            {
                return "Vui lòng chọn khu trọ hợp lệ.";
            }

            string code = model.MaDayTro.Trim();
            if (_repository.IsDayTroCodeUsed(khuTro.Id, code))
            {
                return "Mã dãy đã tồn tại trong khu trọ này.";
            }

            try
            {
                _repository.CreateDayTro(new DayTro
                {
                    KhuTroId = khuTro.Id,
                    MaDayTro = code,
                    TenDayTro = model.TenDayTro.Trim(),
                    MoTa = Clean(model.MoTa),
                    TrangThai = true
                });
            }
            catch (DbUpdateException)
            {
                return "Không thể lưu dãy trọ. Hãy kiểm tra mã dãy và thử lại.";
            }

            return null;
        }

        public string CreateFloor(string username, CreateFloorViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            var dayTro = owner == null
                ? null
                : _repository.GetOwnedDayTro(owner.Id, model.DayTroId);
            if (dayTro == null)
            {
                return "Vui lòng chọn dãy trọ hợp lệ.";
            }

            if (_repository.FloorExists(dayTro.Id, model.SoTang))
            {
                return "Dãy trọ này đã có tầng với số thứ tự đó.";
            }

            try
            {
                _repository.CreateFloor(new Tang
                {
                    DayTroId = dayTro.Id,
                    SoTang = model.SoTang,
                    TenTang = Clean(model.TenTang)
                });
            }
            catch (DbUpdateException)
            {
                return "Không thể lưu tầng. Hãy kiểm tra số tầng và thử lại.";
            }

            return null;
        }

        public string CreateRoom(string username, CreateRoomViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            var floor = owner == null
                ? null
                : _repository.GetOwnedFloor(owner.Id, model.TangId);
            if (floor == null)
            {
                return "Vui lòng chọn tầng hợp lệ.";
            }

            string[] statuses =
            {
                "Trong", "DangThue", "DaDat", "DangSuaChua", "NgungHoatDong"
            };
            if (!statuses.Contains(model.TrangThai))
            {
                return "Tình trạng phòng không hợp lệ.";
            }

            string code = model.MaPhong.Trim();
            if (_repository.IsRoomCodeUsed(floor.Id, code))
            {
                return "Mã phòng đã tồn tại trên tầng này.";
            }

            try
            {
                _repository.CreateRoom(new Phong
                {
                    TangId = floor.Id,
                    MaPhong = code,
                    TenPhong = Clean(model.TenPhong),
                    DienTich = model.DienTich,
                    SoNguoiToiDa = model.SoNguoiToiDa,
                    GiaThue = model.GiaThue,
                    TrangThai = model.TrangThai,
                    MoTa = Clean(model.MoTa),
                    NgayTao = DateTime.Now
                });
            }
            catch (DbUpdateException)
            {
                return "Không thể lưu phòng. Hãy kiểm tra mã phòng và các thông tin hợp lệ.";
            }

            return null;
        }

        public string CreateEmployee(string username, CreateEmployeeViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            if (owner == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            string normalizedUsername = model.TenDangNhap.Trim();
            string email = CleanEmail(model.Email);
            string employeeCode = model.MaNhanVien.Trim();
            if (_repository.IsUsernameUsed(normalizedUsername) ||
                _repository.IsEmailUsed(email))
            {
                return "Tên đăng nhập hoặc email đã được sử dụng.";
            }

            if (_repository.IsEmployeeCodeUsed(owner.Id, employeeCode))
            {
                return "Mã nhân viên đã được sử dụng.";
            }

            if (model.NgaySinh.HasValue &&
                model.NgaySinh.Value.Date > DateTime.Today)
            {
                return "Ngày sinh nhân viên không thể ở tương lai.";
            }

            string[] roleCodes;
            string defaultPosition;
            switch (model.MaVaiTro)
            {
                case "QUANLY":
                    roleCodes = new[] { "QUANLY", "QUAN_LY" };
                    defaultPosition = "Quản lý";
                    break;
                case "KETOAN":
                    roleCodes = new[] { "KETOAN", "KE_TOAN" };
                    defaultPosition = "Kế toán";
                    break;
                case "KYTHUAT":
                    roleCodes = new[] { "KYTHUAT", "KY_THUAT" };
                    defaultPosition = "Kỹ thuật";
                    break;
                case "LETAN":
                    roleCodes = new[] { "LETAN", "LE_TAN" };
                    defaultPosition = "Lễ tân";
                    break;
                default:
                    return "Vai trò nhân viên không hợp lệ.";
            }

            var role = _repository.FindActiveRole(roleCodes);
            var khuTro = model.KhuTroId.HasValue
                ? _repository.GetOwnedKhuTro(owner.Id, model.KhuTroId.Value)
                : null;
            if (role == null)
            {
                return "Chưa khởi tạo vai trò nhân viên đã chọn.";
            }

            if (model.KhuTroId.HasValue && khuTro == null)
            {
                return "Vui lòng chọn khu trọ thuộc quyền quản lý của bạn.";
            }

            var account = new TaiKhoan
            {
                TenDangNhap = normalizedUsername,
                MatKhauHash = PasswordHasher.Hash(model.MatKhau),
                Email = email,
                SoDienThoai = Clean(model.SoDienThoai),
                LoaiTaiKhoan = "NhanVien",
                TrangThai = true,
                YeuCauDoiMatKhau = true,
                NgayTao = DateTime.Now
            };
            var employee = new NhanVien
            {
                ChuTroId = owner.Id,
                MaNhanVien = employeeCode,
                HoTen = model.HoTen.Trim(),
                NgaySinh = model.NgaySinh.HasValue
                    ? model.NgaySinh.Value.Date
                    : (DateTime?)null,
                GioiTinh = Clean(model.GioiTinh),
                SoDienThoai = Clean(model.SoDienThoai),
                Email = email,
                DiaChi = Clean(model.DiaChi),
                ChucVu = string.IsNullOrWhiteSpace(model.ChucVu)
                    ? defaultPosition
                    : model.ChucVu.Trim(),
                NgayVaoLam = DateTime.Today,
                TrangThai = true
            };
            var assignment = khuTro == null
                ? null
                : new PhanCongNhanVien
                {
                    KhuTroId = khuTro.Id,
                    NgayBatDau = DateTime.Today,
                    GhiChu = "Phân công khi tạo tài khoản"
                };

            try
            {
                _repository.CreateEmployee(
                    account,
                    new TaiKhoanVaiTro { VaiTroId = role.Id },
                    employee,
                    assignment);
            }
            catch (DbUpdateException)
            {
                return "Không thể tạo tài khoản nhân viên. Hãy kiểm tra mã nhân viên, tên đăng nhập và email.";
            }

            return null;
        }

        public string CreateResident(string username, CreateResidentViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            if (owner == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            string normalizedUsername = model.TenDangNhap.Trim();
            string email = CleanEmail(model.Email);
            if (_repository.IsUsernameUsed(normalizedUsername) ||
                _repository.IsEmailUsed(email))
            {
                return "Tên đăng nhập hoặc email đã được sử dụng.";
            }

            var room = _repository.GetAvailableRoom(owner.Id, model.PhongId);
            var role = _repository.FindResidentRole();
            if (room == null)
            {
                return "Phòng không còn trống hoặc không thuộc khu trọ của bạn.";
            }

            if (role == null)
            {
                return "Chưa khởi tạo vai trò cư dân.";
            }

            if (!model.NgayBatDau.HasValue ||
                !model.NgayKetThuc.HasValue ||
                model.NgayKetThuc.Value.Date <= model.NgayBatDau.Value.Date ||
                model.NgayThanhToan < 1 ||
                model.NgayThanhToan > 31 ||
                model.GiaThue < 0 ||
                model.TienCoc < 0)
            {
                return "Thông tin hợp đồng chưa hợp lệ.";
            }

            if ((model.NgaySinh.HasValue && model.NgaySinh.Value.Date > DateTime.Today) ||
                (model.NgayCapCCCD.HasValue && model.NgayCapCCCD.Value.Date > DateTime.Today))
            {
                return "Ngày sinh và ngày cấp CCCD không thể ở tương lai.";
            }

            var ownerAccount = _repository.GetOwnerAccount(username);
            if (ownerAccount == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            var account = new TaiKhoan
            {
                TenDangNhap = normalizedUsername,
                MatKhauHash = PasswordHasher.Hash(model.MatKhau),
                Email = email,
                SoDienThoai = Clean(model.SoDienThoai),
                LoaiTaiKhoan = "CuDan",
                TrangThai = true,
                YeuCauDoiMatKhau = true,
                NgayTao = DateTime.Now
            };
            var resident = new CuDan
            {
                HoTen = model.HoTen.Trim(),
                NgaySinh = model.NgaySinh.HasValue
                    ? model.NgaySinh.Value.Date
                    : (DateTime?)null,
                GioiTinh = Clean(model.GioiTinh),
                SoDienThoai = Clean(model.SoDienThoai),
                Email = email,
                SoCCCD = Clean(model.SoCCCD),
                NgayCapCCCD = model.NgayCapCCCD.HasValue
                    ? model.NgayCapCCCD.Value.Date
                    : (DateTime?)null,
                NoiCapCCCD = Clean(model.NoiCapCCCD),
                DiaChiThuongTru = Clean(model.DiaChiThuongTru),
                TrangThai = true,
                NgayTao = DateTime.Now
            };
            var contract = new HopDong
            {
                MaHopDong = "RC-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant(),
                NgayBatDau = model.NgayBatDau.Value.Date,
                NgayKetThuc = model.NgayKetThuc.Value.Date,
                GiaThue = model.GiaThue,
                TienCoc = model.TienCoc,
                NgayThanhToan = model.NgayThanhToan,
                SoThangThanhToan = 1,
                TrangThai = "ChoDuyet",
                NgayTao = DateTime.Now,
                NguoiTao = ownerAccount.Id
            };
            var contractMember = new HopDongThanhVien
            {
                LoaiThanhVien = "NguoiThueChinh",
                LaNguoiDaiDien = true,
                NgayBatDau = model.NgayBatDau.Value.Date,
                TrangThai = true
            };

            try
            {
                _repository.CreateResident(
                    owner.Id,
                    model.PhongId,
                    account,
                    new TaiKhoanVaiTro { VaiTroId = role.Id },
                    resident,
                    contract,
                    contractMember);
            }
            catch (RoomUnavailableException)
            {
                return "Phòng vừa được đặt hoặc không còn trống. Vui lòng tải lại danh sách phòng.";
            }
            catch (DbUpdateException)
            {
                return "Không thể tạo tài khoản cư dân hoặc hợp đồng. Hãy kiểm tra tên đăng nhập, email và phòng.";
            }

            return null;
        }

        public string AssignEmployee(string username, AssignEmployeeViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            if (owner == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            if (model.NgayBatDau.Date < DateTime.Today)
            {
                return "Ngày bắt đầu phân công không thể ở quá khứ.";
            }

            return _repository.AssignEmployee(owner.Id, model)
                ? null
                : "Nhân viên hoặc khu trọ không hợp lệ, hoặc nhân viên đã được phân công tại khu này.";
        }

        public string AddRoomMember(string username, AddRoomMemberViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            if (owner == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            if (model.NgayBatDau.Date < DateTime.Today)
            {
                return "Ngày bắt đầu cư trú không thể ở quá khứ.";
            }

            return _repository.AddRoomMember(owner.Id, model)
                ? null
                : "Không thể thêm thành viên. Hãy kiểm tra hợp đồng, sức chứa phòng và trạng thái cư trú hiện tại.";
        }

        public string EndRoomMember(string username, EndRoomMemberViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            if (owner == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            return _repository.EndRoomMember(owner.Id, model)
                ? null
                : "Không tìm thấy thành viên đang cư trú trong hợp đồng này.";
        }

        public string UpdateContractStatus(string username, UpdateContractStatusViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            if (owner == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            if (model.TrangThai != "DangHieuLuc" &&
                model.TrangThai != "DaKetThuc" &&
                model.TrangThai != "TuChoi")
            {
                return "Trạng thái hợp đồng không hợp lệ.";
            }

            return _repository.UpdateContractStatus(owner.Id, model)
                ? null
                : "Không thể cập nhật hợp đồng. Hợp đồng có thể đã thay đổi trạng thái hoặc phòng không còn khả dụng.";
        }

        public string SetServicePrice(string username, SetServicePriceViewModel model)
        {
            var owner = _repository.GetOwnerByUsername(username);
            if (owner == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            if (model.DonGia < 0 || model.TuNgay.Date < DateTime.Today)
            {
                return "Đơn giá phải không âm và ngày áp dụng không thể ở quá khứ.";
            }

            return _repository.SetServicePrice(owner.Id, model)
                ? null
                : "Không thể áp dụng giá dịch vụ. Hãy kiểm tra khu trọ, dịch vụ và ngày hiệu lực.";
        }

        public string CreateService(string username, CreateServiceViewModel model)
        {
            if (_repository.GetOwnerByUsername(username) == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            string[] billingTypes =
            {
                "TheoChiSo", "CoDinh", "TheoNguoi", "TheoXe", "TheoSoLuong", "TuNhap"
            };
            string code = model.MaDichVu.Trim().ToUpperInvariant();
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                code,
                @"^[A-Z0-9_-]+$"))
            {
                return "Mã dịch vụ chỉ được chứa chữ không dấu, số, dấu gạch ngang và gạch dưới.";
            }

            if (!billingTypes.Contains(model.LoaiTinhPhi))
            {
                return "Phương thức tính phí không hợp lệ.";
            }

            if (_repository.IsServiceCodeUsed(code))
            {
                return "Mã dịch vụ đã tồn tại.";
            }

            try
            {
                _repository.CreateService(new DichVu
                {
                    MaDichVu = code,
                    TenDichVu = model.TenDichVu.Trim(),
                    LoaiTinhPhi = model.LoaiTinhPhi,
                    DonViTinh = Clean(model.DonViTinh),
                    MoTa = Clean(model.MoTa),
                    TrangThai = true
                });
            }
            catch (DbUpdateException)
            {
                return "Không thể lưu dịch vụ. Hãy kiểm tra mã dịch vụ và thử lại.";
            }

            return null;
        }

        public bool MarkNotificationRead(string username, long notificationRecipientId)
        {
            var account = _repository.GetOwnerAccount(username);
            return account != null &&
                _repository.MarkNotificationRead(account.Id, notificationRecipientId);
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string CleanEmail(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
        }
    }
}
