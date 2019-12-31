using System.Linq;
using Sanmei_AirConditioner.EntityFramework;
using Sanmei_AirConditioner.MultiTenancy;

namespace Sanmei_AirConditioner.Migrations.SeedData
{
    public class DefaultTenantCreator
    {
        private readonly Sanmei_AirConditionerDbContext _context;

        public DefaultTenantCreator(Sanmei_AirConditionerDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateUserAndRoles();
        }

        private void CreateUserAndRoles()
        {
            //Default tenant

            var defaultTenant = _context.Tenants.FirstOrDefault(t => t.TenancyName == Tenant.DefaultTenantName);
            if (defaultTenant == null)
            {
                _context.Tenants.Add(new Tenant {TenancyName = Tenant.DefaultTenantName, Name = Tenant.DefaultTenantName});
                _context.SaveChanges();
            }
        }
    }
}
