using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class AdminSanPhamController : BaseAdminController
    {

        // LIST
        public ActionResult Index(string keyword)
        {
            var ds = db.SanPhams
                       .Where(x => x.isDelete != 1);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                ds = ds.Where(x => x.TenSP.Contains(keyword));
            }

            ViewBag.Keyword = keyword;

            return View(ds.ToList());
        }

        // CREATE - GET
        public ActionResult Create()
        {
            ViewBag.MaDM = new SelectList(
                db.DanhMucs.Where(x => x.isDelete != 1),
                "MaDM",
                "TenDM"
            );

            return View();
        }

        // CREATE - POST
        [HttpPost]
        public ActionResult Create(SanPham sp, HttpPostedFileBase uploadFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MaDM = new SelectList(
                    db.DanhMucs.Where(x => x.isDelete != 1),
                    "MaDM",
                    "TenDM"
                );

                return View(sp);
            }

            // UPLOAD ẢNH
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(uploadFile.FileName).ToLower();
                string[] allow = { ".jpg", ".jpeg", ".png", ".webp" };

                if (allow.Contains(ext))
                {
                    string fileName = Guid.NewGuid().ToString() + ext;

                    string folder = Server.MapPath("~/img/");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    uploadFile.SaveAs(Path.Combine(folder, fileName));

                    sp.Anh = "~/img/" + fileName;
                }
            }

            sp.Create_at = DateTime.Now;
            sp.isDelete = 0;
            sp.DiemDanhGia = 5;

            db.SanPhams.InsertOnSubmit(sp);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        // EDIT - GET
        public ActionResult Edit(int id)
        {
            var sp = db.SanPhams.FirstOrDefault(x => x.MaSP == id);

            if (sp == null)
                return HttpNotFound();

            ViewBag.MaDM = new SelectList(
                db.DanhMucs.Where(x => x.isDelete != 1),
                "MaDM",
                "TenDM",
                sp.MaDM
            );

            return View(sp);
        }

        // EDIT - POST
        [HttpPost]
        public ActionResult Edit(SanPham model, HttpPostedFileBase uploadFile)
        {
            var sp = db.SanPhams.FirstOrDefault(x => x.MaSP == model.MaSP);

            if (sp == null)
                return HttpNotFound();

            // FIX lỗi dropdown khi validate fail
            if (!ModelState.IsValid)
            {
                ViewBag.MaDM = new SelectList(
                    db.DanhMucs.Where(x => x.isDelete != 1),
                    "MaDM",
                    "TenDM",
                    model.MaDM
                );

                return View(model);
            }

            // UPDATE DATA
            sp.TenSP = model.TenSP;
            sp.Gia = model.Gia;
            sp.MoTa = model.MoTa;
            sp.MaDM = model.MaDM;
            sp.TrangThai = model.TrangThai;
            sp.LastEdit_at = DateTime.Now;

            // UPLOAD ẢNH MỚI
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(sp.Anh))
                {
                    string oldFile = Server.MapPath(sp.Anh);

                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                }

                string ext = Path.GetExtension(uploadFile.FileName).ToLower();
                string[] allow = { ".jpg", ".jpeg", ".png", ".webp" };

                if (allow.Contains(ext))
                {
                    string fileName = Guid.NewGuid().ToString() + ext;

                    string folder = Server.MapPath("~/img/");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    uploadFile.SaveAs(Path.Combine(folder, fileName));

                    sp.Anh = "~/img/" + fileName;
                }
            }

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        // DELETE 
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var sp = db.SanPhams.FirstOrDefault(x => x.MaSP == id);

            if (sp == null)
                return HttpNotFound();

            return View(sp); // chỉ hiển thị confirm
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int MaSP)
        {
            var sp = db.SanPhams
                       .FirstOrDefault(x => x.MaSP == MaSP);

            if (sp == null)
                return HttpNotFound();

            // Kiểm tra đã có trong đơn hàng chưa
            bool daDat = db.ChiTietDonHangs
                           .Any(x => x.MaSP == MaSP);

            if (daDat)
            {
                TempData["Error"] =
                    "Sản phẩm đã có trong đơn hàng nên không thể xóa.";

                return RedirectToAction("Index");
            }

            // Xóa file ảnh
            if (!string.IsNullOrEmpty(sp.Anh))
            {
                string path = Server.MapPath(sp.Anh);

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            // Xóa sản phẩm khỏi database
            db.SanPhams.DeleteOnSubmit(sp);
            db.SubmitChanges();

            TempData["Success"] =
                "Xóa sản phẩm thành công.";

            return RedirectToAction("Index");
        }
    }
}