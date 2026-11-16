using System.Web.Mvc;
using System.Web.Security;
using Web.Models.ViewModel;
using Web.Services.ResidentService;

namespace Web.Controllers
{
    [Authorize]
    public class ResidentController : Controller
    {
        private readonly IResidentService _residentService;

        public ResidentController(IResidentService residentService)
        {
            _residentService = residentService;
        }

        [HttpGet]
        public ActionResult Index()
        {
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
    }
}
