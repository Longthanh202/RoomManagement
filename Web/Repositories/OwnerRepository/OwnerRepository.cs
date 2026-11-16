using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Web.Models;
using Web.Models.ViewModel;

namespace Web.Repositories.OwnerRepository
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly QuanLyPhongTroEntities _db;

        public OwnerRepository(QuanLyPhongTroEntities db)
        {
            _db = db;
        }

        public ChuTro GetOwnerByUsername(string username)
        {
            var account = GetOwnerAccount(username);

            return account == null
                ? null
                : _db.ChuTro.FirstOrDefault(x =>
                    x.Email == account.Email && x.TrangThai);
        }

        public TaiKhoan GetOwnerAccount(string username)
        {
            return _db.TaiKhoan.FirstOrDefault(x =>
                x.TenDangNhap == username &&
                x.TrangThai &&
                x.LoaiTaiKhoan == "ChuTro");
        }

        public OwnerDashboardViewModel GetDashboard(long ownerId, long accountId, string ownerName)
        {
            var rooms = _db.Phong
                .Include("Tang.DayTro.KhuTro")
                .Where(x => x.Tang.DayTro.KhuTro.ChuTroId == ownerId)
                .OrderBy(x => x.Tang.DayTro.KhuTro.TenKhuTro)
                .ThenBy(x => x.Tang.DayTro.TenDayTro)
                .ThenBy(x => x.Tang.SoTang)
                .ThenBy(x => x.MaPhong)
                .ToList();
            var residents = _db.CuDan
                .Include("TaiKhoan")
                .Include("HopDongThanhVien.HopDong.Phong.Tang.DayTro.KhuTro")
                .Include("HopDong.Phong.Tang.DayTro.KhuTro")
                .Where(x =>
                    x.HopDong.Any(h =>
                        h.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId) ||
                    x.HopDongThanhVien.Any(m =>
                        m.HopDong.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId))
                .OrderBy(x => x.HoTen)
                .ToList();
            var employees = _db.NhanVien
                .Include("TaiKhoan")
                .Include("PhanCongNhanVien.KhuTro")
                .Where(x => x.ChuTroId == ownerId)
                .OrderBy(x => x.HoTen)
                .ToList();
            var contracts = _db.HopDong
                .Include("Phong.Tang.DayTro.KhuTro")
                .Include("CuDan")
                .Include("HopDongThanhVien.CuDan")
                .Where(x => x.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId)
                .OrderByDescending(x => x.NgayTao)
                .ToList();

            return new OwnerDashboardViewModel
            {
                HoTenChuTro = ownerName,
                KhuTro = _db.KhuTro
                    .Include("DayTro.Tang.Phong")
                    .Where(x => x.ChuTroId == ownerId)
                    .OrderBy(x => x.TenKhuTro)
                    .ToList(),
                NhanVien = employees,
                CuDan = residents,
                Phong = rooms,
                HopDong = contracts,
                LichSuCuTru = _db.LichSuCuTru
                    .Include("CuDan")
                    .Include("HopDong")
                    .Include("Phong.Tang.DayTro.KhuTro")
                    .Where(x => x.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId)
                    .OrderByDescending(x => x.NgayVao)
                    .Take(100)
                    .ToList(),
                DichVu = _db.DichVu
                    .Where(x => x.TrangThai)
                    .OrderBy(x => x.TenDichVu)
                    .ToList(),
                BangGiaDichVu = _db.BangGiaDichVu
                    .Include("KhuTro")
                    .Where(x => x.KhuTro.ChuTroId == ownerId)
                    .OrderByDescending(x => x.TuNgay)
                    .ToList(),
                ThongBao = _db.NguoiNhanThongBao
                    .Include("ThongBao")
                    .Where(x => x.TaiKhoanId == accountId)
                    .OrderByDescending(x => x.ThongBao.NgayTao)
                    .Take(20)
                    .ToList(),
                SoThongBaoChuaDoc = _db.NguoiNhanThongBao.Count(x =>
                    x.TaiKhoanId == accountId && !x.DaDoc),
                DayTroOptions = _db.DayTro
                    .Include("KhuTro")
                    .Where(x => x.KhuTro.ChuTroId == ownerId)
                    .OrderBy(x => x.KhuTro.TenKhuTro)
                    .ThenBy(x => x.TenDayTro)
                    .ToList(),
                TangOptions = _db.Tang
                    .Include("DayTro.KhuTro")
                    .Where(x => x.DayTro.KhuTro.ChuTroId == ownerId)
                    .OrderBy(x => x.DayTro.TenDayTro)
                    .ThenBy(x => x.SoTang)
                    .ToList(),
                PhongTrongOptions = rooms
                    .Where(x => x.TrangThai == "Trong")
                    .ToList(),
                SoKhuTro = _db.KhuTro.Count(x => x.ChuTroId == ownerId),
                SoPhong = rooms.Count,
                SoPhongTrong = rooms.Count(x => x.TrangThai == "Trong"),
                SoNhanVien = employees.Count,
                SoCuDan = residents.Count
            };
        }

        public bool IsKhuTroCodeUsed(long ownerId, string code)
        {
            return _db.KhuTro.Any(x => x.ChuTroId == ownerId && x.MaKhuTro == code);
        }

        public void CreateKhuTro(KhuTro khuTro)
        {
            _db.KhuTro.Add(khuTro);
            _db.SaveChanges();
        }

        public KhuTro GetOwnedKhuTro(long ownerId, long khuTroId)
        {
            return _db.KhuTro.FirstOrDefault(x =>
                x.Id == khuTroId && x.ChuTroId == ownerId);
        }

        public bool IsDayTroCodeUsed(long khuTroId, string code)
        {
            return _db.DayTro.Any(x => x.KhuTroId == khuTroId && x.MaDayTro == code);
        }

        public void CreateDayTro(DayTro dayTro)
        {
            _db.DayTro.Add(dayTro);
            _db.SaveChanges();
        }

        public DayTro GetOwnedDayTro(long ownerId, long dayTroId)
        {
            return _db.DayTro.FirstOrDefault(x =>
                x.Id == dayTroId && x.KhuTro.ChuTroId == ownerId);
        }

        public bool FloorExists(long dayTroId, int floorNumber)
        {
            return _db.Tang.Any(x =>
                x.DayTroId == dayTroId && x.SoTang == floorNumber);
        }

        public void CreateFloor(Tang floor)
        {
            _db.Tang.Add(floor);
            _db.SaveChanges();
        }

        public Tang GetOwnedFloor(long ownerId, long floorId)
        {
            return _db.Tang.FirstOrDefault(x =>
                x.Id == floorId && x.DayTro.KhuTro.ChuTroId == ownerId);
        }

        public bool IsRoomCodeUsed(long floorId, string code)
        {
            return _db.Phong.Any(x => x.TangId == floorId && x.MaPhong == code);
        }

        public void CreateRoom(Phong room)
        {
            _db.Phong.Add(room);
            _db.SaveChanges();
        }

        public bool IsUsernameUsed(string username)
        {
            return _db.TaiKhoan.Any(x => x.TenDangNhap == username);
        }

        public bool IsEmailUsed(string email)
        {
            return !string.IsNullOrWhiteSpace(email) &&
                _db.TaiKhoan.Any(x => x.Email == email);
        }

        public bool IsEmployeeCodeUsed(long ownerId, string employeeCode)
        {
            return _db.NhanVien.Any(x =>
                x.ChuTroId == ownerId && x.MaNhanVien == employeeCode);
        }

        public VaiTro FindActiveRole(IEnumerable<string> roleCodes)
        {
            var codes = roleCodes.ToArray();
            return _db.VaiTro.FirstOrDefault(x =>
                codes.Contains(x.MaVaiTro) && x.TrangThai);
        }

        public void CreateEmployee(
            TaiKhoan account,
            TaiKhoanVaiTro roleAssignment,
            NhanVien employee,
            PhanCongNhanVien assignment)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                _db.TaiKhoan.Add(account);
                _db.SaveChanges();

                roleAssignment.TaiKhoanId = account.Id;
                _db.TaiKhoanVaiTro.Add(roleAssignment);

                employee.TaiKhoanId = account.Id;
                _db.NhanVien.Add(employee);
                _db.SaveChanges();

                if (assignment != null)
                {
                    assignment.NhanVienId = employee.Id;
                    _db.PhanCongNhanVien.Add(assignment);
                }

                _db.SaveChanges();
                transaction.Commit();
            }
        }

        public Phong GetAvailableRoom(long ownerId, long roomId)
        {
            return _db.Phong.FirstOrDefault(x =>
                x.Id == roomId &&
                x.Tang.DayTro.KhuTro.ChuTroId == ownerId &&
                x.TrangThai == "Trong");
        }

        public VaiTro FindResidentRole()
        {
            return _db.VaiTro.FirstOrDefault(x =>
                (x.MaVaiTro == "CUDAN" || x.MaVaiTro == "CU_DAN") &&
                x.TrangThai);
        }

        public void CreateResident(
            long ownerId,
            long roomId,
            TaiKhoan account,
            TaiKhoanVaiTro roleAssignment,
            CuDan resident,
            HopDong contract,
            HopDongThanhVien contractMember)
        {
            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var room = GetAvailableRoom(ownerId, roomId);
                if (room == null)
                {
                    throw new RoomUnavailableException();
                }

                _db.TaiKhoan.Add(account);
                _db.SaveChanges();

                roleAssignment.TaiKhoanId = account.Id;
                _db.TaiKhoanVaiTro.Add(roleAssignment);

                resident.TaiKhoanId = account.Id;
                _db.CuDan.Add(resident);
                _db.SaveChanges();

                contract.PhongId = room.Id;
                contract.CuDanDaiDienId = resident.Id;
                _db.HopDong.Add(contract);
                _db.SaveChanges();

                contractMember.HopDongId = contract.Id;
                contractMember.CuDanId = resident.Id;
                _db.HopDongThanhVien.Add(contractMember);

                room.TrangThai = "DaDat";
                _db.SaveChanges();
                transaction.Commit();
            }
        }

        public bool AssignEmployee(long ownerId, AssignEmployeeViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var employee = _db.NhanVien.FirstOrDefault(x =>
                    x.Id == model.NhanVienId &&
                    x.ChuTroId == ownerId &&
                    x.TrangThai);
                var property = _db.KhuTro.FirstOrDefault(x =>
                    x.Id == model.KhuTroId &&
                    x.ChuTroId == ownerId &&
                    x.TrangThai);
                if (employee == null || property == null ||
                    _db.PhanCongNhanVien.Any(x =>
                        x.NhanVienId == employee.Id &&
                        x.KhuTroId == property.Id &&
                        !x.NgayKetThuc.HasValue))
                {
                    return false;
                }

                _db.PhanCongNhanVien.Add(new PhanCongNhanVien
                {
                    NhanVienId = employee.Id,
                    KhuTroId = property.Id,
                    NgayBatDau = model.NgayBatDau.Date,
                    GhiChu = string.IsNullOrWhiteSpace(model.GhiChu)
                        ? null
                        : model.GhiChu.Trim()
                });
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
        }

        public bool AddRoomMember(long ownerId, AddRoomMemberViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var contract = _db.HopDong
                    .Include("Phong")
                    .FirstOrDefault(x =>
                        x.Id == model.HopDongId &&
                        x.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId &&
                        x.TrangThai == "DangHieuLuc");
                var resident = _db.CuDan.FirstOrDefault(x =>
                    x.Id == model.CuDanId &&
                    x.TrangThai &&
                    (x.HopDong.Any(h =>
                        h.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId) ||
                     x.HopDongThanhVien.Any(member =>
                        member.HopDong.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId)));
                if (contract == null ||
                    resident == null ||
                    model.NgayBatDau.Date < contract.NgayBatDau.Date ||
                    model.NgayBatDau.Date > contract.NgayKetThuc.Date)
                {
                    return false;
                }

                bool alreadyLivingElsewhere = _db.HopDong.Any(x =>
                    x.CuDanDaiDienId == resident.Id &&
                    (x.TrangThai == "ChoDuyet" || x.TrangThai == "DangHieuLuc")) ||
                    _db.HopDongThanhVien.Any(x =>
                        x.CuDanId == resident.Id &&
                        x.TrangThai &&
                        (x.HopDong.TrangThai == "ChoDuyet" ||
                         x.HopDong.TrangThai == "DangHieuLuc"));
                int currentMembers = _db.HopDongThanhVien.Count(x =>
                    x.HopDongId == contract.Id && x.TrangThai);
                if (alreadyLivingElsewhere ||
                    currentMembers >= contract.Phong.SoNguoiToiDa ||
                    _db.HopDongThanhVien.Any(x =>
                        x.HopDongId == contract.Id && x.CuDanId == resident.Id))
                {
                    return false;
                }

                var startDate = model.NgayBatDau.Date;
                _db.HopDongThanhVien.Add(new HopDongThanhVien
                {
                    HopDongId = contract.Id,
                    CuDanId = resident.Id,
                    LoaiThanhVien = "ThanhVien",
                    LaNguoiDaiDien = false,
                    NgayBatDau = startDate,
                    TrangThai = true
                });
                _db.LichSuCuTru.Add(new LichSuCuTru
                {
                    CuDanId = resident.Id,
                    PhongId = contract.PhongId,
                    HopDongId = contract.Id,
                    NgayVao = startDate
                });
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
        }

        public bool EndRoomMember(long ownerId, EndRoomMemberViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var member = _db.HopDongThanhVien
                    .Include("HopDong.Phong")
                    .FirstOrDefault(x =>
                        x.Id == model.HopDongThanhVienId &&
                        x.TrangThai &&
                        x.HopDong.TrangThai == "DangHieuLuc" &&
                        x.HopDong.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId);
                if (member == null)
                {
                    return false;
                }

                var today = DateTime.Today;
                member.TrangThai = false;
                member.NgayKetThuc = member.NgayBatDau > today
                    ? member.NgayBatDau
                    : today;
                var history = _db.LichSuCuTru.FirstOrDefault(x =>
                    x.HopDongId == member.HopDongId &&
                    x.CuDanId == member.CuDanId &&
                    !x.NgayRa.HasValue);
                if (history != null)
                {
                    history.NgayRa = history.NgayVao > today
                        ? history.NgayVao
                        : today;
                    history.LyDo = string.IsNullOrWhiteSpace(model.LyDo)
                        ? "Kết thúc cư trú"
                        : model.LyDo.Trim();
                }

                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
        }

        public bool UpdateContractStatus(long ownerId, UpdateContractStatusViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var contract = _db.HopDong
                    .Include("Phong")
                    .FirstOrDefault(x =>
                        x.Id == model.HopDongId &&
                        x.Phong.Tang.DayTro.KhuTro.ChuTroId == ownerId);
                if (contract == null)
                {
                    return false;
                }

                if (model.TrangThai == "DangHieuLuc" &&
                    contract.TrangThai == "ChoDuyet" &&
                    contract.NgayKetThuc.Date >= DateTime.Today)
                {
                    bool roomHasAnotherCurrentContract = _db.HopDong.Any(x =>
                        x.Id != contract.Id &&
                        x.PhongId == contract.PhongId &&
                        (x.TrangThai == "DangHieuLuc" || x.TrangThai == "ChoDuyet"));
                    if (roomHasAnotherCurrentContract)
                    {
                        return false;
                    }

                    contract.TrangThai = "DangHieuLuc";
                    contract.Phong.TrangThai = "DangThue";
                    var members = _db.HopDongThanhVien
                        .Where(x => x.HopDongId == contract.Id && x.TrangThai)
                        .ToList();
                    foreach (var member in members)
                    {
                        bool hasOpenHistory = _db.LichSuCuTru.Any(x =>
                            x.HopDongId == contract.Id &&
                            x.CuDanId == member.CuDanId &&
                            !x.NgayRa.HasValue);
                        if (!hasOpenHistory)
                        {
                            _db.LichSuCuTru.Add(new LichSuCuTru
                            {
                                CuDanId = member.CuDanId,
                                PhongId = contract.PhongId,
                                HopDongId = contract.Id,
                                NgayVao = member.NgayBatDau
                            });
                        }
                    }
                }
                else if (model.TrangThai == "DaKetThuc" &&
                    contract.TrangThai == "DangHieuLuc")
                {
                    var today = DateTime.Today;
                    contract.TrangThai = "DaKetThuc";
                    foreach (var member in _db.HopDongThanhVien
                        .Where(x => x.HopDongId == contract.Id && x.TrangThai))
                    {
                        member.TrangThai = false;
                        member.NgayKetThuc = member.NgayBatDau > today
                            ? member.NgayBatDau
                            : today;
                    }

                    foreach (var history in _db.LichSuCuTru
                        .Where(x => x.HopDongId == contract.Id && !x.NgayRa.HasValue))
                    {
                        history.NgayRa = history.NgayVao > today
                            ? history.NgayVao
                            : today;
                        history.LyDo = "Kết thúc hợp đồng";
                    }

                    bool hasOtherCurrentContract = _db.HopDong.Any(x =>
                        x.Id != contract.Id &&
                        x.PhongId == contract.PhongId &&
                        (x.TrangThai == "DangHieuLuc" || x.TrangThai == "ChoDuyet"));
                    if (!hasOtherCurrentContract)
                    {
                        contract.Phong.TrangThai = "Trong";
                    }
                }
                else if (model.TrangThai == "TuChoi" &&
                    contract.TrangThai == "ChoDuyet")
                {
                    contract.TrangThai = "TuChoi";
                    var today = DateTime.Today;
                    foreach (var member in _db.HopDongThanhVien
                        .Where(x => x.HopDongId == contract.Id && x.TrangThai))
                    {
                        member.TrangThai = false;
                        member.NgayKetThuc = member.NgayBatDau > today
                            ? member.NgayBatDau
                            : today;
                    }

                    bool hasOtherCurrentContract = _db.HopDong.Any(x =>
                        x.Id != contract.Id &&
                        x.PhongId == contract.PhongId &&
                        (x.TrangThai == "DangHieuLuc" || x.TrangThai == "ChoDuyet"));
                    if (!hasOtherCurrentContract)
                    {
                        contract.Phong.TrangThai = "Trong";
                    }
                }
                else
                {
                    return false;
                }

                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
        }

        public bool SetServicePrice(long ownerId, SetServicePriceViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var property = _db.KhuTro.FirstOrDefault(x =>
                    x.Id == model.KhuTroId &&
                    x.ChuTroId == ownerId &&
                    x.TrangThai);
                var service = _db.DichVu.FirstOrDefault(x =>
                    x.Id == model.DichVuId && x.TrangThai);
                if (property == null || service == null)
                {
                    return false;
                }

                var effectiveDate = model.TuNgay.Date;
                var currentPrices = _db.BangGiaDichVu
                    .Where(x =>
                        x.DichVuId == service.Id &&
                        x.KhuTroId == property.Id &&
                        x.TrangThai)
                    .ToList();
                if (currentPrices.Any(x => x.TuNgay >= effectiveDate))
                {
                    return false;
                }

                foreach (var current in currentPrices.Where(x =>
                    !x.DenNgay.HasValue || x.DenNgay.Value >= effectiveDate))
                {
                    current.DenNgay = effectiveDate.AddDays(-1);
                    current.TrangThai = false;
                }

                _db.BangGiaDichVu.Add(new BangGiaDichVu
                {
                    DichVuId = service.Id,
                    KhuTroId = property.Id,
                    DonGia = model.DonGia,
                    TuNgay = effectiveDate,
                    TrangThai = true
                });
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
        }

        public bool IsServiceCodeUsed(string code)
        {
            return _db.DichVu.Any(x => x.MaDichVu == code);
        }

        public void CreateService(DichVu service)
        {
            _db.DichVu.Add(service);
            _db.SaveChanges();
        }

        public bool MarkNotificationRead(long accountId, long notificationRecipientId)
        {
            var notification = _db.NguoiNhanThongBao.FirstOrDefault(x =>
                x.Id == notificationRecipientId && x.TaiKhoanId == accountId);
            if (notification == null)
            {
                return false;
            }

            if (!notification.DaDoc)
            {
                notification.DaDoc = true;
                notification.ThoiGianDoc = DateTime.Now;
                _db.SaveChanges();
            }

            return true;
        }
    }

    public class RoomUnavailableException : System.Exception
    {
    }
}
