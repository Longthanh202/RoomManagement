using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
                name: "RegisterOwner",
                url: "register",
                defaults: new
                {
                    controller = "Account",
                    action = "RegisterOwner"
                });

            routes.MapRoute(
                name: "Login",
                url: "login",
                defaults: new
                {
                    controller = "Account",
                    action = "Login"
                }
            );

            routes.MapRoute(
                name: "Logout",
                url: "logout",
                defaults: new
                {
                    controller = "Account",
                    action = "Logout"
                });

            routes.MapRoute(
                name: "MemberPortal",
                url: "portal",
                defaults: new
                {
                    controller = "Account",
                    action = "Portal"
                });

            routes.MapRoute(
                name: "MemberNotificationRead",
                url: "portal/thong-bao/read/{id}",
                defaults: new
                {
                    controller = "Account",
                    action = "MarkMemberNotificationRead"
                });

            routes.MapRoute(
                name: "ResidentDashboard",
                url: "cu-dan",
                defaults: new
                {
                    controller = "Resident",
                    action = "Index"
                });

            routes.MapRoute(
                name: "ResidentCreateRepair",
                url: "cu-dan/yeu-cau-sua-chua",
                defaults: new
                {
                    controller = "Resident",
                    action = "CreateRepairRequest"
                });

            routes.MapRoute(
                name: "ResidentRegisterGuest",
                url: "cu-dan/dang-ky-khach",
                defaults: new
                {
                    controller = "Resident",
                    action = "RegisterGuest"
                });

            routes.MapRoute(
                name: "EmployeeDashboard",
                url: "nhan-vien",
                defaults: new
                {
                    controller = "Employee",
                    action = "Index"
                });

            routes.MapRoute(
                name: "EmployeeUpdateRepair",
                url: "nhan-vien/yeu-cau-sua-chua/cap-nhat",
                defaults: new
                {
                    controller = "Employee",
                    action = "UpdateRepairStatus"
                });

            routes.MapRoute(
                name: "EmployeeUpdateGuest",
                url: "nhan-vien/dang-ky-khach/cap-nhat",
                defaults: new
                {
                    controller = "Employee",
                    action = "UpdateGuestStatus"
                });

            routes.MapRoute(
                name: "CompleteOwnerProfile",
                url: "owner/profile/complete",
                defaults: new
                {
                    controller = "Account",
                    action = "CompleteOwnerProfile"
                });

            routes.MapRoute(
                name: "OwnerCreateKhuTro",
                url: "owner/khu-tro/create",
                defaults: new
                {
                    controller = "Owner",
                    action = "CreateKhuTro"
                });

            routes.MapRoute(
                name: "OwnerCreateDayTro",
                url: "owner/day-tro/create",
                defaults: new
                {
                    controller = "Owner",
                    action = "CreateDayTro"
                });

            routes.MapRoute(
                name: "OwnerCreateFloor",
                url: "owner/tang/create",
                defaults: new
                {
                    controller = "Owner",
                    action = "CreateFloor"
                });

            routes.MapRoute(
                name: "OwnerCreateRoom",
                url: "owner/phong/create",
                defaults: new
                {
                    controller = "Owner",
                    action = "CreateRoom"
                });

            routes.MapRoute(
                name: "OwnerCreateEmployee",
                url: "owner/nhan-vien/create",
                defaults: new
                {
                    controller = "Owner",
                    action = "CreateEmployee"
                });

            routes.MapRoute(
                name: "OwnerCreateResident",
                url: "owner/cu-dan/create",
                defaults: new
                {
                    controller = "Owner",
                    action = "CreateResident"
                });

            routes.MapRoute(
                name: "OwnerAssignEmployee",
                url: "owner/nhan-vien/assign",
                defaults: new
                {
                    controller = "Owner",
                    action = "AssignEmployee"
                });

            routes.MapRoute(
                name: "OwnerAddRoomMember",
                url: "owner/hop-dong/thanh-vien/add",
                defaults: new
                {
                    controller = "Owner",
                    action = "AddRoomMember"
                });

            routes.MapRoute(
                name: "OwnerEndRoomMember",
                url: "owner/hop-dong/thanh-vien/end",
                defaults: new
                {
                    controller = "Owner",
                    action = "EndRoomMember"
                });

            routes.MapRoute(
                name: "OwnerUpdateContract",
                url: "owner/hop-dong/status",
                defaults: new
                {
                    controller = "Owner",
                    action = "UpdateContract"
                });

            routes.MapRoute(
                name: "OwnerCreateService",
                url: "owner/dich-vu/create",
                defaults: new
                {
                    controller = "Owner",
                    action = "CreateService"
                });

            routes.MapRoute(
                name: "OwnerSetServicePrice",
                url: "owner/dich-vu/pricing",
                defaults: new
                {
                    controller = "Owner",
                    action = "SetServicePrice"
                });

            routes.MapRoute(
                name: "OwnerMarkNotificationRead",
                url: "owner/thong-bao/read/{id}",
                defaults: new
                {
                    controller = "Owner",
                    action = "MarkNotificationRead"
                });

            routes.MapRoute(
                name: "OwnerDashboard",
                url: "owner",
                defaults: new
                {
                    controller = "Owner",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
