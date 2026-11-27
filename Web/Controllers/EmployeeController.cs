using System.Web.Mvc;
using System.Web.Security;
using Web.Models.ViewModel;
using Web.Services.EmployeeService;
using Web.Services.TaiKhoanService;

namespace Web.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ITaiKhoanService _taiKhoanService;

        public EmployeeController(
            IEmployeeService employeeService,
            ITaiKhoanService taiKhoanService)
        {
            _employeeService = employeeService;
            _taiKhoanService = taiKhoanService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var passwordRedirect = GetPasswordChangeRedirect();
            if (passwordRedirect != null)
            {
                return passwordRedirect;
            }

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
            var passwordRedirect = GetPasswordChangeRedirect();
            if (passwordRedirect != null)
            {
                return passwordRedirect;
            }

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
            var passwordRedirect = GetPasswordChangeRedirect();
            if (passwordRedirect != null)
            {
                return passwordRedirect;
            }

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

        private ActionResult GetPasswordChangeRedirect()
        {
            var portal = _taiKhoanService.GetPortal(User.Identity.Name);
            if (portal == null || portal.LoaiTaiKhoan != "NhanVien")
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }

            return portal.YeuCauDoiMatKhau
                ? (ActionResult)RedirectToAction("ChangePassword", "Account")
                : null;
        }
    }
}
