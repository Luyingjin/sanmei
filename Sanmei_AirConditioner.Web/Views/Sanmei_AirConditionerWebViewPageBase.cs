using Abp.Web.Mvc.Views;

namespace Sanmei_AirConditioner.Web.Views
{
    public abstract class Sanmei_AirConditionerWebViewPageBase : Sanmei_AirConditionerWebViewPageBase<dynamic>
    {

    }

    public abstract class Sanmei_AirConditionerWebViewPageBase<TModel> : AbpWebViewPage<TModel>
    {
        protected Sanmei_AirConditionerWebViewPageBase()
        {
            LocalizationSourceName = Sanmei_AirConditionerConsts.LocalizationSourceName;
        }
    }
}