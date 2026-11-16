using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Models;
using Web.Models.ViewModel;

namespace Web.Services.TaiKhoanService
{
    public interface ITaiKhoanService
    {
        string RegisterOwner(RegisterOwnerViewModel model);

        TaiKhoan AuthenticateOwner(string username, string password);

        TaiKhoan AuthenticateAccount(string username, string password);

        PortalDashboardViewModel GetPortal(string username);

        TaiKhoan GetActiveOwnerAccount(string username);

        bool HasActiveOwnerProfile(string email);

        string CompleteOwnerProfile(string username, CompleteOwnerProfileViewModel model);

        bool MarkNotificationRead(string username, long recipientId);
    }
}
