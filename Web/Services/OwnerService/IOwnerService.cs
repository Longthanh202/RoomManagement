using Web.Models.ViewModel;

namespace Web.Services.OwnerService
{
    public interface IOwnerService
    {
        bool IsOwnerActive(string username);

        OwnerDashboardViewModel GetDashboard(string username);

        string CreateKhuTro(string username, CreateKhuTroViewModel model);
        string CreateDayTro(string username, CreateDayTroViewModel model);
        string CreateFloor(string username, CreateFloorViewModel model);
        string CreateRoom(string username, CreateRoomViewModel model);
        string CreateEmployee(string username, CreateEmployeeViewModel model);
        string CreateResident(string username, CreateResidentViewModel model);
        string AssignEmployee(string username, AssignEmployeeViewModel model);
        string AddRoomMember(string username, AddRoomMemberViewModel model);
        string EndRoomMember(string username, EndRoomMemberViewModel model);
        string UpdateContractStatus(string username, UpdateContractStatusViewModel model);
        string CreateService(string username, CreateServiceViewModel model);
        string SetServicePrice(string username, SetServicePriceViewModel model);
        bool MarkNotificationRead(string username, long notificationRecipientId);
    }
}
