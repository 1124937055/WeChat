using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace WeChat2.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            
            return Content("Index");
        }

        private void Log(string text)
        {
            string str = Server.MapPath("~/Log/") + "log.txt";
            FileStream fs = new FileStream(str, FileMode.Open, FileAccess.Write);
            StreamWriter sr = new StreamWriter(fs);
            sr.WriteLine(DateTime.Now + " : " + text);
            sr.Close();
            fs.Close();
        }
    }
}