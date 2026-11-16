using System;
using System.Web.Mvc;
using System.Web.Security;
using Web.Models.ViewModel;
using Web.Services.OwnerService;

namespace Web.Controllers
{
    [Authorize]
    public class OwnerController : Controller
    {
        private readonly IOwnerService _ownerService;

        public OwnerController(IOwnerService ownerService)
        {
            _ownerService = ownerService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = _ownerService.GetDashboard(User.Identity.Name);
            if (model == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateKhuTro(CreateKhuTroViewModel model)
        {
            return Create(() => _ownerService.CreateKhuTro(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin khu trọ.",
                "Đã tạo khu trọ.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDayTro(CreateDayTroViewModel model)
        {
            return Create(() => _ownerService.CreateDayTro(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin dãy trọ.",
                "Đã tạo dãy trọ.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFloor(CreateFloorViewModel model)
        {
            return Create(() => _ownerService.CreateFloor(User.Identity.Name, model),
                "Vui lòng kiểm tra lại số tầng.",
                "Đã tạo tầng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRoom(CreateRoomViewModel model)
        {
            return Create(() => _ownerService.CreateRoom(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin phòng.",
                "Đã tạo phòng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEmployee(CreateEmployeeViewModel model)
        {
            return Create(() => _ownerService.CreateEmployee(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin nhân viên.",
                "Đã tạo tài khoản nhân viên.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateResident(CreateResidentViewModel model)
        {
            return Create(() => _ownerService.CreateResident(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin cư dân.",
                "Đã tạo tài khoản cư dân và hợp đồng ở trạng thái chờ duyệt.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignEmployee(AssignEmployeeViewModel model)
        {
            return Create(() => _ownerService.AssignEmployee(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin phân công.",
                "Đã phân công nhân viên.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRoomMember(AddRoomMemberViewModel model)
        {
            return Create(() => _ownerService.AddRoomMember(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thành viên và hợp đồng.",
                "Đã thêm thành viên vào hợp đồng và lịch sử cư trú.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EndRoomMember(EndRoomMemberViewModel model)
        {
            return Create(() => _ownerService.EndRoomMember(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin thành viên.",
                "Đã cập nhật thành viên và lịch sử cư trú.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateContract(UpdateContractStatusViewModel model)
        {
            return Create(() => _ownerService.UpdateContractStatus(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin hợp đồng.",
                "Đã cập nhật trạng thái hợp đồng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateService(CreateServiceViewModel model)
        {
            return Create(() => _ownerService.CreateService(User.Identity.Name, model),
                "Vui lòng kiểm tra lại thông tin dịch vụ.",
                "Đã thêm dịch vụ.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetServicePrice(SetServicePriceViewModel model)
        {
            return Create(() => _ownerService.SetServicePrice(User.Identity.Name, model),
                "Vui lòng kiểm tra lại đơn giá dịch vụ.",
                "Đã cập nhật giá dịch vụ.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkNotificationRead(long id)
        {
            if (!_ownerService.MarkNotificationRead(User.Identity.Name, id))
            {
                TempData["Error"] = "Không tìm thấy thông báo trong tài khoản của bạn.";
            }

            return RedirectToAction("Index");
        }

        private ActionResult Create(Func<string> operation, string invalidModelMessage, string successMessage)
        {
            if (!_ownerService.IsOwnerActive(User.Identity.Name))
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = invalidModelMessage;
                return RedirectToAction("Index");
            }

            string error = operation();
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Index");
            }

            TempData["Success"] = successMessage;
            return RedirectToAction("Index");
        }
    }
}
