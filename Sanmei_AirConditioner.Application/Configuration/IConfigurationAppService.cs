using System.Threading.Tasks;
using Abp.Application.Services;
using Sanmei_AirConditioner.Configuration.Dto;

namespace Sanmei_AirConditioner.Configuration
{
    public interface IConfigurationAppService: IApplicationService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}