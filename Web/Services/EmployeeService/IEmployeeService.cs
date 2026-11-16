using Web.Models.ViewModel;

namespace Web.Services.EmployeeService
{
    public interface IEmployeeService
    {
        EmployeeDashboardViewModel GetDashboard(string username);
        string UpdateRepairStatus(string username, UpdateRepairStatusViewModel model);
        string UpdateGuestStatus(string username, UpdateGuestStatusViewModel model);
    }
}
