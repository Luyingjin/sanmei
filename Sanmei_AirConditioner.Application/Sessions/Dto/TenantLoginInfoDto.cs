using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sanmei_AirConditioner.MultiTenancy;

namespace Sanmei_AirConditioner.Sessions.Dto
{
    [AutoMapFrom(typeof(Tenant))]
    public class TenantLoginInfoDto : EntityDto
    {
        public string TenancyName { get; set; }

        public string Name { get; set; }
    }
}