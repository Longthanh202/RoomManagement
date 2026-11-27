using System.Web.Mvc;
using System.Web.Security;
using Web.Models.ViewModel;
using Web.Services.ResidentService;
using Web.Services.TaiKhoanService;

namespace Web.Controllers
{
    [Authorize]
    public class ResidentController : Controller
    {
        private readonly IResidentService _residentService;
        private readonly ITaiKhoanService _taiKhoanService;

        public ResidentController(
            IResidentService residentService,
            ITaiKhoanService taiKhoanService)
        {
            _residentService = residentService;
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

            var model = _residentService.GetDashboard(User.Identity.Name);
            if (model == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRepairRequest(CreateRepairRequestViewModel request)
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

            string error = _residentService.CreateRepairRequest(User.Identity.Name, request);
            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã gửi yêu cầu sửa chữa.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterGuest(RegisterGuestViewModel guest)
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

            string error = _residentService.RegisterGuest(User.Identity.Name, guest);
            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã gửi đăng ký khách chờ lễ tân xác nhận.";
            }

            return RedirectToAction("Index");
        }

        private ActionResult ReturnDashboardWithValidationErrors()
        {
            var dashboard = _residentService.GetDashboard(User.Identity.Name);
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
            if (portal == null || portal.LoaiTaiKhoan != "CuDan")
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
