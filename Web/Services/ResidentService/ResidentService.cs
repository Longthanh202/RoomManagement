using System;
using Web.Models.ViewModel;
using Web.Repositories.ResidentRepository;

namespace Web.Services.ResidentService
{
    public class ResidentService : IResidentService
    {
        private readonly IResidentRepository _repository;

        public ResidentService(IResidentRepository repository)
        {
            _repository = repository;
        }

        public ResidentDashboardViewModel GetDashboard(string username)
        {
            return string.IsNullOrWhiteSpace(username)
                ? null
                : _repository.GetDashboard(username.Trim());
        }

        public string CreateRepairRequest(string username, CreateRepairRequestViewModel model)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            if (model.MucDoUuTien != "Thap" &&
                model.MucDoUuTien != "BinhThuong" &&
                model.MucDoUuTien != "Cao" &&
                model.MucDoUuTien != "KhanCap")
            {
                return "Mức độ ưu tiên không hợp lệ.";
            }

            return _repository.CreateRepairRequest(username.Trim(), model)
                ? null
                : "Bạn cần có hợp đồng cư trú đang hiệu lực để gửi yêu cầu sửa chữa.";
        }

        public string RegisterGuest(string username, RegisterGuestViewModel model)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            }

            if (!model.ThoiGianDuKienDen.HasValue ||
                !model.ThoiGianDuKienVe.HasValue ||
                model.ThoiGianDuKienDen.Value.Date < DateTime.Today ||
                model.ThoiGianDuKienVe.Value.Date < model.ThoiGianDuKienDen.Value.Date)
            {
                return "Thời gian khách đến và rời đi không hợp lệ.";
            }

            return _repository.RegisterGuest(username.Trim(), model)
                ? null
                : "Bạn cần có hợp đồng cư trú đang hiệu lực để đăng ký khách.";
        }
    }
}
