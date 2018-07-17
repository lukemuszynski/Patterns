using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Licensing.DataAccessLayer.Repositories.Abstracts.Base;

namespace Licensing.DataAccessLayer.Repositories.Implementations.Base
{
    public abstract class RepositoryBase<TEntity, TContext> : IRepository<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        protected TContext Context { get; private set; }

        protected RepositoryBase(TContext context)
        {
            Context = context;
        }

        public virtual async Task<TEntity> Get(int id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }

        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }
    }
}
