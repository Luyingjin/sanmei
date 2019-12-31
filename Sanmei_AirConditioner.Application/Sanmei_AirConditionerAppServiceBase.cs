using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Sanmei_AirConditioner.Authorization.Users;
using Sanmei_AirConditioner.MultiTenancy;
using Sanmei_AirConditioner.Users;
using Microsoft.AspNet.Identity;

namespace Sanmei_AirConditioner
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class Sanmei_AirConditionerAppServiceBase : ApplicationService
    {
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        protected Sanmei_AirConditionerAppServiceBase()
        {
            LocalizationSourceName = Sanmei_AirConditionerConsts.LocalizationSourceName;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId());
            if (user == null)
            {
                throw new ApplicationException("There is no current user!");
            }

            return user;
        }

        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}