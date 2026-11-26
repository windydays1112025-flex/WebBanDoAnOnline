using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    // ViewModel cho sản phẩm bán chạy
    public class SanPhamBanChayViewModel
    {
        public SanPham SanPham { get; set; }
        public int? TongSoLuong { get; set; }
    }

    public class AdminController : BaseAdminController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";

            // Tổng doanh thu (đơn đã nhận hàng)
            ViewBag.TongDoanhThu = db.DonHangs
                .Where(x => x.isDelete != 1 && x.TrangThai == "Đã nhận hàng")
                .Sum(x => (decimal?)x.TongTien) ?? 0;

            // Tổng đơn hàng
            ViewBag.TongDonHang = db.DonHangs
                .Count(x => x.isDelete != 1);

            // Đơn chờ xác nhận
            ViewBag.DonChoXacNhan = db.DonHangs
                .Count(x => x.isDelete != 1 && x.TrangThai == "Chờ xác nhận");

            // Đơn đang giao
            ViewBag.DonDangGiao = db.DonHangs
                .Count(x => x.isDelete != 1 && x.TrangThai == "Đang giao");

            // Tổng sản phẩm
            ViewBag.TongSanPham = db.SanPhams
                .Count(x => x.isDelete != 1);

            // Tổng khách hàng
            ViewBag.TongKhachHang = db.TaiKhoans
                .Count(x => x.isDelete != 1 && x.VaiTro == "Khách hàng");

            // 5 đơn hàng mới nhất
            ViewBag.DonHangMoiNhat = db.DonHangs
                .Where(x => x.isDelete != 1)
                .OrderByDescending(x => x.MaDH)
                .Take(5)
                .ToList();

            // 5 sản phẩm bán chạy nhất
            var topSP = db.ChiTietDonHangs
                          .Where(x => x.isDelete != 1)
                          .GroupBy(x => x.MaSP)
                          .Select(g => new
                          {
                              MaSP = g.Key,
                              TongSoLuong = g.Sum(x => x.SoLuong)
                          })
                          .OrderByDescending(x => x.TongSoLuong)
                          .Take(5)
                          .ToList();

            var sanPhamBanChay = new List<SanPhamBanChayViewModel>();

            foreach (var item in topSP)
            {
                var sp = db.SanPhams.FirstOrDefault(x => x.MaSP == item.MaSP);
                if (sp != null)
                {
                    sanPhamBanChay.Add(new SanPhamBanChayViewModel
                    {
                        SanPham = sp,
                        TongSoLuong = item.TongSoLuong
                    });
                }
            }

            ViewBag.SanPhamBanChay = sanPhamBanChay;

            return View();
        }
    }
}