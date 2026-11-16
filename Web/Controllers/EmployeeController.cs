using System.Web.Mvc;
using System.Web.Security;
using Web.Models.ViewModel;
using Web.Services.EmployeeService;

namespace Web.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = _employeeService.GetDashboard(User.Identity.Name);
            if (model == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateRepairStatus(UpdateRepairStatusViewModel update)
        {
            if (!ModelState.IsValid)
            {
                return ReturnDashboardWithValidationErrors();
            }

            string error = _employeeService.UpdateRepairStatus(User.Identity.Name, update);
            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã cập nhật tiến độ sửa chữa.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateGuestStatus(UpdateGuestStatusViewModel update)
        {
            if (!ModelState.IsValid)
            {
                return ReturnDashboardWithValidationErrors();
            }

            string error = _employeeService.UpdateGuestStatus(User.Identity.Name, update);
            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã cập nhật đăng ký khách.";
            }

            return RedirectToAction("Index");
        }

        private ActionResult ReturnDashboardWithValidationErrors()
        {
            var dashboard = _employeeService.GetDashboard(User.Identity.Name);
            if (dashboard == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }

            return View("Index", dashboard);
        }
    }
}
