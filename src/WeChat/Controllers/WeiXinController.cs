using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using WeChat.Models;

namespace WeChat.Controllers
{
    using Senparc.Weixin;
    using Senparc.Weixin.MP;
    public class WeiXinController : Controller
    {

        string token = "garfieldzf8";

        string appID = "wx3475193134aa161e";
        string appsecret = "10c0994def4d52442a2edde4ce1843cf";

        readonly Func<string> _getRandomFileName = () => DateTime.Now.ToString("yyyyMMdd-HHmmss") + Guid.NewGuid().ToString("n").Substring(0, 6);

        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(string signature,string timestamp,string nonce,string echostr)
        {
            if (CheckSignature.Check(signature, timestamp, nonce, token))
            {
                return Content("echostr:"+echostr);
            }
            else
            {
                return Content("err");
            }
            
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(string signature, string timestamp, string nonce,string echostr)
        {
            if (!CheckSignature.Check(signature, timestamp, nonce, token))
            {
                return Content("参数错误");
            }

            var logPath = Server.MapPath(string.Format("~/log/MP/{0}/", DateTime.Now.ToString("yyyy-MM-dd")));

            try
            {
                
                var messageHandler = new CustomMessageHandler(Request.InputStream);

                messageHandler.RequestDocument.Save(Path.Combine(logPath, string.Format("{0}_Request_{1}.txt", _getRandomFileName(), messageHandler.RequestMessage.FromUserName)));

                messageHandler.Execute();

                return Content(messageHandler.ResponseDocument.ToString());
            }
            catch (Exception ex)
            {
                return Content("err:");
            }
            
        }
    }
}