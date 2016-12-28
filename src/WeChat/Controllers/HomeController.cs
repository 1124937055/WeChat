using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.Mvc;


namespace WeChat.Controllers
{
    using Senparc.Weixin;
    using Senparc.Weixin.MP;
    public class HomeController : Controller
    {
        string token = "garfieldzf8";

        string appID = "wx3475193134aa161e";
        string appsecret = "10c0994def4d52442a2edde4ce1843cf";
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WeChat()
        {
            string signature = Request["signature"];
            string timestamp = Request["timestamp"];
            string nonce = Request["nonce"];
            string echoStr = Request["echostr"];

            if (Request.HttpMethod == "GET")
            {
                if(CheckSignature.Check(signature,timestamp,nonce,token))
                {
                    return Content(echoStr);
                }
                else
                {
                    return Content("failed");
                }
            }else

            return Content("");
            
        }

       

        public string GetSignature(string timestamp, string nonce, string token)
        {
            string[] arr = new[] { token, timestamp, nonce }.OrderBy(z => z).ToArray();
            string arrString = string.Join("", arr);
            System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] sha1Arr = sha1.ComputeHash(Encoding.UTF8.GetBytes(arrString));
            StringBuilder enText = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                enText.AppendFormat("{0:x2}", b);
            }
            return enText.ToString();
        }

        private void WriteContent(string msg)
        {
            Response.Output.Write(msg);
        }
    }
}