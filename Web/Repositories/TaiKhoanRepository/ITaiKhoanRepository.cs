using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Models;
using Web.Models.ViewModel;

namespace Web.Repositories.TaiKhoanRepository
{
    public interface ITaiKhoanRepository
    {
        bool IsUsernameExists(string username);

        bool IsEmailExists(string email);

        string CreateOwner(TaiKhoan taiKhoan, ChuTro chuTro);

        TaiKhoan GetActiveOwnerAccount(string username);

        TaiKhoan GetActiveAccount(string username);

        PortalDashboardViewModel GetPortal(string username);

        bool HasActiveOwnerProfile(string email);

        bool IsEmailUsedByAnotherAccount(long accountId, string email);

        bool IsOwnerEmailUsed(string email);

        void CompleteOwnerProfile(TaiKhoan account, ChuTro chuTro);
    }
}
