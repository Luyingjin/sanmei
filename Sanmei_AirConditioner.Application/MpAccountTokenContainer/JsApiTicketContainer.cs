using Abp.Application.Services;
using Abp.Auditing;
using Abp.Runtime.Caching;
using Senparc.Weixin.Containers;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Entities;
using System;
using System.Threading.Tasks;

namespace Sanmei_AirConditioner.MpAccountTokenContainer
{
    [DisableAuditing]
    [RemoteService(IsEnabled = false)]
    public class JsApiTicketContainer : BaseContainer<JsApiTicketBag>, IJsApiTicketContainer
    {
        private readonly ICacheManager _cacheManager;
        public static object Lock = new object();
        private Func<Task<JsApiTicketBag>> RegisterFuncAsync;
        private Func<Task<AccessTokenBag>> RegisterAccessTokenFuncAsync;
        private readonly IAccessTokenContainer _AccessTokenContainer;
        public JsApiTicketContainer(ICacheManager cacheManager, IAccessTokenContainer AccessTokenContainer)
        {
            _cacheManager = cacheManager;
            _AccessTokenContainer = AccessTokenContainer;
        }
        /*此接口不提供异步方法*/

        /* 项目“Pb.Wechat.Application(net461)”的未合并的更改
        在此之前:
                public void Register(string appId, string appSecret, string name = null)
        在此之后:
                public void RegisterAsync(string appId, string appSecret, string name = null)
        */

        #region JSSDK
        public async Task RegisterAsync(string appId, string appSecret, string name = null)
        {
            //记录注册信息，RegisterFunc委托内的过程会在缓存丢失之后自动重试
            RegisterFuncAsync = async () =>
            {
                var bag = new JsApiTicketBag()
                {
                    Name = name,
                    AppId = appId,
                    AppSecret = appSecret,
                    JsApiTicketExpireTime = DateTimeOffset.MinValue,
                    JsApiTicketResult = new JsApiTicketResult()
                };
                await _cacheManager.GetCache(Sanmei_AirConditionerConsts.Cache_JsApiTicket).SetAsync(appId, bag);

                return bag;
                
            };
            await RegisterFuncAsync();

        }
        public async Task<bool> CheckRegisteredAsync(string appId)
        {
            var result =await _cacheManager.GetCache(Sanmei_AirConditionerConsts.Cache_JsApiTicket).GetOrDefaultAsync(appId);
            if (result == null)
                return false;
            return true;
        }

        /// <summary>
        /// 【异步方法】使用完整的应用凭证获取Ticket，如果不存在将自动注册
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <param name="getNewTicket"></param>
        /// <returns></returns>
        public async Task<string> TryGetJsApiTicketAsync(string appId, string appSecret, bool getNewTicket = false)
        {
            return await Senparc.Weixin.MP.Containers.JsApiTicketContainer.TryGetJsApiTicketAsync(appId, appSecret, getNewTicket);
            //if (!await CheckRegisteredAsync(appId) || getNewTicket)
            //{
            //    await RegisterAsync(appId, appSecret);
            //}
            //return await GetJsApiTicketAsync(appId, getNewTicket);
        }

        /// <summary>
        ///【异步方法】 获取可用Ticket
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="getNewTicket">是否强制重新获取新的Ticket</param>
        /// <returns></returns>
        public async Task<string> GetJsApiTicketAsync(string appId, bool getNewTicket = false)
        {
            return await Senparc.Weixin.MP.Containers.JsApiTicketContainer.GetJsApiTicketAsync(appId, getNewTicket);
            //var result = await GetJsApiTicketResultAsync(appId, getNewTicket);
            //return result.ticket;
        }

        /// <summary>
        /// 【异步方法】获取可用Ticket
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="getNewTicket">是否强制重新获取新的Ticket</param>
        /// <returns></returns>
        public async Task<JsApiTicketResult> GetJsApiTicketResultAsync(string appId, bool getNewTicket = false)
        {
            return await Senparc.Weixin.MP.Containers.JsApiTicketContainer.GetJsApiTicketResultAsync(appId, getNewTicket);
            //if (!await CheckRegisteredAsync(appId))
            //{
            //    throw new UnRegisterAppIdException(null, "此appId尚未注册，请先使用JsApiTicketContainer.Register完成注册（全局执行一次即可）！");
            //}

            //JsApiTicketBag jsApiTicketBag = (JsApiTicketBag)_cacheManager.GetCache(AppConsts.Cache_JsApiTicket).GetOrDefault(appId);

            //lock (Lock)
            //{
            //    if (getNewTicket || jsApiTicketBag.JsApiTicketExpireTime <= DateTime.Now)
            //    {
            //        //已过期，重新获取
            //        //jsApiTicketBag.JsApiTicketResult = CommonApi.GetTicket(jsApiTicketBag.AppId, jsApiTicketBag.AppSecret);
            //        jsApiTicketBag.JsApiTicketResult = GetTicket(jsApiTicketBag.AppId, jsApiTicketBag.AppSecret);

            //        jsApiTicketBag.JsApiTicketExpireTime = DateTime.Now.AddSeconds(jsApiTicketBag.JsApiTicketResult.expires_in);
            //        _cacheManager.GetCache(AppConsts.Cache_JsApiTicket).Set(appId, jsApiTicketBag, TimeSpan.FromSeconds(jsApiTicketBag.JsApiTicketResult.expires_in), TimeSpan.FromSeconds(jsApiTicketBag.JsApiTicketResult.expires_in));
            //    }
            //}

            
            //return jsApiTicketBag.JsApiTicketResult;
        }

        private JsApiTicketResult GetTicket(string appId, string secret, string type = "jsapi")
        {
            //var accessToken = TryGetAccessTokenAsync(appId,secret).Result;
            var accessToken = _AccessTokenContainer.TryGetAccessTokenAsync(appId, secret).Result;
            var result= CommonApi.GetTicketByAccessToken(accessToken, type);
            if (result.errcode != Senparc.Weixin.ReturnCode.请求成功)
            {
                //accessToken = TryGetAccessTokenAsync(appId, secret,true).Result;
                accessToken = _AccessTokenContainer.TryGetAccessTokenAsync(appId, secret,true).Result;
                result = CommonApi.GetTicketByAccessToken(accessToken, type);
            }
            return result;
        }
        #endregion

        #region AccessToken
        /// <summary>
        /// 注册应用凭证信息，此操作只是注册，不会马上获取Token，并将清空之前的Token
        /// </summary>
        /// <param name="appId">微信公众号后台的【开发】>【基本配置】中的“AppID(应用ID)”</param>
        /// <param name="appSecret">微信公众号后台的【开发】>【基本配置】中的“AppSecret(应用密钥)”</param>
        /// <param name="name">标记AccessToken名称（如微信公众号名称），帮助管理员识别</param>
        public async Task RegisterAccessTokenAsync(string appId, string appSecret, string name = null)
        {

            //记录注册信息，RegisterFunc委托内的过程会在缓存丢失之后自动重试
            RegisterAccessTokenFuncAsync = async () =>
            {
                var bag = new AccessTokenBag()
                {
                    //Key = appId,
                    Name = name,
                    AppId = appId,
                    AppSecret = appSecret,
                    AccessTokenExpireTime = DateTimeOffset.MinValue,
                    AccessTokenResult = new AccessTokenResult()
                };
                await _cacheManager.GetCache(Sanmei_AirConditionerConsts.Cache_AccessToken).SetAsync(appId, bag);

                return bag;

            };
            await RegisterAccessTokenFuncAsync();

            //为JsApiTicketContainer进行自动注册

            await RegisterAsync(appId, appSecret, name);

        }
        public async new Task<bool> CheckAccessTokenRegistered(string appId)
        {
            var result = await _cacheManager.GetCache(Sanmei_AirConditionerConsts.Cache_AccessToken).GetOrDefaultAsync(appId);
            if (result == null)
                return false;
            return true;
        }
        /// <summary>
        /// 获取可用AccessTokenResult对象
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="getNewToken">是否强制重新获取新的Token</param>
        /// <returns></returns>
        public async Task<IAccessTokenResult> GetAccessTokenResultAsync(string appId, bool getNewToken = false)
        {
            if (!await CheckAccessTokenRegistered(appId))
            {
                throw new UnRegisterAppIdException(appId, string.Format("此appId（{0}）尚未注册，请先使用AccessTokenContainer.Register完成注册（全局执行一次即可）！", appId));
            }

            AccessTokenBag accessTokenBag = (AccessTokenBag)_cacheManager.GetCache(Sanmei_AirConditionerConsts.Cache_AccessToken).GetOrDefault(appId);

            lock (Lock)
            {

                if (getNewToken || accessTokenBag.AccessTokenExpireTime <= DateTime.Now)
                {
                    //已过期，重新获取
                    accessTokenBag.AccessTokenResult = CommonApi.GetToken(accessTokenBag.AppId, accessTokenBag.AppSecret);
                    accessTokenBag.AccessTokenExpireTime = DateTime.Now.AddSeconds(accessTokenBag.AccessTokenResult.expires_in);
                    _cacheManager.GetCache(Sanmei_AirConditionerConsts.Cache_AccessToken).Set(appId, accessTokenBag);
                }
            }

            return accessTokenBag.AccessTokenResult;
        }
       

        /// <summary>
        /// 【异步方法】使用完整的应用凭证获取Token，如果不存在将自动注册
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <param name="getNewToken"></param>
        /// <returns></returns>
        public async Task<string> TryGetAccessTokenAsync(string appId, string appSecret, bool getNewToken = false)
        {
            if (!await CheckAccessTokenRegistered(appId) || getNewToken)
            {
                await RegisterAccessTokenAsync(appId, appSecret);
            }
            return await GetAccessTokenAsync(appId, getNewToken);
        }
        /// <summary>
        /// 【异步方法】获取可用Token
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="getNewToken">是否强制重新获取新的Token</param>
        /// <returns></returns>
        public async Task<string> GetAccessTokenAsync(string appId, bool getNewToken = false)
        {

            var result = await GetAccessTokenResultAsync(appId, getNewToken);
            return result.access_token;
        }
        #endregion
    }
}
