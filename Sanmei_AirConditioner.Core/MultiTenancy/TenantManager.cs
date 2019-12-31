using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Sanmei_AirConditioner.Authorization.Users;
using Sanmei_AirConditioner.Editions;

namespace Sanmei_AirConditioner.MultiTenancy
{
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        public TenantManager(
            IRepository<Tenant> tenantRepository, 
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository, 
            EditionManager editionManager,
            IAbpZeroFeatureValueStore featureValueStore
            ) 
            : base(
                tenantRepository, 
                tenantFeatureRepository, 
                editionManager,
                featureValueStore
            )
        {
        }
    }
}