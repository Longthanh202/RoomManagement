using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Web.Repositories.OwnerRepository;
using Web.Repositories.TaiKhoanRepository;
using Web.Services.OwnerService;
using Web.Services.TaiKhoanService;

namespace Web.App_Start
{
    public class AutofacConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // Đăng ký tất cả Controller
            builder.RegisterControllers(
                Assembly.GetExecutingAssembly()
            );

            // Đăng ký DbContext
            builder.RegisterType<QuanLyPhongTroEntities>()
                .AsSelf()
                .InstancePerRequest();

            // Đăng ký Repository
            builder.RegisterType<TaiKhoanRepository>()
                .As<ITaiKhoanRepository>()
                .InstancePerRequest();

            builder.RegisterType<OwnerRepository>()
                .As<IOwnerRepository>()
                .InstancePerRequest();

            // Đăng ký Service
            builder.RegisterType<TaiKhoanService>()
                .As<ITaiKhoanService>()
                .InstancePerRequest();

            builder.RegisterType<OwnerService>()
                .As<IOwnerService>()
                .InstancePerRequest();

            // Build Container
            var container = builder.Build();

            // Đăng ký Dependency Resolver cho MVC
            DependencyResolver.SetResolver(
                new AutofacDependencyResolver(container)
            );
        }
    }
}