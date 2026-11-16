using System.Collections.Generic;
using Web.Models;
using Web.Models.ViewModel;

namespace Web.Repositories.OwnerRepository
{
    public interface IOwnerRepository
    {
        ChuTro GetOwnerByUsername(string username);

        TaiKhoan GetOwnerAccount(string username);

        OwnerDashboardViewModel GetDashboard(long ownerId, long accountId, string ownerName);

        bool IsKhuTroCodeUsed(long ownerId, string code);
        void CreateKhuTro(KhuTro khuTro);

        KhuTro GetOwnedKhuTro(long ownerId, long khuTroId);
        bool IsDayTroCodeUsed(long khuTroId, string code);
        void CreateDayTro(DayTro dayTro);

        DayTro GetOwnedDayTro(long ownerId, long dayTroId);
        bool FloorExists(long dayTroId, int floorNumber);
        void CreateFloor(Tang floor);

        Tang GetOwnedFloor(long ownerId, long floorId);
        bool IsRoomCodeUsed(long floorId, string code);
        void CreateRoom(Phong room);

        bool IsUsernameUsed(string username);
        bool IsEmailUsed(string email);
        bool IsEmployeeCodeUsed(long ownerId, string employeeCode);
        VaiTro FindActiveRole(IEnumerable<string> roleCodes);
        void CreateEmployee(
            TaiKhoan account,
            TaiKhoanVaiTro roleAssignment,
            NhanVien employee,
            PhanCongNhanVien assignment);

        Phong GetAvailableRoom(long ownerId, long roomId);
        VaiTro FindResidentRole();
        void CreateResident(
            long ownerId,
            long roomId,
            TaiKhoan account,
            TaiKhoanVaiTro roleAssignment,
            CuDan resident,
            HopDong contract,
            HopDongThanhVien contractMember);

        bool AssignEmployee(long ownerId, AssignEmployeeViewModel model);
        bool AddRoomMember(long ownerId, AddRoomMemberViewModel model);
        bool EndRoomMember(long ownerId, EndRoomMemberViewModel model);
        bool UpdateContractStatus(long ownerId, UpdateContractStatusViewModel model);
        bool IsServiceCodeUsed(string code);
        void CreateService(DichVu service);
        bool SetServicePrice(long ownerId, SetServicePriceViewModel model);
        bool MarkNotificationRead(long accountId, long notificationRecipientId);
    }
}
