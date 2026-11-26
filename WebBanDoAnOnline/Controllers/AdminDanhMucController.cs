using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;
using System.IO;

namespace WebBanDoAnOnline.Controllers
{
    public class AdminDanhMucController : BaseAdminController
    {


        // DANH SÁCH
        public ActionResult Index(string keyword)
        {
            var ds = db.DanhMucs
                       .Where(x => x.isDelete != 1);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                ds = ds.Where(x => x.TenDM.Contains(keyword));
            }

            ViewBag.Keyword = keyword;

            return View(ds.ToList());
        }

        // THÊM DANH MỤC

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(
        DanhMuc dm,
        HttpPostedFileBase imageFile)
        {
            // Kiểm tra tên danh mục
            if (string.IsNullOrWhiteSpace(dm.TenDM))
            {
                ViewBag.Error = "Tên danh mục không được để trống";
                return View(dm);
            }

            // Kiểm tra trùng tên
            if (db.DanhMucs.Any(x =>
                x.TenDM.Trim().ToLower() ==
                dm.TenDM.Trim().ToLower()))
            {
                ViewBag.Error = "Danh mục đã tồn tại";
                return View(dm);
            }

            // Upload ảnh
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string fileName =
                    Path.GetFileName(imageFile.FileName);

                string path =
                    Server.MapPath("~/img/" + fileName);

                imageFile.SaveAs(path);

                dm.Anh = "~/img/" + fileName;
            }

            dm.Create_at = DateTime.Now;
            dm.isDelete = 0;

            db.DanhMucs.InsertOnSubmit(dm);
            db.SubmitChanges();

            TempData["Success"] = "Thêm danh mục thành công";

            return RedirectToAction("Index");

        }

        // SỬA DANH MỤC

        public ActionResult Edit(int id)
        {
            var dm = db.DanhMucs
                       .FirstOrDefault(x => x.MaDM == id);

            if (dm == null)
                return HttpNotFound();

            return View(dm);
        }

        [HttpPost]
        public ActionResult Edit(DanhMuc model, HttpPostedFileBase imageFile){
            var dm = db.DanhMucs
            .FirstOrDefault(x => x.MaDM == model.MaDM);

            if (dm == null)
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(model.TenDM))
            {
                ViewBag.Error = "Tên danh mục không được để trống";
                return View(model);
            }

            bool trungTen = db.DanhMucs.Any(x =>
                x.MaDM != model.MaDM &&
                x.TenDM == model.TenDM &&
                x.isDelete != 1);

            if (trungTen)
            {
                ViewBag.Error = "Tên danh mục đã tồn tại";
                return View(model);
            }

            dm.TenDM = model.TenDM;

            // Nếu chọn ảnh mới
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(dm.Anh))
                {
                    string oldPath = Server.MapPath(dm.Anh);

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                string fileName = System.IO.Path.GetFileName(imageFile.FileName);

                string newPath = Server.MapPath("~/img/" + fileName);

                imageFile.SaveAs(newPath);

                dm.Anh = "~/img/" + fileName;
            }

            dm.LastEdit_at = DateTime.Now;

            db.SubmitChanges();

            TempData["Success"] =
                "Cập nhật danh mục thành công";

            return RedirectToAction("Index");

        }


        // XÓA 

        public ActionResult Delete(int id)
        {
            var dm = db.DanhMucs
                       .FirstOrDefault(x => x.MaDM == id);

            if (dm == null)
                return HttpNotFound();

            db.DanhMucs.DeleteOnSubmit(dm);

            db.SubmitChanges();

            TempData["Success"] = "Đã xóa danh mục thành công";

            return RedirectToAction("Index");
        }

    }
}