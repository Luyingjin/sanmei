using Sanmei_AirConditioner.EntityFramework;
using EntityFramework.DynamicFilters;

namespace Sanmei_AirConditioner.Migrations.SeedData
{
    public class InitialHostDbBuilder
    {
        private readonly Sanmei_AirConditionerDbContext _context;

        public InitialHostDbBuilder(Sanmei_AirConditionerDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            _context.DisableAllFilters();

            new DefaultEditionsCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create();
        }
    }
}
