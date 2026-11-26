using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class AdminDanhGiaController : BaseAdminController
    {
        // GET: AdminDanhGia
        // Hiển thị TẤT CẢ đánh giá (cả đã ẩn) để admin quản lý
        public ActionResult Index()
        {
            var ds = db.DanhGias
                       .OrderByDescending(x => x.Create_at)
                       .ToList();

            return View(ds);
        }

        public ActionResult PhanHoi(int id)
        {
            var dg = db.DanhGias.FirstOrDefault(x => x.MaDG == id);

            if (dg == null)
                return HttpNotFound();

            return View(dg);
        }

        [HttpPost]
        public ActionResult PhanHoi(int id, string phanHoi)
        {
            var dg = db.DanhGias.FirstOrDefault(x => x.MaDG == id);

            if (dg == null)
                return HttpNotFound();

            dg.PhanHoi = phanHoi;
            dg.Update_at = DateTime.Now;

            db.SubmitChanges();

            TempData["Success"] = "Phản hồi thành công";

            return RedirectToAction("Index");
        }

        // Ẩn / hiện đánh giá (gọi qua AJAX)
        [HttpPost]
        public ActionResult AnHienDanhGia(int id)
        {
            var dg = db.DanhGias.FirstOrDefault(x => x.MaDG == id);

            if (dg == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            dg.isDelete = (dg.isDelete == 1) ? (byte)0 : (byte)1;
            dg.Update_at = DateTime.Now;

            db.SubmitChanges();

            return Json(new
            {
                success = true,
                isHidden = (dg.isDelete == 1),
                message = (dg.isDelete == 1) ? "Đã ẩn đánh giá" : "Đã hiện lại đánh giá"
            });
        }

        public ActionResult XoaDanhGia(int id)
        {
            var dg = db.DanhGias.FirstOrDefault(x => x.MaDG == id);

            if (dg == null)
                return HttpNotFound();

            return View(dg);
        }

        [HttpPost]
        [ActionName("XoaDanhGia")]
        public ActionResult XacNhanXoaDanhGia(int id)
        {
            var dg = db.DanhGias.FirstOrDefault(x => x.MaDG == id);

            if (dg != null)
            {
                db.DanhGias.DeleteOnSubmit(dg);
                db.SubmitChanges();
            }

            return RedirectToAction("Index");
        }
    }
}