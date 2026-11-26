using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoAnOnline.Models;

namespace WebBanDoAnOnline.Controllers
{
    public class BaseController : Controller
    {
        protected DataClasses1DataContext db = new DataClasses1DataContext(
            System.Configuration.ConfigurationManager
            .ConnectionStrings["BanDoAnOnlineConnectionString"]
            .ConnectionString);

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.DanhMucList = db.DanhMucs
                                    .Where(x => x.isDelete != 1)
                                    .ToList();

            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}