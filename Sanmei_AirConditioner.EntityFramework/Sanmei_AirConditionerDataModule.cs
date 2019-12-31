using System.Data.Entity;
using System.Reflection;
using Abp.Modules;
using Abp.Zero.EntityFramework;
using Sanmei_AirConditioner.EntityFramework;

namespace Sanmei_AirConditioner
{
    [DependsOn(typeof(AbpZeroEntityFrameworkModule), typeof(Sanmei_AirConditionerCoreModule))]
    public class Sanmei_AirConditionerDataModule : AbpModule
    {
        public override void PreInitialize()
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<Sanmei_AirConditionerDbContext>());

            Configuration.DefaultNameOrConnectionString = "Default";
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
