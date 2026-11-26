using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;


namespace WebBanDoAnOnline.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var monNoiBat = db.SanPhams
            .Where(sp => sp.isDelete != 1)
            .ToList()
            .Select(sp => new
            {
                SanPham = sp,

                SoLuongBan = sp.ChiTietDonHangs
                    .Where(ct =>
                        ct.isDelete != 1 &&
                        ct.DonHang.TrangThai == "Đã nhận hàng")
                    .Sum(ct => ct.SoLuong ?? 0),

                DiemDanhGia = sp.DiemDanhGia ?? 0
            })
            .OrderByDescending(x => x.SoLuongBan)
            .ThenByDescending(x => x.DiemDanhGia)
            .Take(4)
            .Select(x => x.SanPham)
            .ToList();

            ViewBag.MonNoiBat = monNoiBat;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult TestLogin()
        {
            if (Session["TaiKhoan"] == null)
            {
                return Content("Chua dang nhap");
            }

            var tk = (TaiKhoan)Session["TaiKhoan"];

            return Content("Xin chao " + tk.HoTen);
        }
    }
}