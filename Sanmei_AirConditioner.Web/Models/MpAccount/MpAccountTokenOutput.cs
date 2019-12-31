using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sanmei_AirConditioner.Web.Models.MpAccount
{
    public class MpAccountTokenOutput
    {
        /// <summary>
        /// 公众号编号
        /// </summary>
        public int MpId { get { return 1; } }
        /// <summary>
        /// 公众号APPID
        /// </summary>
        public string AppId { get; set; }
       /// <summary>
       /// 公众号APPSECRET
       /// </summary>
        public string AppSecret { get; set; }
        /// <summary>
        /// 支付SECRET
        /// </summary>
        public string WxPayAppSecret { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        public string MchID { get; set; }
        /// <summary>
        /// 证书地址
        /// </summary>
        public string CertPhysicalPath { get; set; }
        /// <summary>
        /// 证书密码
        /// </summary>
        public string CertPassword { get; set; }
    }
}