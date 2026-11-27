using System.Web.Mvc;
using System.Web.Security;
using Web.Models.ViewModel;
using Web.Services.TaiKhoanService;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ITaiKhoanService _taiKhoanService;

        public AccountController(ITaiKhoanService taiKhoanService)
        {
            _taiKhoanService = taiKhoanService;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult RegisterOwner()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectAuthenticatedAccount();
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterOwner(RegisterOwnerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string error = _taiKhoanService.RegisterOwner(model);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["Success"] = "Đăng ký tài khoản chủ trọ thành công!";
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectAuthenticatedAccount();
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var account = _taiKhoanService.AuthenticateAccount(
                model.TenDangNhap,
                model.MatKhau);

            if (account == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(account.TenDangNhap, false);

            // Nhân viên và cư dân phải đổi mật khẩu ở lần đăng nhập đầu tiên
            if ((account.LoaiTaiKhoan == "NhanVien" ||
                 account.LoaiTaiKhoan == "CuDan") &&
                account.YeuCauDoiMatKhau == true)
            {
                return RedirectToAction("ChangePassword", "Account");
            }

            // Xử lý tài khoản chủ trọ
            if (account.LoaiTaiKhoan == "ChuTro" &&
                !_taiKhoanService.HasActiveOwnerProfile(account.Email))
            {
                return RedirectToAction("CompleteOwnerProfile");
            }

            if (account.LoaiTaiKhoan == "ChuTro" &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (account.LoaiTaiKhoan == "ChuTro")
            {
                return RedirectToAction("Index", "Owner");
            }

            return account.LoaiTaiKhoan == "NhanVien"
                ? (ActionResult)RedirectToAction("Index", "Employee")
                : RedirectToAction("Index", "Resident");
        }


        [Authorize]
        [HttpGet]
        public ActionResult Portal()
        {
            var ownerAccount = _taiKhoanService.GetActiveOwnerAccount(User.Identity.Name);
            if (ownerAccount != null)
            {
                return _taiKhoanService.HasActiveOwnerProfile(ownerAccount.Email)
                    ? (ActionResult)RedirectToAction("Index", "Owner")
                    : RedirectToAction("CompleteOwnerProfile");
            }

            var model = _taiKhoanService.GetPortal(User.Identity.Name);
            if (model == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            return model.LoaiTaiKhoan == "NhanVien"
                ? (ActionResult)RedirectToAction("Index", "Employee")
                : RedirectToAction("Index", "Resident");
        }

        [Authorize]
        [HttpGet]
        public ActionResult CompleteOwnerProfile()
        {
            var account = _taiKhoanService.GetActiveOwnerAccount(User.Identity.Name);
            if (account == null)
            {
                if (_taiKhoanService.GetPortal(User.Identity.Name) != null)
                {
                    return RedirectToAction("Portal");
                }

                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            if (_taiKhoanService.HasActiveOwnerProfile(account.Email))
            {
                return RedirectToAction("Index", "Owner");
            }

            return View(new CompleteOwnerProfileViewModel
            {
                Email = account.Email,
                SoDienThoai = account.SoDienThoai
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteOwnerProfile(CompleteOwnerProfileViewModel model)
        {
            var account = _taiKhoanService.GetActiveOwnerAccount(User.Identity.Name);
            if (account == null)
            {
                if (_taiKhoanService.GetPortal(User.Identity.Name) != null)
                {
                    return RedirectToAction("Portal");
                }

                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string error = _taiKhoanService.CompleteOwnerProfile(
                account.TenDangNhap,
                model);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["Success"] = "Đã hoàn thiện hồ sơ chủ trọ.";
            return RedirectToAction("Index", "Owner");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkMemberNotificationRead(long id)
        {
            var portal = _taiKhoanService.GetPortal(User.Identity.Name);
            if (portal == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            if (portal.YeuCauDoiMatKhau)
            {
                return RedirectToAction("ChangePassword");
            }

            if (!_taiKhoanService.MarkNotificationRead(User.Identity.Name, id))
            {
                TempData["Error"] = "Không tìm thấy thông báo trong tài khoản này.";
            }

            if (portal.LoaiTaiKhoan == "NhanVien")
            {
                return RedirectToAction("Index", "Employee");
            }

            return portal.LoaiTaiKhoan == "CuDan"
                ? (ActionResult)RedirectToAction("Index", "Resident")
                : RedirectToAction("Index", "Owner");
        }

        private ActionResult RedirectAuthenticatedAccount()
        {
            var account = _taiKhoanService.GetActiveOwnerAccount(User.Identity.Name);
            if (account != null)
            {
                return !_taiKhoanService.HasActiveOwnerProfile(account.Email)
                    ? (ActionResult)RedirectToAction("CompleteOwnerProfile")
                    : RedirectToAction("Index", "Owner");
            }

            var portal = _taiKhoanService.GetPortal(User.Identity.Name);
            if (portal != null)
            {
                return portal.LoaiTaiKhoan == "NhanVien"
                    ? (ActionResult)RedirectToAction("Index", "Employee")
                    : RedirectToAction("Index", "Resident");
            }

            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            var portal = _taiKhoanService.GetPortal(User.Identity.Name);
            if (portal == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            if (portal.LoaiTaiKhoan == "ChuTro")
            {
                return RedirectToAction("Index", "Owner");
            }

            if (portal.LoaiTaiKhoan != "NhanVien" &&
                portal.LoaiTaiKhoan != "CuDan")
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var portal = _taiKhoanService.GetPortal(User.Identity.Name);
            if (portal == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            if (portal.LoaiTaiKhoan == "ChuTro")
            {
                return RedirectToAction("Index", "Owner");
            }

            if (portal.LoaiTaiKhoan != "NhanVien" &&
                portal.LoaiTaiKhoan != "CuDan")
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = User.Identity.Name;

            bool result = _taiKhoanService.ChangePassword(
                username,
                model.MatKhauCu,
                model.MatKhauMoi
            );

            if (!result)
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không chính xác.");
                return View(model);
            }

            var account = _taiKhoanService.GetActiveAccount(username);
            if (account == null)
            {
                return RedirectToAction("Login");
            }

            switch (account.LoaiTaiKhoan)
            {
                case "NhanVien":
                    return RedirectToAction("Index", "Employee");

                case "CuDan":
                    return RedirectToAction("Index", "Resident");

                default:
                    FormsAuthentication.SignOut();
                    return RedirectToAction("Login");
            }
        }
    }
}
