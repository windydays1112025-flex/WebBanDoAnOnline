using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class AdminTaiKhoanController : BaseAdminController
    {


        // DANH SÁCH
        public ActionResult Index(string keyword)
        {
            var ds = db.TaiKhoans.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                ds = ds.Where(x =>
                    x.HoTen.Contains(keyword) ||
                    x.Email.Contains(keyword) ||
                    x.TenTK.Contains(keyword) ||
                    x.VaiTro.Contains(keyword));
            }

            ViewBag.Keyword = keyword;

            return View(ds.ToList());
        }

        // BAN
        public ActionResult Ban(int id)
        {
            var tk = db.TaiKhoans.FirstOrDefault(x => x.MaTK == id);

            if (tk == null)
                return HttpNotFound();

            var currentUser = Session["TaiKhoan"] as TaiKhoan;

            // Không tự khóa chính mình
            if (currentUser.MaTK == tk.MaTK)
                return new HttpStatusCodeResult(403);

            // Nhân viên chỉ được khóa khách hàng
            if (currentUser.VaiTro == "Nhân viên" &&
                tk.VaiTro != "Khách hàng")
            {
                return new HttpStatusCodeResult(403);
            }

            // Quản lý thường không được khóa quản lý
            if (currentUser.VaiTro == "Quản lý" &&
                currentUser.TenTK != "admin" &&
                tk.VaiTro == "Quản lý")
            {
                return new HttpStatusCodeResult(403);
            }

            tk.isDelete = 1;
            tk.LastEdit_at = DateTime.Now;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        // UNBAN
        public ActionResult Unban(int id)
        {
            var tk = db.TaiKhoans.FirstOrDefault(x => x.MaTK == id);

            if (tk == null)
                return HttpNotFound();

            var currentUser = Session["TaiKhoan"] as TaiKhoan;

            // Nhân viên chỉ được mở khóa khách hàng
            if (currentUser.VaiTro == "Nhân viên" &&
                tk.VaiTro != "Khách hàng")
            {
                return new HttpStatusCodeResult(403);
            }

            // Quản lý thường không được mở khóa quản lý
            if (currentUser.VaiTro == "Quản lý" &&
                currentUser.TenTK != "admin" &&
                tk.VaiTro == "Quản lý")
            {
                return new HttpStatusCodeResult(403);
            }

            tk.isDelete = 0;
            tk.LastEdit_at = DateTime.Now;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        // XÓA tk
        public ActionResult Delete(int id)
        {
            var tk = db.TaiKhoans.FirstOrDefault(x => x.MaTK == id);

            if (tk == null)
                return HttpNotFound();

            var currentUser = Session["TaiKhoan"] as TaiKhoan;

            // Không tự xóa chính mình
            if (currentUser.MaTK == tk.MaTK)
                return new HttpStatusCodeResult(403);

            // Nhân viên chỉ được xóa khách hàng
            if (currentUser.VaiTro == "Nhân viên" &&
                tk.VaiTro != "Khách hàng")
            {
                return new HttpStatusCodeResult(403);
            }

            // Quản lý thường không được xóa quản lý
            if (currentUser.VaiTro == "Quản lý" &&
                currentUser.TenTK != "admin" &&
                tk.VaiTro == "Quản lý")
            {
                return new HttpStatusCodeResult(403);
            }

            db.TaiKhoans.DeleteOnSubmit(tk);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }
        //THÊM nhân viên
        public ActionResult CreateNhanVien()
        {
            var currentUser = Session["TaiKhoan"] as TaiKhoan;

            if (currentUser == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            if (currentUser.VaiTro != "Quản lý")
                return new HttpStatusCodeResult(403);

            return View();
        }

        [HttpPost]
        public ActionResult CreateNhanVien(TaiKhoan tk)
        {
            var currentUser = Session["TaiKhoan"] as TaiKhoan;

if (currentUser == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            if (string.IsNullOrWhiteSpace(tk.HoTen) ||
                string.IsNullOrWhiteSpace(tk.SoDienThoai) ||
                string.IsNullOrWhiteSpace(tk.Email) ||
                string.IsNullOrWhiteSpace(tk.TenTK) ||
                string.IsNullOrWhiteSpace(tk.MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View(tk);
            }

            if (db.TaiKhoans.Any(x => x.SoDienThoai == tk.SoDienThoai))
            {
                ViewBag.Error = "Số điện thoại đã tồn tại";
                return View(tk);
            }

            if (db.TaiKhoans.Any(x => x.Email == tk.Email))
            {
                ViewBag.Error = "Email đã tồn tại";
                return View(tk);
            }

            if (db.TaiKhoans.Any(x => x.TenTK == tk.TenTK))
            {
                ViewBag.Error = "Tên tài khoản đã tồn tại";
                return View(tk);
            }

            tk.VaiTro = "Nhân viên";
            tk.Create_at = DateTime.Now;
            tk.isDelete = 0;

            db.TaiKhoans.InsertOnSubmit(tk);
            db.SubmitChanges();

            TempData["Success"] = "Thêm nhân viên thành công!";
            return RedirectToAction("Index");

}


        //Thêm quản lý
        public ActionResult CreateQuanLy()
        {
            var currentUser = Session["TaiKhoan"] as TaiKhoan;

            if (currentUser == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            if (currentUser.TenTK != "admin")
                return new HttpStatusCodeResult(403);

            return View();
        }

        [HttpPost]
        public ActionResult CreateQuanLy(TaiKhoan tk)
        {
            var currentUser = Session["TaiKhoan"] as TaiKhoan;

if (currentUser == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            if (currentUser.TenTK != "admin")
                return new HttpStatusCodeResult(403);

            // Kiểm tra tên tài khoản
            if (db.TaiKhoans.Any(x => x.TenTK == tk.TenTK))
            {
                ViewBag.Error = "Tên tài khoản đã tồn tại";
                return View(tk);
            }

            // Kiểm tra Email
            if (db.TaiKhoans.Any(x => x.Email == tk.Email))
            {
                ViewBag.Error = "Email đã tồn tại";
                return View(tk);
            }

            // Kiểm tra SĐT
            if (db.TaiKhoans.Any(x => x.SoDienThoai == tk.SoDienThoai))
            {
                ViewBag.Error = "Số điện thoại đã tồn tại";
                return View(tk);
            }

            // Kiểm tra dữ liệu rỗng
            if (string.IsNullOrWhiteSpace(tk.HoTen) ||
                string.IsNullOrWhiteSpace(tk.SoDienThoai) ||
                string.IsNullOrWhiteSpace(tk.Email) ||
                string.IsNullOrWhiteSpace(tk.TenTK) ||
                string.IsNullOrWhiteSpace(tk.MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View(tk);
            }

            tk.VaiTro = "Quản lý";
            tk.Create_at = DateTime.Now;
            tk.isDelete = 0;

            db.TaiKhoans.InsertOnSubmit(tk);
            db.SubmitChanges();

            TempData["Success"] = "Thêm quản lý thành công!";
            return RedirectToAction("Index");

}

    }
}