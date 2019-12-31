using Abp.Domain.Entities;
using Abp.EntityFramework;
using Abp.EntityFramework.Repositories;

namespace Sanmei_AirConditioner.EntityFramework.Repositories
{
    public abstract class Sanmei_AirConditionerRepositoryBase<TEntity, TPrimaryKey> : EfRepositoryBase<Sanmei_AirConditionerDbContext, TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        protected Sanmei_AirConditionerRepositoryBase(IDbContextProvider<Sanmei_AirConditionerDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }

        //add common methods for all repositories
    }

    public abstract class Sanmei_AirConditionerRepositoryBase<TEntity> : Sanmei_AirConditionerRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        protected Sanmei_AirConditionerRepositoryBase(IDbContextProvider<Sanmei_AirConditionerDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }

        //do not add any method here, add to the class above (since this inherits it)
    }
}
