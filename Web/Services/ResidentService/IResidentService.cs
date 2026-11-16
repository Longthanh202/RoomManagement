using Web.Models.ViewModel;

namespace Web.Services.ResidentService
{
    public interface IResidentService
    {
        ResidentDashboardViewModel GetDashboard(string username);
        string CreateRepairRequest(string username, CreateRepairRequestViewModel model);
        string RegisterGuest(string username, RegisterGuestViewModel model);
    }
}
