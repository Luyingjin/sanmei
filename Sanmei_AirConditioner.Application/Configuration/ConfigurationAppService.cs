using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Sanmei_AirConditioner.Configuration.Dto;

namespace Sanmei_AirConditioner.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : Sanmei_AirConditionerAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
