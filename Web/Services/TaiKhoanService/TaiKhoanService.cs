using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Web.Helpers;
using Web.Models;
using Web.Models.ViewModel;
using Web.Repositories.TaiKhoanRepository;

namespace Web.Services.TaiKhoanService
{
    public class TaiKhoanService: ITaiKhoanService
    {
        private readonly ITaiKhoanRepository _repository;
        public TaiKhoanService(ITaiKhoanRepository repository)
        {
            _repository = repository;
        }
        public string RegisterOwner(
            RegisterOwnerViewModel model)
        {
            string username =
                model.TenDangNhap.Trim();

            string email =
                model.Email.Trim().ToLowerInvariant();

            // Kiem tra username
            if (_repository.IsUsernameExists(username))
            {
                return "Tên đăng nhập đã tồn tại";
            }

            // Kiem tra email
            if (_repository.IsEmailExists(email))
            {
                return "Email đã được sử dụng";
            }

            // Hash password
            string passwordHash =
                PasswordHasher.Hash(model.MatKhau);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = username,
                MatKhauHash = passwordHash,
                Email = email,
                SoDienThoai = model.SoDienThoai,
                LoaiTaiKhoan = "ChuTro",
                TrangThai = true,

                // Chu tro tu dang ky, khong can doi mat khau
                YeuCauDoiMatKhau = false,

                NgayTao = DateTime.Now
            };

            var chuTro = new ChuTro
            {
                HoTen = model.HoTen.Trim(),
                Email = email,
                SoDienThoai = model.SoDienThoai == null
                    ? null
                    : model.SoDienThoai.Trim(),
                DiaChi = string.IsNullOrWhiteSpace(model.DiaChi)
                    ? null
                    : model.DiaChi.Trim(),
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            try
            {
                return _repository.CreateOwner(taiKhoan, chuTro);
            }
            catch (DbUpdateException)
            {
                return "Dữ liệu đăng ký bị trùng hoặc không hợp lệ.";
            }
        }

        public TaiKhoan AuthenticateOwner(string username, string password)
        {
            var account = AuthenticateAccount(username, password);
            return account != null && account.LoaiTaiKhoan == "ChuTro"
                ? account
                : null;
        }

        public TaiKhoan AuthenticateAccount(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password))
            {
                return null;
            }

            var account = _repository.GetActiveAccount(username.Trim());
            if (account == null ||
                !PasswordHasher.Verify(password, account.MatKhauHash))
            {
                return null;
            }

            return account.LoaiTaiKhoan == "ChuTro" ||
                _repository.GetPortal(account.TenDangNhap) != null
                    ? account
                    : null;
        }

        public PortalDashboardViewModel GetPortal(string username)
        {
            return string.IsNullOrWhiteSpace(username)
                ? null
                : _repository.GetPortal(username.Trim());
        }

        public TaiKhoan GetActiveOwnerAccount(string username)
        {
            return string.IsNullOrWhiteSpace(username)
                ? null
                : _repository.GetActiveOwnerAccount(username.Trim());
        }

        public bool HasActiveOwnerProfile(string email)
        {
            return !string.IsNullOrWhiteSpace(email) &&
                _repository.HasActiveOwnerProfile(email);
        }

        public string CompleteOwnerProfile(
            string username,
            CompleteOwnerProfileViewModel model)
        {
            var account = GetActiveOwnerAccount(username);
            if (account == null)
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            string email = model.Email.Trim().ToLowerInvariant();
            if (_repository.IsEmailUsedByAnotherAccount(account.Id, email))
            {
                return "Email đã được sử dụng.";
            }

            if (_repository.IsOwnerEmailUsed(email))
            {
                return "Email đã được liên kết với hồ sơ chủ trọ khác.";
            }

            var owner = new ChuTro
            {
                HoTen = model.HoTen.Trim(),
                Email = email,
                SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                    ? null
                    : model.SoDienThoai.Trim(),
                DiaChi = string.IsNullOrWhiteSpace(model.DiaChi)
                    ? null
                    : model.DiaChi.Trim(),
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            try
            {
                _repository.CompleteOwnerProfile(account, owner);
            }
            catch (DbUpdateException)
            {
                return "Không thể lưu hồ sơ chủ trọ. Hãy kiểm tra email và thử lại.";
            }

            return null;
        }
    }
}