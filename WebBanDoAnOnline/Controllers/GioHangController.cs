using System;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Controllers;
using WebBanDoAnOnline.Models;
using System.Linq;

public class GioHangController : BaseController
{
    // Tạo db riêng cho GioHangController
    private DataClasses1DataContext _db = new DataClasses1DataContext(
        System.Configuration.ConfigurationManager
        .ConnectionStrings["BanDoAnOnlineConnectionString"]
        .ConnectionString);

    public ActionResult ThemGioHang(int id)
    {
        var user = Session["TaiKhoan"] as TaiKhoan;
        if (user == null)
            return RedirectToAction("DangNhap", "TaiKhoan");

        var gioHang = _db.GioHangs.FirstOrDefault(x =>
            x.MaTK == user.MaTK &&
            x.MaSP == id);

        if (gioHang == null)
        {
            GioHang gh = new GioHang();
            gh.MaTK = user.MaTK;
            gh.MaSP = id;
            gh.SoLuong = 1;
            gh.isDelete = 0;
            gh.Create_at = DateTime.Now;
            _db.GioHangs.InsertOnSubmit(gh);
        }
        else
        {
            gioHang.SoLuong += 1;
        }

        _db.SubmitChanges();
        return RedirectToAction("Index", "Menu");
    }

    public ActionResult Index()
    {
        var user = Session["TaiKhoan"] as TaiKhoan;
        if (user == null)
            return RedirectToAction("DangNhap", "TaiKhoan");

        var gioHang = _db.GioHangs
                         .Where(x => x.MaTK == user.MaTK &&
                                     x.isDelete != 1)
                         .ToList();

        return View(gioHang);
    }

    public ActionResult TangSoLuong(int id)
    {
        var item = _db.GioHangs.FirstOrDefault(x => x.MaGio == id);
        if (item != null)
        {
            item.SoLuong += 1;
            _db.SubmitChanges();
        }
        return RedirectToAction("Index");
    }

    public ActionResult GiamSoLuong(int id)
    {
        var item = _db.GioHangs.FirstOrDefault(x => x.MaGio == id);
        if (item != null)
        {
            if (item.SoLuong > 1)
                item.SoLuong -= 1;
            else
                _db.GioHangs.DeleteOnSubmit(item);

            _db.SubmitChanges();
        }
        return RedirectToAction("Index");
    }

    public ActionResult Xoa(int id)
    {
        var item = _db.GioHangs.FirstOrDefault(x => x.MaGio == id);
        if (item != null)
        {
            _db.GioHangs.DeleteOnSubmit(item);
            _db.SubmitChanges();
        }
        return RedirectToAction("Index");
    }

    public ActionResult DatHang()
    {
        var user = Session["TaiKhoan"] as TaiKhoan;
        if (user == null)
            return RedirectToAction("DangNhap", "TaiKhoan");

        var dsDiaChi = _db.DiaChis
                          .Where(x => x.MaTK == user.MaTK
                                   && x.isDelete != 1)
                          .ToList();

        return View(dsDiaChi);
    }

    [HttpPost]
    public ActionResult DatHang(int MaDiaChi, string phuongThuc, HttpPostedFileBase AnhThanhToan)
    {
        var user = Session["TaiKhoan"] as TaiKhoan;
        if (user == null)
            return RedirectToAction("DangNhap", "TaiKhoan");

        var gioHang = _db.GioHangs
                         .Where(x => x.MaTK == user.MaTK
                                  && x.isDelete != 1)
                         .ToList();

        if (gioHang.Count == 0)
            return RedirectToAction("Index");

        decimal tongTien = 0;
        foreach (var item in gioHang)
            tongTien += item.SanPham.Gia.Value * item.SoLuong.Value;

        DonHang dh = new DonHang();
        dh.MaTK = user.MaTK;
        dh.MaDiaChi = MaDiaChi;
        dh.PhuongThucTT = phuongThuc;
        dh.MaVoucher = null;
        dh.TongTien = tongTien;
        dh.TrangThai = "Chờ xác nhận";
        dh.Create_at = DateTime.Now;
        dh.isDelete = 0;

        _db.DonHangs.InsertOnSubmit(dh);
        _db.SubmitChanges();

        if (phuongThuc == "Chuyển khoản" && AnhThanhToan != null)
        {
            string folder = Server.MapPath("~/Uploads/ThanhToan/");
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            string ext = System.IO.Path.GetExtension(AnhThanhToan.FileName);
            string fileName = "DH" + dh.MaDH + ext;
            AnhThanhToan.SaveAs(System.IO.Path.Combine(folder, fileName));
        }

        foreach (var item in gioHang)
        {
            ChiTietDonHang ct = new ChiTietDonHang();
            ct.MaDH = dh.MaDH;
            ct.MaSP = item.MaSP.Value;
            ct.SoLuong = item.SoLuong.Value;
            ct.DonGia = item.SanPham.Gia.Value;
            ct.Create_at = DateTime.Now;
            ct.isDelete = 0;
            _db.ChiTietDonHangs.InsertOnSubmit(ct);

            // Xóa hẳn thay vì xóa mềm
            _db.GioHangs.DeleteOnSubmit(item);
        }

        _db.SubmitChanges();

        TempData["Success"] = "Đặt hàng thành công!";
        return RedirectToAction("DonHangCuaToi", "TaiKhoan");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _db.Dispose();
        base.Dispose(disposing);
    }
}