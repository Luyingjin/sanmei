using Abp.Auditing;
using Abp.Web.Mvc.Controllers;
using Sanmei_AirConditioner.Web.Models;
using Senparc.CO2NET.Utilities;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Sanmei_AirConditioner.Web.Controllers
{
    [DisableAuditing]
    public class MpCallbackController : AbpController
    {
        #region 公众号回调
        [HttpGet]
        [ActionName("CommonMiniPost")]
        public async Task<ActionResult> Get(PostModel postModel, string echostr)
        {
           
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, ConfigData.ConfigDataDics["MpToken"]))
            {
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            else
            {
                return Content("failed:" + postModel.Signature + "," + Senparc.Weixin.MP.CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, ConfigData.ConfigDataDics["MpToken"]) + "。" +
                    "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
            }
        }

      
        #endregion

    }
}