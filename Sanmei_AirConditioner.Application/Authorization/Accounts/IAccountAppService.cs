using System.Threading.Tasks;
using Abp.Application.Services;
using Sanmei_AirConditioner.Authorization.Accounts.Dto;

namespace Sanmei_AirConditioner.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
