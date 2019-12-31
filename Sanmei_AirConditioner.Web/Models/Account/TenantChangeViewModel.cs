using Abp.AutoMapper;
using Sanmei_AirConditioner.Sessions.Dto;

namespace Sanmei_AirConditioner.Web.Models.Account
{
    [AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
    public class TenantChangeViewModel
    {
        public TenantLoginInfoDto Tenant { get; set; }
    }
}