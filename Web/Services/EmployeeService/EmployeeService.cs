using Web.Models.ViewModel;
using Web.Repositories.EmployeeRepository;

namespace Web.Services.EmployeeService
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public EmployeeDashboardViewModel GetDashboard(string username)
        {
            return string.IsNullOrWhiteSpace(username)
                ? null
                : _repository.GetDashboard(username.Trim());
        }

        public string UpdateRepairStatus(string username, UpdateRepairStatusViewModel model)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                _repository.UpdateRepairStatus(username.Trim(), model)
                    ? null
                    : "Không thể cập nhật yêu cầu. Yêu cầu có thể không thuộc khu trọ được phân công hoặc trạng thái không hợp lệ.";
        }

        public string UpdateGuestStatus(string username, UpdateGuestStatusViewModel model)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                _repository.UpdateGuestStatus(username.Trim(), model)
                    ? null
                    : "Không thể cập nhật đăng ký khách. Kiểm tra quyền được phân công và trạng thái hiện tại.";
        }
    }
}
