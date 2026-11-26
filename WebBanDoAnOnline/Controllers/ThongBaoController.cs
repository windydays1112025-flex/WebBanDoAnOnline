using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class ThongBaoController : BaseController
    {
        // GET: /ThongBao/SoThongBaoChuaDoc
        public ActionResult SoThongBaoChuaDoc()
        {
            var user = Session["TaiKhoan"] as TaiKhoan;
            if (user == null)
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);

            int count = db.ThongBaos
                          .Count(x => x.MaTK == user.MaTK && !x.IsRead);

            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

        // GET: /ThongBao/DanhSach
        public ActionResult DanhSach()
        {
            var user = Session["TaiKhoan"] as TaiKhoan;
            if (user == null)
                return Json(new System.Collections.Generic.List<object>(),
                            JsonRequestBehavior.AllowGet);

            var ds = db.ThongBaos
                       .Where(x => x.MaTK == user.MaTK)
                       .OrderByDescending(x => x.CreatedAt)
                       .Take(10)
                       .ToList()
                       .Select(x => new
                       {
                           x.MaTB,
                           x.Title,
                           x.Subtitle,
                           x.IsRead,
                           ThoiGian = x.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                       })
                       .ToList();

            return Json(ds, JsonRequestBehavior.AllowGet);
        }

        // POST: /ThongBao/DanhDauDaDoc
        [HttpPost]
        public ActionResult DanhDauDaDoc(int id)
        {
            var user = Session["TaiKhoan"] as TaiKhoan;
            if (user == null)
                return Json(new { success = false });

            var tb = db.ThongBaos
                       .FirstOrDefault(x => x.MaTB == id
                                         && x.MaTK == user.MaTK);
            if (tb != null)
            {
                tb.IsRead = true;
                db.SubmitChanges();
            }

            return Json(new { success = true });
        }

        // POST: /ThongBao/DocTatCa
        [HttpPost]
        public ActionResult DocTatCa()
        {
            var user = Session["TaiKhoan"] as TaiKhoan;
            if (user == null)
                return Json(new { success = false });

            var dsChuaDoc = db.ThongBaos
                              .Where(x => x.MaTK == user.MaTK
                                       && !x.IsRead)
                              .ToList();

            foreach (var tb in dsChuaDoc)
                tb.IsRead = true;

            db.SubmitChanges();

            return Json(new { success = true });
        }
    }
}