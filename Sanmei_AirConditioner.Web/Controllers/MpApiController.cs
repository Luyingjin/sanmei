using Abp.Auditing;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.Web.Models;
using Abp.Web.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanmei_AirConditioner.Helpers;
using Sanmei_AirConditioner.MpAccountTokenContainer;
using Sanmei_AirConditioner.Web.Models;
using Sanmei_AirConditioner.Web.Models.MpAccount;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.TenPay.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Sanmei_AirConditioner.Web.Controllers
{

    [DisableAuditing]
    public class MpApiController : AbpController
    {

        private readonly IAbpSession _abpSession;
        private readonly ICacheManager _cacheManager;
        private readonly IAccessTokenContainer _accessTokenContainer;
        private readonly IJsApiTicketContainer _jsApiTicketContainer;

        public IClientInfoProvider ClientInfoProvider { get; set; }

        public MpApiController(IAccessTokenContainer accessTokenContainer,
            IJsApiTicketContainer jsApiTicketContainer,
            IAbpSession abpSession,
            ICacheManager cacheManager
            )
        {
           
            ClientInfoProvider = NullClientInfoProvider.Instance;
            _accessTokenContainer = accessTokenContainer;
            _jsApiTicketContainer = jsApiTicketContainer;
            _abpSession = abpSession;
            _cacheManager = cacheManager;
        }

        [HttpPost]
        public async Task<ActionResult> GetAccessToken(string token, int getnewtoken = 0)
        {
            var account = GetAccountToken();
            if (account == null)
            {
                Logger.Error($"token为“{token}”的静默授权失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("公众号或令牌不存在");
            }
            return Json(new
            {
                access_token = await _accessTokenContainer.TryGetAccessTokenAsync(account.AppId, account.AppSecret, getnewtoken == 1)
            });
        }

        #region OpenID隐式授权接口
        [HttpGet]
        public async Task<ActionResult> OAuth2Base(string token, string reurl, string needRegister = "0", string baseEncryp = "1")
        {
            var account =  GetAccountToken();
            if (account == null)
            {
                return Content("公众号或令牌不存在");
            }
            if (string.IsNullOrEmpty(reurl))
            {
                return Content("reurl为空");
            }
          
            string appId = account.AppId;
           
            var url = Senparc.Weixin.MP.AdvancedAPIs.OAuthApi.GetAuthorizeUrl(appId, $"{Request.Url.Scheme}://{Request.Url.Host}/mpapi/OAuth2BaseCallback?mpid={account.MpId}&token={token}&reurl={reurl}&baseEncryp={baseEncryp}&needRegister={needRegister}", "JeffreySu", Senparc.Weixin.MP.OAuthScope.snsapi_base);
            return Redirect(url);
        }
        [HttpGet]
        public async Task<ActionResult> OAuth2BaseCallback(string token, string reurl, string code, string state, string needRegister = "0", string baseEncryp = "1")
        {

            var url = Base64Helper.DecodeBase64(reurl.Replace(" ", "+"));
            if (string.IsNullOrEmpty(code))
            {
                Logger.Error($"token为“{token}”的静默授权失败，原因：拒绝了授权，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("您拒绝了授权！");
            }

            if (state != "JeffreySu" && state != "JeffreySu?10000skip=true")
            {
                //这里的state其实是会暴露给客户端的，验证能力很弱，这里只是演示一下
                //实际上可以存任何想传递的数据，比如用户ID，并且需要结合例如下面的Session["OAuthAccessToken"]进行验证
                Logger.Error($"token为“{token}”的静默授权失败，原因：验证失败，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("验证失败！请从正规途径进入！");
            }

            var account = GetAccountToken();
            if (account == null)
            {
                Logger.Error($"token为“{token}”的静默授权失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("公众号不存在");
            }

            //通过，用code换取access_token
            //OAuthAccessTokenResult result = null;
            Senparc.Weixin.MP.AdvancedAPIs.OAuth.OAuthAccessTokenResult result = null;
            try
            {
                result = Senparc.Weixin.MP.AdvancedAPIs.OAuthApi.GetAccessToken(account.AppId, account.AppSecret, code);
                //result = await _accessTokenContainer.GetAccessTokenAsync(account.MpId, code);
            }
            catch (Exception ex)
            {
                Logger.Error($"token为“{token}”的静默授权在通过code获取token时异常，原因：{ex.Message}，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("错误：" + ex.Message);
            }

            //if (result.errcode != Senparc.Weixin.ReturnCode.请求成功)
            //{
            //    Logger.Error($"token为“{token}”的静默授权在通过code获取token时异常，原因：{result.errmsg}，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
            //    return Content("错误：" + result.errmsg);
            //}




            if (baseEncryp == "1")
            {
                var paramJson = Base64Helper.EncodeBase64(JsonConvert.SerializeObject(new
                {
                    result.openid,
                    MpId = account.MpId
                }));
                url = string.Format("{0}{1}userInfo={2}&token={3}"
                   , url, url.Contains("?") ? "&" : "?", paramJson, token);

            }
            else
                url = string.Format("{0}{1}openid={2}&MpId={3}"
                  , url, url.Contains("?") ? "&" : "?", result.openid, account.MpId);


            return Redirect(url);
        }
        #endregion

        #region OpenID显示授权接口
        [HttpGet]
        [DisableAuditing]
        public async Task<ActionResult> OAuth2UserInfo(string token, string reurl, string needRegister = "0", string baseEncryp = "1")
        {

            var account = GetAccountToken();
            if (account == null)
            {
                return Content("公众号或令牌不存在");
            }
            if (string.IsNullOrEmpty(reurl))
            {
                return Content("reurl为空");
            }
            
            string appId = account.AppId;
            //var url = await _accessTokenContainer.GetAuthorizeUrl(account.MpId, $"{Request.Url.Scheme}://{Request.Url.Host}/mpapi/OAuth2UserInfoCallback?mpid={account.MpId}&token={token}&reurl={reurl}&baseEncryp={baseEncryp}&needRegister={needRegister}", "JeffreySu", "snsapi_userinfo");
            var url = Senparc.Weixin.MP.AdvancedAPIs.OAuthApi.GetAuthorizeUrl(appId, $"{Request.Url.Scheme}://{Request.Url.Host}/mpapi/OAuth2UserInfoCallback?mpid={account.MpId}&token={token}&reurl={reurl}&baseEncryp={baseEncryp}&needRegister={needRegister}", "JeffreySu", Senparc.Weixin.MP.OAuthScope.snsapi_userinfo);
            return Redirect(url);
        }

        [HttpGet]
        public async Task<ActionResult> OAuth2UserInfoCallback(string token, string reurl, string code, string state, string needRegister = "0", string baseEncryp = "1")
        {

            var url = Base64Helper.DecodeBase64(reurl.Replace(" ", "+"));

            if (string.IsNullOrEmpty(code))
            {
                Logger.Error($"token为“{token}”的认证授权失败，原因：拒绝了授权，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("您拒绝了授权！");
            }

            if (state != "JeffreySu" && state != "JeffreySu?10000skip=true")
            {
                //这里的state其实是会暴露给客户端的，验证能力很弱，这里只是演示一下
                //实际上可以存任何想传递的数据，比如用户ID，并且需要结合例如下面的Session["OAuthAccessToken"]进行验证
                Logger.Error($"token为“{token}”的认证授权失败，原因：验证失败，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("验证失败！请从正规途径进入！");
            }

            //通过，用code换取access_token
            var account = GetAccountToken();
            if (account == null)
            {
                Logger.Error($"token为“{token}”的认证授权失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("公众号不存在");
            }

            //OAuthAccessTokenResult result = null;
            Senparc.Weixin.MP.AdvancedAPIs.OAuth.OAuthAccessTokenResult result = null;
            try
            {
                //result = await _accessTokenContainer.GetAccessTokenAsync(account.MpId, code);
                result = Senparc.Weixin.MP.AdvancedAPIs.OAuthApi.GetAccessToken(account.AppId, account.AppSecret, code);
                Logger.Error($"获取OAuthAccessTokenResult：{JsonConvert.SerializeObject(result)}");
            }
            catch (Exception ex)
            {
                Logger.Error($"token为“{token}”的认证授权在通过code获取token时异常，原因：{ex.Message}，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("错误：" + ex.Message);
            }

            //if (result.errcode != Senparc.Weixin.ReturnCode.请求成功)
            //{
            //    Logger.Error($"token为“{token}”的认证授权在通过code获取token时失败，原因：{result.errmsg}，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
            //    return Content("错误：" + result.errmsg);
            //}

            //string mgccAuthKey = await _mpUserMemberAppService.GetMgccAuthKey(result.openid);

            //因为这里还不确定用户是否关注本微信，所以只能试探性地获取一下
            //OAuthUserInfo userInfo = null;
            Senparc.Weixin.MP.AdvancedAPIs.OAuth.OAuthUserInfo userInfo = null;
            string baseEncrypString = string.Empty;
            try
            {

                //已关注，可以得到详细信息
                //userInfo = await _accessTokenContainer.GetUserInfoAsync(result.access_token, result.openid);
                userInfo = Senparc.Weixin.MP.AdvancedAPIs.OAuthApi.GetUserInfo(result.access_token, result.openid);

                if (baseEncryp == "1")
                {
                    var unBase64 = JsonConvert.SerializeObject(new
                    {
                        result.openid,
                        nickname = userInfo.nickname,
                        headimgurl = userInfo.headimgurl,
                        MpId = account.MpId
                    });

                    var paramJson = Base64Helper.EncodeBase64(unBase64);


                    url = string.Format("{0}{1}userInfo={2}"
                       , url, url.Contains("?") ? "&" : "?", paramJson);
                }
                else
                    url = string.Format("{0}{1}openid={2}&nickname={3}&headimgurl={4}&MpId={5}"
                      , url, url.Contains("?") ? "&" : "?", result.openid, userInfo.nickname, userInfo.headimgurl, account.MpId);



                return Redirect(url);
            }
            catch (Senparc.Weixin.Exceptions.ErrorJsonResultException ex)
            {
                Logger.Error($"token为“{token}”的认证授权失败，原因：{ex.Message}，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("错误：" + ex.Message);
            }
        }
        #endregion

        #region JSSDK接口
        [HttpPost]
        public async Task<ActionResult> Jssdk(string token, string callurl)
        {

            var account = GetAccountToken();
            if (account == null)
            {
                Logger.Error($"token为“{token}”的Jssdk失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("公众号或令牌不存在");
            }
            //var ticket = await _accessTokenContainer.GetJsApiTicketAsync(account.MpId);
            var ticket = await _jsApiTicketContainer.TryGetJsApiTicketAsync(account.AppId, account.AppSecret);
            //var ticket = await Senparc.Weixin.MP.Containers.JsApiTicketContainer.TryGetJsApiTicketAsync(account.AppId, account.AppSecret);

            var url = Base64Helper.DecodeBase64(callurl.Replace(" ", "+"));

            string timestamp = Convert.ToString(ConvertDateTimeInt(DateTime.Now));
            string nonceStr = createNonceStr();
            string rawstring = "jsapi_ticket=" + ticket + "&noncestr=" + nonceStr + "&timestamp=" + timestamp + "&url=" + url;

            string signature = SHA1_Hash(rawstring);

            return Json(new
            {
                appId = account.AppId,
                nonceStr = nonceStr,
                timestamp = timestamp,
                url = url,
                signature = signature,
                rawString = rawstring,
            });


        }

        [HttpGet]
        public async Task<ActionResult> JssdkJsonP(string token, string callurl, string callback)
        {

            var account = GetAccountToken();
            if (account == null)
            {
                Logger.Error($"token为“{token}”的JssdkJsonp失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("公众号或令牌不存在");
            }
            var ticket = await _jsApiTicketContainer.TryGetJsApiTicketAsync(account.AppId, account.AppSecret);
            //var ticket = await _accessTokenContainer.GetJsApiTicketAsync(account.MpId);
            //var ticket = await Senparc.Weixin.MP.Containers.JsApiTicketContainer.TryGetJsApiTicketAsync(account.AppId, account.AppSecret);
            var url = Base64Helper.DecodeBase64(callurl.Replace(" ", "+"));
            string timestamp = Convert.ToString(ConvertDateTimeInt(DateTime.Now));
            string nonceStr = createNonceStr();
            string rawstring = "jsapi_ticket=" + ticket + "&noncestr=" + nonceStr + "&timestamp=" + timestamp + "&url=" + url;
            string signature = SHA1_Hash(rawstring);

            return Content(string.IsNullOrEmpty(callback) ? "" : string.Format("{0}({1})", callback, JsonConvert.SerializeObject(new
            {
                appId = account.AppId,
                nonceStr = nonceStr,
                timestamp = timestamp,
                url = url,
                signature = signature,
                rawString = rawstring,
            })));


        }
        #region jssdk私有函数
        private string createNonceStr()
        {
            int length = 16;
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string str = "";
            Random rad = new Random();
            for (int i = 0; i < length; i++)
            {
                str += chars.Substring(rad.Next(0, chars.Length - 1), 1);
            }
            return str;
        }

        /// <summary> 
        /// 将c# DateTime时间格式转换为Unix时间戳格式 
        /// </summary> 
        /// <param name="time">时间</param> 
        /// <returns>double</returns> 
        public int ConvertDateTimeInt(System.DateTime time)
        {
            int intResult = 0;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            intResult = Convert.ToInt32((time - startTime).TotalSeconds);
            return intResult;
        }

        //SHA1哈希加密算法 
        public string SHA1_Hash(string str_sha1_in)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes_sha1_in = System.Text.UTF8Encoding.Default.GetBytes(str_sha1_in);
            byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
            string str_sha1_out = BitConverter.ToString(bytes_sha1_out);
            str_sha1_out = str_sha1_out.Replace("-", "").ToLower();
            return str_sha1_out;
        }
        #endregion
        #endregion

        #region 获取微信用户信息
        [HttpPost]
        //[ResponseCache(Duration = AbpZeroTemplateConsts.PageOutputCacheDuration, VaryByQueryKeys = new string[] { "token", "openid" })]
        public async Task<ActionResult> GetUser(string token, string openid)
        {
            var account = GetAccountToken();
            if (account == null)
            {
                Logger.Error($"token为“{token}”的用户信息获取失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("公众号或令牌不存在");
            }

            #region 获取用户信息
            //OAuthUserInfo userinfo = null;
            Senparc.Weixin.MP.AdvancedAPIs.User.UserInfoJson userinfo = null;
            try
            {
                var access_token = await _accessTokenContainer.TryGetAccessTokenAsync(account.AppId, account.AppSecret);
                //userinfo = await _accessTokenContainer.GetUserInfoAsync(access_token, openid);
                userinfo = await Senparc.Weixin.MP.AdvancedAPIs.UserApi.InfoAsync(access_token, openid);
            }
            catch
            {
                try
                {
                    var access_token = await _accessTokenContainer.TryGetAccessTokenAsync(account.AppId, account.AppSecret, true);
                    userinfo = await Senparc.Weixin.MP.AdvancedAPIs.UserApi.InfoAsync(access_token, openid);
                    //var access_token = await _accessTokenContainer.TryGetAccessTokenAsync(account.MpId, true);
                    //userinfo = await _accessTokenContainer.GetUserInfoAsync(access_token, openid);
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        errorcode = "500",
                        errormsg = ex.Message,
                    });
                }
            }
            #endregion

            return Json(new
            {
                errorcode = "200",
                errormsg = "",
                result = userinfo,
            });
        }

        [HttpGet]
        //[ResponseCache(Duration = AbpZeroTemplateConsts.PageOutputCacheDuration, VaryByQueryKeys = new string[] { "token", "openid" })]
        public async Task<ActionResult> GetUserJsonP(string token, string openid, string callback)
        {
            var account = GetAccountToken();
            if (account == null)
            {
                Logger.Error($"token为“{token}”的jsonp用户信息获取失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("公众号或令牌不存在");
            }

            #region 获取用户信息

            //OAuthUserInfo userinfo = null;
            Senparc.Weixin.MP.AdvancedAPIs.User.UserInfoJson userinfo = null;
            try
            {
                var access_token = await _accessTokenContainer.TryGetAccessTokenAsync(account.AppId, account.AppSecret);
                userinfo = await Senparc.Weixin.MP.AdvancedAPIs.UserApi.InfoAsync(access_token, openid);
                //var access_token = await _accessTokenContainer.TryGetAccessTokenAsync(account.MpId);
                //userinfo = await _accessTokenContainer.GetUserInfoAsync(access_token, openid);
            }
            catch
            {
                try
                {
                    var access_token = await _accessTokenContainer.TryGetAccessTokenAsync(account.AppId, account.AppSecret, true);
                    userinfo = await Senparc.Weixin.MP.AdvancedAPIs.UserApi.InfoAsync(access_token, openid);
                    //var access_token = await _accessTokenContainer.TryGetAccessTokenAsync(account.MpId, true);
                    //userinfo = await _accessTokenContainer.GetUserInfoAsync(access_token, openid);
                }
                catch (Exception ex)
                {
                    return Content(string.IsNullOrEmpty(callback) ? "" : string.Format("{0}({1})", callback, JsonConvert.SerializeObject(new
                    {
                        errorcode = "500",
                        errormsg = ex.Message,
                    })));
                }
            }
            #endregion

            return Content(string.IsNullOrEmpty(callback) ? "" : string.Format("{0}({1})", callback, JsonConvert.SerializeObject(new
            {
                errorcode = "200",
                errormsg = "",
                result = userinfo,
            })));
        }
        #endregion

        #region 红包接口
        [HttpPost]
        public async Task<ActionResult> SendRedPacket(string token, string openid, string total, string activityname, string sendername, string sendmsg, string remark)
        {
            #region 校验金额
            int rptotal = 0;
            if (!int.TryParse(total, out rptotal))
            {
                Logger.Error($"token为“{token}”的发送红包接口调用失败，原因：金额不正确");
                return Json(new
                {
                    errorcode = "202",
                    errormsg = "金额不正确",
                });
            }
            #endregion

            #region 校验公众号
            var account = GetAccountToken();
            if (account == null)
            {
                Logger.Error($"token为“{token}”的发送红包接口调用失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                return Content("公众号或令牌不存在");
            }
            #endregion

            #region 发送红包
            string mchbillno = DateTime.Now.ToString("HHmmss") + TenPayV3Util.BuildRandomStr(28);
            //本地或者服务器的证书位置（证书在微信支付申请成功发来的通知邮件中）
            string cert = account.CertPhysicalPath;
            //私钥（在安装证书时设置）
            string password = account.CertPassword;

            string nonceStr;//随机字符串
            string paySign;//签名
            var redpackageresult = RedPackApi.SendNormalRedPack(
                account.AppId,
                account.MchID,
                account.WxPayAppSecret,
                cert,
                openid,
                sendername,
                ConfigData.ConfigDataDics["ClientIp"],
                rptotal,
                sendmsg,
                activityname,
                remark,
                out nonceStr,
                out paySign,
                mchbillno);

            return Json(new
            {
                errorcode = redpackageresult.result_code == "1" ? "200" : "204",
                errormsg = redpackageresult.return_msg,
            });
            #endregion
        }
        #endregion

        #region 微信支付接口
        [HttpPost]
        public async Task<ActionResult> GetWxPayJsApi(string token, string openid, string productname, int total, string orderno)
        {
            try
            {

                #region 校验公众号
                var account =  GetAccountToken();
                if (account == null)
                {
                    Logger.Error($"token为“{token}”的发送红包接口调用失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                    return Content("公众号或令牌不存在");
                }
                #endregion

                string timeStamp = "";
                string nonceStr = "";
                string paySign = "";

                //生成订单10位序列号，此处用时间和随机数生成，商户根据自己调整，保证唯一
                orderno = string.IsNullOrEmpty(orderno) ? (DateTime.Now.ToString("yyyyMMddHHmmss") + TenPayV3Util.BuildRandomStr(3)) : orderno;

                //创建支付应答对象
                RequestHandler packageReqHandler = new RequestHandler(null);
                //初始化
                packageReqHandler.Init();

                timeStamp = TenPayV3Util.GetTimestamp();
                nonceStr = TenPayV3Util.GetNoncestr();

                //设置package订单参数
                packageReqHandler.SetParameter("appid", account.AppId);       //公众账号ID
                packageReqHandler.SetParameter("mch_id", account.MchID);          //商户号
                packageReqHandler.SetParameter("nonce_str", nonceStr);                    //随机字符串
                packageReqHandler.SetParameter("body", productname);    //商品信息
                packageReqHandler.SetParameter("out_trade_no", orderno);      //商家订单号
                packageReqHandler.SetParameter("total_fee", total.ToString());                  //商品金额,以分为单位(money * 100).ToString()
                packageReqHandler.SetParameter("spbill_create_ip", ClientInfoProvider.ClientIpAddress);   //用户的公网ip，不是商户服务器IP
                packageReqHandler.SetParameter("notify_url", $"{Request.Url.Scheme}://{Request.Url.Host}/wxapi/PayNotifyUrl");            //接收财付通通知的URL
                packageReqHandler.SetParameter("trade_type", TenPayV3Type.JSAPI.ToString());                        //交易类型
                packageReqHandler.SetParameter("openid", openid);                       //用户的openId
                packageReqHandler.SetParameter("attach", $"{account.MpId},{token}");                       //用户的openId

                string sign = packageReqHandler.CreateMd5Sign("key", account.WxPayAppSecret);
                packageReqHandler.SetParameter("sign", sign);                       //签名

                string data = packageReqHandler.ParseXML();

                var result = TenPayV3.Unifiedorder(data);
                var res = XDocument.Parse(result);
                string prepayId = res.Element("xml").Element("prepay_id").Value;

                //设置支付参数
                RequestHandler paySignReqHandler = new RequestHandler(null);
                paySignReqHandler.SetParameter("appId", account.AppId);
                paySignReqHandler.SetParameter("timeStamp", timeStamp);
                paySignReqHandler.SetParameter("nonceStr", nonceStr);
                paySignReqHandler.SetParameter("package", string.Format("prepay_id={0}", prepayId));
                paySignReqHandler.SetParameter("signType", "MD5");
                paySign = paySignReqHandler.CreateMd5Sign("key", account.WxPayAppSecret);

                ViewData["appId"] = account.AppId;
                ViewData["timeStamp"] = timeStamp;
                ViewData["nonceStr"] = nonceStr;
                ViewData["package"] = string.Format("prepay_id={0}", prepayId);
                ViewData["paySign"] = paySign;
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        appId = account.AppId,
                        timeStamp = timeStamp,
                        nonceStr = nonceStr,
                        package = string.Format("prepay_id={0}", prepayId),
                        paySign = paySign,
                        orderNO = orderno
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"openid:'{openid}',total:{total},productname:'{productname}',ex:'{ex.Message}'");
                return Json(new
                {
                    success = false,
                    msg = ex.Message,
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> PayNotifyUrl()
        {
            var returntmpl = @"<xml>
   <return_code><![CDATA[{0}]]></return_code>
   <return_msg><![CDATA[{1}]]></return_msg>
</xml>";
            try
            {
                ResponseHandler resHandler = new ResponseHandler(null);
                string return_code = resHandler.GetParameter("return_code");
                string return_msg = resHandler.GetParameter("return_msg");
                if (return_code == "SUCCESS")
                {
                    var attachs = resHandler.GetParameter("attach");
                    var mt = attachs.Split(',');
                    string openid = resHandler.GetParameter("openid");
                    string out_trade_no = resHandler.GetParameter("out_trade_no");
                    string total_fee = resHandler.GetParameter("total_fee");

                    #region 校验公众号
                    //var account = await GetAccountToken(mt[1], MpApiTokenType.Redpackage.ToString());
                    var account = GetAccountToken();
                    if (account == null)
                    {
                        Logger.Error($"attach为“{attachs}”的微信支付回调验证失败，原因：公众号或令牌不存在，Url：{Request.Url.Scheme}://{Request.Url.Host}{Request.Path}");
                        return Content("公众号或令牌不存在");
                    }
                    #endregion

                    resHandler.SetKey(account.WxPayAppSecret);
                    //验证请求是否从微信发过来（安全）
                    if (resHandler.IsTenpaySign())
                    {
                        return Content(string.Format(returntmpl, return_code, return_msg), "text/xml");
                    }
                }
                return Content(string.Format(returntmpl, "FAIL", "Validate Error"), "text/xml");
            }
            catch (Exception ex)
            {
                Logger.Error("微信支付回调验证失败", ex);
                return Content(string.Format(returntmpl, "FAIL", "System Error"), "text/xml");
            }
        }
        #endregion

        #region 私有方法
        private MpAccountTokenOutput GetAccountToken()
        {
            return new MpAccountTokenOutput {
                AppId= ConfigData.ConfigDataDics["AppId"],
                AppSecret= ConfigData.ConfigDataDics["AppSecret"],
                CertPassword= ConfigData.ConfigDataDics["CertPassword"],
                WxPayAppSecret= ConfigData.ConfigDataDics["WxPayAppSecret"],
                CertPhysicalPath= ConfigData.ConfigDataDics["CertPhysicalPath"],
                MchID= ConfigData.ConfigDataDics["MchID"]
            };
        }

       
        #endregion

       

       

    }


}