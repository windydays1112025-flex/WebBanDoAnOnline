using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;
using System.IO;

namespace WebBanDoAnOnline.Controllers
{
    public class AdminDonHangController : BaseAdminController
    {
        // ============================================
        // GET: /AdminDonHang/Index
        // Danh sách toàn bộ đơn hàng
        // ============================================
        public ActionResult Index()
        {
            var dsDonHang = db.DonHangs
                              .Where(x => x.isDelete != 1)
                              .OrderByDescending(x => x.MaDH)
                              .ToList();

            return View(dsDonHang);
        }

        // ============================================
        // GET: /AdminDonHang/ChiTiet/{id}
        // Xem chi tiết 1 đơn hàng + ảnh thanh toán (nếu có)
        // ============================================
        public ActionResult ChiTiet(int id)
        {
            var chiTiet = db.ChiTietDonHangs
                    .Where(x => x.MaDH == id
                             && x.isDelete != 1)
                    .ToList();

            ViewBag.MaDH = id;

            // Tìm ảnh chuyển khoản đã upload (nếu có)
            string folder = Server.MapPath("~/Uploads/ThanhToan/");

            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder, "DH" + id + ".*");

                if (files.Length > 0)
                {
                    ViewBag.AnhThanhToan =
                        "/Uploads/ThanhToan/" +
                        Path.GetFileName(files[0]);
                }
            }

            return View(chiTiet);
        }

        // POST: /AdminDonHang/CapNhatTrangThai
        // Gọi qua AJAX từ dropdown trong Index.cshtml
        // Trả về JSON, không reload trang
        [HttpPost]
        public ActionResult CapNhatTrangThai(int id, string trangThaiMoi)
        {
            var donHang = db.DonHangs.FirstOrDefault(x => x.MaDH == id);

            if (donHang == null)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            string trangThaiCu = donHang.TrangThai;

            if (trangThaiCu == trangThaiMoi)
                return Json(new { success = false, message = "Trạng thái không thay đổi" });

            // Cập nhật trạng thái đơn hàng
            donHang.TrangThai = trangThaiMoi;
            donHang.LastEdit_at = DateTime.Now;

            // Soạn nội dung thông báo gửi khách theo từng trạng thái
            string tieuDe = "Cập nhật đơn hàng #" + id;
            string noiDung = "";

            switch (trangThaiMoi)
            {
                case "Chờ xác nhận":
                    noiDung = "Đơn hàng #" + id + " đang chờ được xác nhận.";
                    break;
                case "Đã xác nhận":
                    noiDung = "Đơn hàng #" + id + " đã được xác nhận. Chúng tôi đang chuẩn bị món ăn!";
                    break;
                case "Đang giao":
                    noiDung = "Đơn hàng #" + id + " đang trên đường giao đến bạn. Vui lòng chú ý điện thoại!";
                    break;
                case "Đã nhận hàng":
                    noiDung = "Đơn hàng #" + id + " đã giao thành công. Cảm ơn bạn đã tin tưởng FuTho Food!";
                    break;
                case "Đã hủy":
                    noiDung = "Đơn hàng #" + id + " đã bị hủy. Liên hệ hỗ trợ nếu có thắc mắc.";
                    break;
                default:
                    noiDung = "Đơn hàng #" + id + " đã được cập nhật trạng thái: " + trangThaiMoi;
                    break;
            }

            // Tạo thông báo gửi cho khách hàng đặt đơn này
            if (donHang.MaTK.HasValue)
            {
                var thongBao = new ThongBao
                {
                    MaTK = donHang.MaTK.Value,
                    Title = tieuDe,
                    Subtitle = noiDung,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                db.ThongBaos.InsertOnSubmit(thongBao);
            }

            // Lưu lại lịch sử thay đổi trạng thái (audit trail)
            var nhanVien = Session["TaiKhoan"] as TaiKhoan;

            var lichSu = new LichSuTrangThai
            {
                MaDH = id,
                TrangThaiCu = trangThaiCu,
                TrangThaiMoi = trangThaiMoi,
                ThoiGian = DateTime.Now,
                MaNhanVien = nhanVien != null ? nhanVien.MaTK : (int?)null,
                Create_at = DateTime.Now,
                isDelete = 0
            };

            db.LichSuTrangThais.InsertOnSubmit(lichSu);

            // Lưu toàn bộ thay đổi vào DB cùng 1 lần
            db.SubmitChanges();

            return Json(new
            {
                success = true,
                message = "Cập nhật thành công: " + trangThaiMoi
            });
        }
    }
}