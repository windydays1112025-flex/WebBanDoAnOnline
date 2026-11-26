using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class DanhGiaController : BaseController
    {
        // GET: DanhGia
        public ActionResult Index()
        {
            return View();
        }
        // Hiển thị form đánh giá
        public ActionResult Create(int maSP, int maDH)
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
            {
                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan");
            }

            ViewBag.MaSP = maSP;
            ViewBag.MaDH = maDH;

            return View();
        }

        [HttpPost]
        public ActionResult Create(
            int maSP,
            int maDH,
            int soSao,
            string binhLuan)
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
            {
                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan");
            }

            // Kiểm tra đã đánh giá chưa
            var daDanhGia = db.DanhGias.Any(x =>
                x.MaTK == user.MaTK &&
                x.MaSP == maSP &&
                x.MaDH == maDH &&
                x.isDelete != 1);

            if (daDanhGia)
            {
                TempData["Error"] =
                    "Bạn đã đánh giá sản phẩm này rồi.";

                return RedirectToAction(
                    "DonHangCuaToi",
                    "TaiKhoan");
            }

            DanhGia dg = new DanhGia();

            dg.MaTK = user.MaTK;
            dg.MaSP = maSP;
            dg.MaDH = maDH;
            dg.SoSao = soSao;
            dg.BinhLuan = binhLuan;

            dg.Create_at = DateTime.Now;
            dg.isDelete = 0;

            db.DanhGias.InsertOnSubmit(dg);

            db.SubmitChanges();

            var sp = db.SanPhams
           .FirstOrDefault(x => x.MaSP == maSP);

            if (sp != null)
            {
                sp.DiemDanhGia = db.DanhGias
                    .Where(x => x.MaSP == maSP &&
                                x.isDelete != 1)
                    .Average(x => (double)x.SoSao);

                db.SubmitChanges();
            }


            TempData["Success"] =
                "Đánh giá thành công.";

            return RedirectToAction(
                "DonHangCuaToi",
                "TaiKhoan");
        }

    }
}