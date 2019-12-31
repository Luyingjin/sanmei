using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sanmei_AirConditioner.MultiTenancy.Dto;

namespace Sanmei_AirConditioner.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}
