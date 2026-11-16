using Web.Models.ViewModel;

namespace Web.Repositories.EmployeeRepository
{
    public interface IEmployeeRepository
    {
        EmployeeDashboardViewModel GetDashboard(string username);
        bool UpdateRepairStatus(string username, UpdateRepairStatusViewModel model);
        bool UpdateGuestStatus(string username, UpdateGuestStatusViewModel model);
    }
}
