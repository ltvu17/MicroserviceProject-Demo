using System.Linq.Expressions;
using MongoDB.Driver;

namespace Play.Common
{
    public interface IRepoBase<T> where T : IEntity
    {
        public Task<IReadOnlyCollection<T>> GetAllAsync();
        public Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T,bool>> filter);
        public Task<T> GetAsync(Guid id);
        public Task<T> GetAsync(Expression<Func<T,bool>> filter);
        public Task CreateAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task RemoveAsync(Guid id);
    }
    public class RepoBase<T> : IRepoBase<T> where T : IEntity
    {
        private readonly IMongoCollection<T> dbCollection;
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public RepoBase(IMongoDatabase database, string collectionName)
        {
            dbCollection = database.GetCollection<T>(collectionName);
        }
        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }
        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).ToListAsync();
        }
        public async Task<T> GetAsync(Guid id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(p => p.Id, id);
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task CreateAsync(T entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            await dbCollection.InsertOneAsync(entity);
        }
        public async Task UpdateAsync(T entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            FilterDefinition<T> filter = filterBuilder.Eq(p => p.Id, entity.Id);
            await dbCollection.ReplaceOneAsync(filter,entity);
        }
        public async Task RemoveAsync(Guid id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(p => p.Id, id);
            await dbCollection.DeleteOneAsync(filter);

        }
    }
}