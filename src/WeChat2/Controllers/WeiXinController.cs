using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Xml;

using WeChat2.Models;

namespace WeChat2.Controllers
{
    using Senparc.Weixin.MP;
    public class WeiXinController : Controller
    {
        string token = "garfieldzf8";

        string appID = "wx3475193134aa161e";
        string appsecret = "10c0994def4d52442a2edde4ce1843cf";


        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(string signature,string timestamp,string nonce,string echostr)
        {
            if (CheckSignature.Check(signature, timestamp, nonce, token))
            {
                return Content(echostr);
            }
            else
            {
                return Content("err");
            }
            
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Get(string signature, string timestamp, string nonce)
        {
            StreamReader sr = new StreamReader(Request.InputStream, Encoding.UTF8);
            XmlDocument doc = new XmlDocument();
            doc.Load(sr);
            sr.Close();
            sr.Dispose();

            WxMessage wxMessage = new WxMessage();
            wxMessage.ToUserName = doc.SelectSingleNode("xml").SelectSingleNode("ToUserName").InnerText;
            wxMessage.FromUserName = doc.SelectSingleNode("xml").SelectSingleNode("FromUserName").InnerText;
            wxMessage.MsgType = doc.SelectSingleNode("xml").SelectSingleNode("MsgType").InnerText;
            wxMessage.CreateTime = int.Parse(doc.SelectSingleNode("xml").SelectSingleNode("CreateTime").InnerText);

            Log(wxMessage.ToUserName + "," + wxMessage.FromUserName + "," + wxMessage.MsgType);

            if (wxMessage.MsgType == "event")
            {
                wxMessage.EventName = doc.SelectSingleNode("xml").SelectSingleNode("Event").InnerText;
                Log(wxMessage.EventName);
                if (!string.IsNullOrEmpty(wxMessage.EventName) && wxMessage.EventName == "subscribe")
                {
                    string content = "您好，欢迎访问garfieldzf8测试公众平台";
                    content = SendTextMessage(wxMessage, content);
                    Log(content);

                    return Content(content);
                }
            }


            return Content("");
        }

        private string SendTextMessage(WxMessage wxmessage,string content)
        {
            string result = string.Format(Message, wxmessage.FromUserName,wxmessage.ToUserName,DateTime.Now.Ticks, content);
            return result;
        }

        //被动回复用户消息
        public string Message
        {
            get
            {
                return @"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>{2}</CreateTime>
                            <MsgType><![CDATA[text]]></MsgType>
                            <Content><![CDATA[{3}]]></Content>
                            </xml>";
            }
        }

        private void Log(string text)
        {
            string str = Server.MapPath("~/Log/") + "log.txt";
            FileStream fs = new FileStream(str, FileMode.Append, FileAccess.Write);
            StreamWriter sr = new StreamWriter(fs);
            sr.WriteLine(DateTime.Now+" : "+text);
            sr.Close();
            fs.Close();
        }
    }
}