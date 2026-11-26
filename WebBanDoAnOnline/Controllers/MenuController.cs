// File: /Controllers/MenuController.cs
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class MenuController : BaseController
    {
        // GET: /Menu/Index
        public ActionResult Index(
    int? maDM,
    string sapXep)
        {
            ViewBag.CurrentPage = "Menu";
            ViewBag.Title = "Thực đơn";
            ViewBag.MaDMHienTai = maDM;
            ViewBag.SapXep = sapXep;

            var sanPhamQuery = db.SanPhams
                                 .Where(sp =>
                                     sp.isDelete != 1 &&
                                     sp.TrangThai == "Còn hàng");

            // Lọc theo danh mục
            if (maDM.HasValue)
            {
                sanPhamQuery =
                    sanPhamQuery.Where(sp =>
                        sp.MaDM == maDM.Value);
            }

            // Sắp xếp
            switch (sapXep)
            {
                case "giaTang":
                    sanPhamQuery =
                        sanPhamQuery.OrderBy(sp => sp.Gia);
                    break;

                case "giaGiam":
                    sanPhamQuery =
                        sanPhamQuery.OrderByDescending(sp => sp.Gia);
                    break;

                case "danhGia":
                    sanPhamQuery =
                        sanPhamQuery.OrderByDescending(sp => sp.DiemDanhGia);
                    break;
            }

            ViewBag.SanPhamList =
                sanPhamQuery.ToList();

            return View();
        }

        public ActionResult ChiTiet(int id)
        {
            var sanPham = db.SanPhams
                            .FirstOrDefault(sp =>
                                sp.MaSP == id &&
                                sp.isDelete != 1);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            ViewBag.DanhGia = db.DanhGias
                        .Where(x => x.MaSP == id
                                 && x.isDelete != 1)
                        .OrderByDescending(x => x.Create_at)
                        .ToList();

            // Lấy danh sách đánh giá
            var dsDanhGia = db.DanhGias
                              .Where(x => x.MaSP == id
                                       && x.isDelete != 1)
                              .OrderByDescending(x => x.Create_at)
                              .ToList();

            ViewBag.DanhGia = dsDanhGia;

            // Tính điểm trung bình
            double diemTB = 0;

            if (dsDanhGia.Count > 0)
            {
                diemTB = dsDanhGia.Average(x => (double)x.SoSao);
            }

            ViewBag.DiemTB = diemTB;

            // Tổng số đánh giá
            ViewBag.SoLuongDanhGia = dsDanhGia.Count;



            return View(sanPham);
        }

        public ActionResult TimKiem(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            var rawData = (from sp in db.SanPhams
                           join dm in db.DanhMucs on sp.MaDM equals dm.MaDM
                           where sp.isDelete != 1
                              && sp.TrangThai == "Còn hàng"
                              && sp.TenSP.Contains(q)
                           orderby sp.TenSP
                           select new
                           {
                               sp.MaSP,
                               sp.TenSP,
                               sp.Gia,
                               sp.Anh,
                               TenDM = dm.TenDM
                           })
                           .Take(6)
                           .ToList();

            var ketQua = rawData.Select(sp => new
            {
                sp.MaSP,
                sp.TenSP,
                sp.Gia,
                Anh = "/img/" + System.IO.Path.GetFileName(sp.Anh),
                sp.TenDM
            }).ToList();

            return Json(ketQua, JsonRequestBehavior.AllowGet);
        }

        public ActionResult KetQuaTimKiem(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Index");

            var ketQua = (from sp in db.SanPhams
                          where sp.isDelete != 1
                             && sp.TrangThai == "Còn hàng"
                             && sp.TenSP.Contains(q)
                          orderby sp.TenSP
                          select sp)
                          .ToList();

            ViewBag.TuKhoa = q;
            ViewBag.SoKetQua = ketQua.Count;
            return View(ketQua);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}