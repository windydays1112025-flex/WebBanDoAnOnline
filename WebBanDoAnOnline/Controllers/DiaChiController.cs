using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class DiaChiController : BaseController
    {
        

        public ActionResult Index()
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var dsDiaChi = db.DiaChis
                             .Where(x => x.MaTK == user.MaTK
                                      && x.isDelete != 1)
                             .ToList();

            return View(dsDiaChi);
        }
        //thêm địa chỉ
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(
        string TenNguoiNhan,
        string SDTNhan,
        string DiaChiCuThe)
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            DiaChi dc = new DiaChi();

            dc.MaTK = user.MaTK;
            dc.TenNguoiNhan = TenNguoiNhan;
            dc.SDTNhan = SDTNhan;
            dc.DiaChiCuThe = DiaChiCuThe;

            dc.MacDinh = false;
            dc.Create_at = DateTime.Now;
            dc.isDelete = 0;

            db.DiaChis.InsertOnSubmit(dc);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}