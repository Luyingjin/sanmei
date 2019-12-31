using System.Data.Entity;
using System.Reflection;
using Abp.Modules;
using Sanmei_AirConditioner.EntityFramework;

namespace Sanmei_AirConditioner.Migrator
{
    [DependsOn(typeof(Sanmei_AirConditionerDataModule))]
    public class Sanmei_AirConditionerMigratorModule : AbpModule
    {
        public override void PreInitialize()
        {
            Database.SetInitializer<Sanmei_AirConditionerDbContext>(null);

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}