using System.Threading.Tasks;
using Abp.Application.Services;
using Sanmei_AirConditioner.Sessions.Dto;

namespace Sanmei_AirConditioner.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
