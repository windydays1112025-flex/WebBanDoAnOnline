using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class BaseAdminController : Controller
    {
        protected DataClasses1DataContext db = new DataClasses1DataContext(
            System.Configuration.ConfigurationManager
            .ConnectionStrings["BanDoAnOnlineConnectionString"]
            .ConnectionString);

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser = Session["TaiKhoan"] as TaiKhoan;

            // Chưa đăng nhập
            if (currentUser == null)
            {
                filterContext.Result = RedirectToRoute(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "TaiKhoan" },
                        { "action", "DangNhap" }
                    });
                return;
            }

            // Không phải Quản lý hoặc Nhân viên
            if (currentUser.VaiTro != "Quản lý" &&
                currentUser.VaiTro != "Nhân viên")
            {
                filterContext.Result = RedirectToRoute(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Index" }
                    });
                return;
            }

            // Set thông tin user cho view admin
            ViewBag.CurrentUser = currentUser;
            ViewBag.IsQuanLy = currentUser.VaiTro == "Quản lý";

            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}