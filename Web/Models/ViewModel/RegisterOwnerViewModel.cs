using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Web.Models;

namespace Web.Models.ViewModel
{
    public class RegisterOwnerViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(200)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, MinimumLength = 4)]
        [RegularExpression(
            @"^[a-zA-Z0-9._-]+$",
            ErrorMessage = "Tên đăng nhập không hợp lệ")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [StringLength(500)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("MatKhau",
            ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string XacNhanMatKhau { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class CompleteOwnerProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(200)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [StringLength(500)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
    }

    public class OwnerDashboardViewModel
    {
        public string HoTenChuTro { get; set; }
        public int SoKhuTro { get; set; }
        public int SoPhong { get; set; }
        public int SoPhongTrong { get; set; }
        public int SoNhanVien { get; set; }
        public int SoCuDan { get; set; }
        public IList<KhuTro> KhuTro { get; set; }
        public IList<NhanVien> NhanVien { get; set; }
        public IList<CuDan> CuDan { get; set; }
        public IList<Phong> Phong { get; set; }
        public IList<DayTro> DayTroOptions { get; set; }
        public IList<Tang> TangOptions { get; set; }
        public IList<Phong> PhongTrongOptions { get; set; }
        public IList<HopDong> HopDong { get; set; }
        public IList<LichSuCuTru> LichSuCuTru { get; set; }
        public IList<DichVu> DichVu { get; set; }
        public IList<BangGiaDichVu> BangGiaDichVu { get; set; }
        public IList<NguoiNhanThongBao> ThongBao { get; set; }
        public int SoThongBaoChuaDoc { get; set; }
    }

    public class AssignEmployeeViewModel
    {
        [Required]
        public long NhanVienId { get; set; }

        [Required]
        public long KhuTroId { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string GhiChu { get; set; }
    }

    public class AddRoomMemberViewModel
    {
        [Required]
        public long HopDongId { get; set; }

        [Required]
        public long CuDanId { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; } = DateTime.Today;
    }

    public class EndRoomMemberViewModel
    {
        [Required]
        public long HopDongThanhVienId { get; set; }

        [StringLength(500)]
        public string LyDo { get; set; }
    }

    public class SetServicePriceViewModel
    {
        [Required]
        public long DichVuId { get; set; }

        [Required]
        public long KhuTroId { get; set; }

        [Range(0, 999999999999, ErrorMessage = "Đơn giá không hợp lệ")]
        public decimal DonGia { get; set; }

        [DataType(DataType.Date)]
        public DateTime TuNgay { get; set; } = DateTime.Today;
    }

    public class CreateServiceViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã dịch vụ")]
        [StringLength(50)]
        public string MaDichVu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
        [StringLength(200)]
        public string TenDichVu { get; set; }

        [Required]
        public string LoaiTinhPhi { get; set; }

        [StringLength(50)]
        public string DonViTinh { get; set; }

        [StringLength(500)]
        public string MoTa { get; set; }
    }

    public class UpdateContractStatusViewModel
    {
        [Required]
        public long HopDongId { get; set; }

        [Required]
        public string TrangThai { get; set; }
    }

    public class PortalDashboardViewModel
    {
        public string HoTen { get; set; }
        public string LoaiTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }
        public string TenPhong { get; set; }
        public string TenKhuTro { get; set; }
        public string TenDayTro { get; set; }
        public string VaiTro { get; set; }
        public string KhuTroPhuTrach { get; set; }
        public bool YeuCauDoiMatKhau { get; set; }
    }

    public class CreateEmployeeViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã nhân viên")]
        [StringLength(50)]
        public string MaNhanVien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(200)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, MinimumLength = 4)]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Tên đăng nhập không hợp lệ")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(10, ErrorMessage = "Mật khẩu tối thiểu 10 ký tự")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [StringLength(20)]
        public string GioiTinh { get; set; }

        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [StringLength(100)]
        public string ChucVu { get; set; }

        [StringLength(500)]
        public string DiaChi { get; set; }

        [Required]
        public string MaVaiTro { get; set; }

        public long? KhuTroId { get; set; }
    }

    public class CreateResidentViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(200)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, MinimumLength = 4)]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Tên đăng nhập không hợp lệ")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(10, ErrorMessage = "Mật khẩu tối thiểu 10 ký tự")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [StringLength(30)]
        public string SoCCCD { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [StringLength(20)]
        public string GioiTinh { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayCapCCCD { get; set; }

        [StringLength(200)]
        public string NoiCapCCCD { get; set; }

        [StringLength(500)]
        public string DiaChiThuongTru { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        public long PhongId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime? NgayBatDau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime? NgayKetThuc { get; set; }

        [Range(0, 999999999999, ErrorMessage = "Giá thuê không hợp lệ")]
        public decimal GiaThue { get; set; }

        [Range(0, 999999999999, ErrorMessage = "Tiền cọc không hợp lệ")]
        public decimal TienCoc { get; set; }

        [Range(1, 31, ErrorMessage = "Ngày thanh toán từ 1 đến 31")]
        public int NgayThanhToan { get; set; }
    }

    public class CreateKhuTroViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã khu trọ")]
        [StringLength(50)]
        public string MaKhuTro { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khu trọ")]
        [StringLength(200)]
        public string TenKhuTro { get; set; }

        [StringLength(500)]
        public string DiaChi { get; set; }

        public string MoTa { get; set; }
    }

    public class CreateDayTroViewModel
    {
        [Required]
        public long KhuTroId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã dãy")]
        [StringLength(50)]
        public string MaDayTro { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dãy")]
        [StringLength(200)]
        public string TenDayTro { get; set; }

        [StringLength(500)]
        public string MoTa { get; set; }
    }

    public class CreateFloorViewModel
    {
        [Required]
        public long DayTroId { get; set; }

        [Range(1, 100, ErrorMessage = "Số tầng phải từ 1 đến 100")]
        public int SoTang { get; set; }

        [StringLength(100)]
        public string TenTang { get; set; }
    }

    public class CreateRoomViewModel
    {
        [Required]
        public long TangId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã phòng")]
        [StringLength(50)]
        public string MaPhong { get; set; }

        [StringLength(100)]
        public string TenPhong { get; set; }

        [Range(0.01, 999999, ErrorMessage = "Diện tích phải lớn hơn 0")]
        public decimal? DienTich { get; set; }

        [Range(1, 100)]
        public int SoNguoiToiDa { get; set; } = 4;

        [Range(0, 999999999999, ErrorMessage = "Giá thuê không hợp lệ")]
        public decimal GiaThue { get; set; }

        [Required]
        public string TrangThai { get; set; } = "Trong";

        public string MoTa { get; set; }
    }
}