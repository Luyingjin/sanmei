using System.Data.Common;
using System.Data.Entity;
using Abp.Zero.EntityFramework;
using Sanmei_AirConditioner.Authorization.Roles;
using Sanmei_AirConditioner.Authorization.Users;
using Sanmei_AirConditioner.MultiTenancy;

namespace Sanmei_AirConditioner.EntityFramework
{
    public class Sanmei_AirConditionerDbContext : AbpZeroDbContext<Tenant, Role, User>
    {
        //TODO: Define an IDbSet for your Entities...

        /* NOTE: 
         *   Setting "Default" to base class helps us when working migration commands on Package Manager Console.
         *   But it may cause problems when working Migrate.exe of EF. If you will apply migrations on command line, do not
         *   pass connection string name to base classes. ABP works either way.
         */
        public Sanmei_AirConditionerDbContext()
            : base("Default")
        {

        }

        /* NOTE:
         *   This constructor is used by ABP to pass connection string defined in Sanmei_AirConditionerDataModule.PreInitialize.
         *   Notice that, actually you will not directly create an instance of Sanmei_AirConditionerDbContext since ABP automatically handles it.
         */
        public Sanmei_AirConditionerDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }

        //This constructor is used in tests
        public Sanmei_AirConditionerDbContext(DbConnection existingConnection)
         : base(existingConnection, false)
        {

        }

        public Sanmei_AirConditionerDbContext(DbConnection existingConnection, bool contextOwnsConnection)
         : base(existingConnection, contextOwnsConnection)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
