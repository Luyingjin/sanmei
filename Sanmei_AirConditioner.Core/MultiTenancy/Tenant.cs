using Abp.MultiTenancy;
using Sanmei_AirConditioner.Authorization.Users;

namespace Sanmei_AirConditioner.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {
            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}