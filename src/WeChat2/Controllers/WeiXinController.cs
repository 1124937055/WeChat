using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Xml;
using System.Net;
using Newtonsoft.Json;

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
                    string content = "您好，欢迎访问garfieldzf8测试公众平台\n";
                    content+="<a href='" +Request.Url.Host+Url.Action("OAuthSnsApiBase") +"'>SnsApiBase</a>\n";
                    content += "<a href='" +Request.Url.Host+Url.Action("OAuthSnsApiUserInfo") + "'>SnsApiUserInfo</a>";
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

        /**
         * snsapi_base
         * **/
        public ActionResult OAuthSnsApiBase()
        {
            string code = Request.QueryString["code"];
            try
            {
                if (!string.IsNullOrEmpty(code))
                {
                   
                    OAuthToken oauthToken = HttpUtility.Get<OAuthToken>(string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", appID, appsecret, code));

                    string accesstoken = string.Empty;
                    AccessToken token = HttpUtility.Get<AccessToken>(string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}",appID,appsecret));

                    if (token != null && !string.IsNullOrEmpty(token.access_token))
                    {
                        accesstoken = token.access_token;
                    }
                    
                    if (oauthToken != null && !string.IsNullOrEmpty(oauthToken.openid))
                    {
                        
                        OAuthUserInfo userInfo = HttpUtility.Get<OAuthUserInfo>(string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", accesstoken, oauthToken.openid));
                        if (userInfo != null)
                        {
                            
                            Log("获取到用户信息nickName:"+userInfo.nickname);
                            ViewData["headImage"] = userInfo.headimgurl;
                            ViewData["openid"] = userInfo.openid;
                            ViewData["nickName"] = userInfo.nickname;
                            if (userInfo.sex == 0)
                            {
                                ViewData["sex"] = "未知";
                            }
                            else if (userInfo.sex == 1)
                            {
                                ViewData["sex"] = "男";
                            }
                            else
                            {
                                ViewData["sex"] = "女";
                            }
                            ViewData["province"] = userInfo.province;
                            ViewData["city"] = userInfo.city;
                        }
                        else
                        {
                            Log("未获取到用户信息");
                        }
                    }
                    else
                    {
                        Log("access_token:" + oauthToken.access_token + ",openid:" + oauthToken.openid);
                    }
                    


                }
                else
                {
                    return Redirect(string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_base&state=123456#wechat_redirect", appID,"http://"+Request.Url.Host + Url.Action("OAuthSnsApiBase")));
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                ViewData["errmsg"] = ex.Message;
            }
            
            return View();
        }


        /**
         * snsapi_userinfo
         * **/
        public ActionResult OAuthSnsApiUserInfo()
        {
            string code = Request.QueryString["code"];
            try
            {
                if (!string.IsNullOrEmpty(code))
                {
                    OAuthToken oauthToken = HttpUtility.Get<OAuthToken>(string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", appID, appsecret, code));


                    if (oauthToken != null && !string.IsNullOrEmpty(oauthToken.openid) && !string.IsNullOrEmpty(oauthToken.access_token))
                    {

                        OAuthUserInfo userInfo = Get<OAuthUserInfo>(string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN", oauthToken.access_token, oauthToken.openid));
                        if (userInfo != null)
                        {

                            Log("获取到用户信息nickName:" + userInfo.nickname);
                            ViewData["headImage"] = userInfo.headimgurl;
                            ViewData["openid"] = userInfo.openid;
                            ViewData["nickName"] = userInfo.nickname;
                            if (userInfo.sex == 0)
                            {
                                ViewData["sex"] = "未知";
                            }
                            else if (userInfo.sex == 1)
                            {
                                ViewData["sex"] = "男";
                            }
                            else
                            {
                                ViewData["sex"] = "女";
                            }
                            ViewData["province"] = userInfo.province;
                            ViewData["city"] = userInfo.city;
                        }
                        else
                        {
                            Log("未获取到用户信息");
                        }
                    }
                    else
                    {
                        Log("access_token:" + oauthToken.access_token + ",openid:" + oauthToken.openid);
                    }

                }
                else
                {
                    return Redirect(string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_userinfo&state=123456#wechat_redirect", appID,Server.UrlEncode("http://" + Request.Url.Host + Url.Action("OAuthSnsApiUserInfo"))));
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                ViewData["errmsg"] = ex.Message;
            }
            
            return View();
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


        public  T Get<T>(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "get";
            request.Timeout = 2000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);

            string result = sr.ReadToEnd();
            Log("result:" + result);
            return JsonConvert.DeserializeObject<T>(result);


        }

    }

    public class HttpUtility
    {
        public static T Get<T>(string url)
        {
            HttpWebRequest request =(HttpWebRequest)WebRequest.Create(url);
            request.Method = "get";
            request.Timeout = 2000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);

            string result = sr.ReadToEnd();
            
            return JsonConvert.DeserializeObject<T>(result);


        }
    }
}