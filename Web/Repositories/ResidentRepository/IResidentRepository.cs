using Web.Models.ViewModel;

namespace Web.Repositories.ResidentRepository
{
    public interface IResidentRepository
    {
        ResidentDashboardViewModel GetDashboard(string username);
        bool CreateRepairRequest(string username, CreateRepairRequestViewModel model);
        bool RegisterGuest(string username, RegisterGuestViewModel model);
    }
}
