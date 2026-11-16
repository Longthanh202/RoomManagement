using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.Models;

namespace Web.Models.ViewModel
{
    public class ResidentDashboardViewModel
    {
        public ResidentDashboardViewModel()
        {
            HopDong = new List<HopDong>();
            LichSuCuTru = new List<LichSuCuTru>();
            HoaDon = new List<HoaDon>();
            BangGiaDichVu = new List<BangGiaDichVu>();
            YeuCauSuaChua = new List<YeuCauSuaChua>();
            DangKyKhach = new List<DangKyKhach>();
            ThongBao = new List<NguoiNhanThongBao>();
        }

        public string HoTen { get; set; }
        public string TenDangNhap { get; set; }
        public string TenKhuTro { get; set; }
        public string TenDayTro { get; set; }
        public string TenPhong { get; set; }
        public bool YeuCauDoiMatKhau { get; set; }
        public int SoThongBaoChuaDoc { get; set; }
        public bool CoHopDongHienTai { get; set; }
        public IList<HopDong> HopDong { get; set; }
        public IList<LichSuCuTru> LichSuCuTru { get; set; }
        public IList<HoaDon> HoaDon { get; set; }
        public IList<BangGiaDichVu> BangGiaDichVu { get; set; }
        public IList<YeuCauSuaChua> YeuCauSuaChua { get; set; }
        public IList<DangKyKhach> DangKyKhach { get; set; }
        public IList<NguoiNhanThongBao> ThongBao { get; set; }
    }

    public class CreateRepairRequestViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề yêu cầu.")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Vui lòng mô tả sự cố.")]
        [StringLength(2000)]
        [Display(Name = "Mô tả")]
        public string NoiDung { get; set; }

        [Required]
        [RegularExpression("^(Thap|BinhThuong|Cao|KhanCap)$", ErrorMessage = "Mức độ ưu tiên không hợp lệ.")]
        [Display(Name = "Mức độ ưu tiên")]
        public string MucDoUuTien { get; set; }
    }

    public class RegisterGuestViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khách.")]
        [StringLength(200)]
        [Display(Name = "Tên khách")]
        public string TenKhach { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [StringLength(20)]
        [Display(Name = "Số CCCD")]
        public string SoCCCD { get; set; }

        [Range(1, 20, ErrorMessage = "Số lượng khách phải từ 1 đến 20.")]
        [Display(Name = "Số lượng khách")]
        public int SoLuongKhach { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian khách đến.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Dự kiến đến")]
        public DateTime? ThoiGianDuKienDen { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian khách rời đi.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Dự kiến về")]
        public DateTime? ThoiGianDuKienVe { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
    }

    public class EmployeeDashboardViewModel
    {
        public EmployeeDashboardViewModel()
        {
            KhuTro = new List<KhuTro>();
            HopDong = new List<HopDong>();
            HoaDon = new List<HoaDon>();
            YeuCauSuaChua = new List<YeuCauSuaChua>();
            DangKyKhach = new List<DangKyKhach>();
            ThongBao = new List<NguoiNhanThongBao>();
        }

        public string HoTen { get; set; }
        public string TenDangNhap { get; set; }
        public string ChucVu { get; set; }
        public bool YeuCauDoiMatKhau { get; set; }
        public bool CoQuyenXuLySuaChua { get; set; }
        public bool CoQuyenXuLyKhach { get; set; }
        public bool CoQuyenXemCuDan { get; set; }
        public bool CoQuyenXemHoaDon { get; set; }
        public int SoThongBaoChuaDoc { get; set; }
        public IList<KhuTro> KhuTro { get; set; }
        public IList<HopDong> HopDong { get; set; }
        public IList<HoaDon> HoaDon { get; set; }
        public IList<YeuCauSuaChua> YeuCauSuaChua { get; set; }
        public IList<DangKyKhach> DangKyKhach { get; set; }
        public IList<NguoiNhanThongBao> ThongBao { get; set; }
    }

    public class UpdateRepairStatusViewModel
    {
        [Required]
        public long YeuCauSuaChuaId { get; set; }

        [Required]
        public string TrangThai { get; set; }

        [StringLength(1000)]
        public string NoiDung { get; set; }
    }

    public class UpdateGuestStatusViewModel
    {
        [Required]
        public long DangKyKhachId { get; set; }

        [Required]
        public string TrangThai { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }
    }
}
