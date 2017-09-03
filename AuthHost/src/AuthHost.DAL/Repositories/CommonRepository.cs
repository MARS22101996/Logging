using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AuthHost.DAL.Entities;
using AuthHost.DAL.Interfaces;
using MongoDB.Driver;

namespace AuthHost.DAL.Repositories
{
    public class CommonRepository<TEntity> : IRepository<TEntity> where TEntity : BaseType, new()
    {
        protected readonly IDbContext Context;
        protected readonly string CollectionName;

        public CommonRepository(IDbContext context)
        {
            Context = context;
            CollectionName = new TEntity().CollectionName;
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            var collection = Context.GetCollection<TEntity>(CollectionName);

            return collection.AsQueryable();
        }

        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> expression)
        {
            var collection = Context.GetCollection<TEntity>(CollectionName);
            var filterCompile = expression.Compile();
            var entities = collection.AsQueryable().Where(filterCompile);

            return entities.AsQueryable();
        }

        public virtual async Task CreateAsync(TEntity item)
        {
            var collection = Context.GetCollection<TEntity>(CollectionName);
            item.Id = Guid.NewGuid();

            await collection.InsertOneAsync(item);
        }
    }
}