using System.Data.Entity.Migrations;
using Abp.MultiTenancy;
using Abp.Zero.EntityFramework;
using Sanmei_AirConditioner.Migrations.SeedData;
using EntityFramework.DynamicFilters;

namespace Sanmei_AirConditioner.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<Sanmei_AirConditioner.EntityFramework.Sanmei_AirConditionerDbContext>, IMultiTenantSeed
    {
        public AbpTenantBase Tenant { get; set; }

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Sanmei_AirConditioner";
        }

        protected override void Seed(Sanmei_AirConditioner.EntityFramework.Sanmei_AirConditionerDbContext context)
        {
            context.DisableAllFilters();

            if (Tenant == null)
            {
                //Host seed
                new InitialHostDbBuilder(context).Create();

                //Default tenant seed (in host database).
                new DefaultTenantCreator(context).Create();
                new TenantRoleAndUserBuilder(context, 1).Create();
            }
            else
            {
                //You can add seed for tenant databases and use Tenant property...
            }

            context.SaveChanges();
        }
    }
}
