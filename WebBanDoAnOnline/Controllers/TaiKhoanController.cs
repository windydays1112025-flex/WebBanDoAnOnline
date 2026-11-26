using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class TaiKhoanController : BaseController
    {
        

        // GET: TaiKhoan/DangNhap
        public ActionResult DangNhap()
        {
            return View();
        }

        // POST: TaiKhoan/DangNhap
        [HttpPost]
        public ActionResult DangNhap(string TenTK, string MatKhau)
        {
            var tk = db.TaiKhoans.FirstOrDefault(x =>
                x.TenTK == TenTK &&
                x.MatKhau == MatKhau);

            if (tk == null)
            {
                ViewBag.ThongBao = "Tên tài khoản hoặc mật khẩu không đúng";
                return View();
            }

            // Tài khoản bị khóa
            if (tk.isDelete == 1)
            {
                ViewBag.ThongBao = "Tài khoản đã bị khóa";
                return View();
            }

            Session["TaiKhoan"] = tk;

            // Phân quyền sau đăng nhập
            if (tk.VaiTro == "Quản lý" || tk.VaiTro == "Nhân viên")
            {
                return RedirectToAction("Index", "Admin");
            }

            // Khách hàng → kiểm tra returnUrl
            string returnUrl = Request.QueryString["returnUrl"];
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("DangNhap");
        }

        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(
            string HoTen,
            string SoDienThoai,
            string Email,
            string TenTK,
            string MatKhau)
        {
            var checkTK = db.TaiKhoans
                            .FirstOrDefault(x => x.TenTK == TenTK);

            if (checkTK != null)
            {
                ViewBag.Message = "Tên tài khoản đã tồn tại";
                return View();
            }

            TaiKhoan tk = new TaiKhoan();

            tk.HoTen = HoTen;
            tk.SoDienThoai = SoDienThoai;
            tk.Email = Email;
            tk.TenTK = TenTK;
            tk.MatKhau = MatKhau;

            tk.VaiTro = "Khách hàng";
            tk.Create_at = DateTime.Now;
            tk.isDelete = 0;

            db.TaiKhoans.InsertOnSubmit(tk);
            db.SubmitChanges();

            return RedirectToAction("DangNhap");
        }
        public ActionResult DonHangCuaToi()
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
            {
                return RedirectToAction("DangNhap");
            }

            var dsDonHang = db.DonHangs
                              .Where(x => x.MaTK == user.MaTK &&
                                          x.isDelete != 1)
                              .OrderByDescending(x => x.MaDH)
                              .ToList();

            return View(dsDonHang);
        }
        public ActionResult ChiTietDonHang(int id)
        {
            if (id == null)
                return RedirectToAction("DonHangCuaToi");

            var chiTiet = db.ChiTietDonHangs
                            .Where(x => x.MaDH == id &&
                                        x.isDelete != 1)
                            .ToList();

            return View(chiTiet);
        }
        public ActionResult HuyDonHang(int id)
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
            {
                return RedirectToAction("DangNhap");
            }

            var donHang = db.DonHangs.FirstOrDefault(x =>
                x.MaDH == id &&
                x.MaTK == user.MaTK);

            if (donHang == null)
            {
                return RedirectToAction("DonHangCuaToi");
            }

            if (donHang.TrangThai == "Chờ xác nhận")
            {
                donHang.TrangThai = "Đã hủy";

                db.SubmitChanges();

                TempData["Success"] = "Hủy đơn hàng thành công";
            }

            return RedirectToAction("DonHangCuaToi");
        }
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        // POST: Xác minh email và đổi mật khẩu
        [HttpPost]
        public ActionResult QuenMatKhau(string Email, string SoDienThoai,
                                string MatKhauMoi, string XacNhanMatKhau)
        {
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(SoDienThoai) ||
                string.IsNullOrWhiteSpace(MatKhauMoi) ||
                string.IsNullOrWhiteSpace(XacNhanMatKhau))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            if (MatKhauMoi != XacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            if (MatKhauMoi.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
                return View();
            }

            // Phải khớp cả Email lẫn SĐT mới cho đổi
            var tk = db.TaiKhoans.FirstOrDefault(x =>
                x.Email == Email &&
                x.SoDienThoai == SoDienThoai &&
                x.isDelete != 1);

            if (tk == null)
            {
                ViewBag.Error = "Email hoặc số điện thoại không đúng.";
                return View();
            }

            tk.MatKhau = MatKhauMoi;
            tk.LastEdit_at = DateTime.Now;
            db.SubmitChanges();

            TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
            return RedirectToAction("DangNhap");
        }
        public ActionResult ThongTinCaNhan()
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
            {
                return RedirectToAction("DangNhap");
            }

            var tk = db.TaiKhoans
                       .FirstOrDefault(x => x.MaTK == user.MaTK);

            return View(tk);
        }
        public ActionResult SuaThongTin()
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
                return RedirectToAction("DangNhap");

            var tk = db.TaiKhoans
                       .FirstOrDefault(x => x.MaTK == user.MaTK);

            return View(tk);
        }

        [HttpPost]
        public ActionResult SuaThongTin(
            string HoTen,
            string Email,
            string SoDienThoai)
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            if (user == null)
                return RedirectToAction("DangNhap");

            var tk = db.TaiKhoans
                       .FirstOrDefault(x => x.MaTK == user.MaTK);

            tk.HoTen = HoTen;
            tk.Email = Email;
            tk.SoDienThoai = SoDienThoai;
            tk.LastEdit_at = DateTime.Now;

            db.SubmitChanges();

            Session["TaiKhoan"] = tk;

            TempData["Success"] = "Cập nhật thành công";

            return RedirectToAction("ThongTinCaNhan");
        }
        public ActionResult DoiMatKhau()
        {
            if (Session["TaiKhoan"] == null)
                return RedirectToAction("DangNhap");

            return View();
        }

        [HttpPost]
        public ActionResult DoiMatKhau(
            string MatKhauCu,
            string MatKhauMoi,
            string XacNhanMatKhau)
        {
            var user = Session["TaiKhoan"] as TaiKhoan;

            var tk = db.TaiKhoans
                       .FirstOrDefault(x => x.MaTK == user.MaTK);

            if (tk.MatKhau != MatKhauCu)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng";
                return View();
            }

            if (MatKhauMoi != XacNhanMatKhau)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp";
                return View();
            }

            tk.MatKhau = MatKhauMoi;
            tk.LastEdit_at = DateTime.Now;

            db.SubmitChanges();

            TempData["Success"] = "Đổi mật khẩu thành công";

            return RedirectToAction("ThongTinCaNhan");
        }
    }
}