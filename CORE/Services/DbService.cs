using CORE.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CORE.Services
{
    // Abstract base class to perform CRUD (Create, Read, Update and Delete)
    // database operations for a specific entity type inherited from the
    // Record class and has a parameterless constructor.
    // Inherits from the Service class to manage culture and return
    // successful or failure operation result CommandResponse objects
    // after operations.
    // Implements the IDisposable interface to release unmanaged resources.
    public abstract class DbService<TEntity> : Service, IDisposable
        where TEntity : Record, new()
    {
        // Field for Dependency Injection for the DbContext instance to
        // perform database operations in the below methods.
        private readonly DbContext _db;

        // Constructor with an injected DbContext instance parameter 
        // for performing database operations in the below methods by
        // using the injected instance assigned _db field.
        protected DbService(DbContext db)
        {
            _db = db;
        }

        // Method to return the entity query such as "select * from table"
        // where table is the related entity DbSet.
        // Defined as virtual to allow derived classes to override the
        // query if needed such as for ordering, filtering, including
        // the related entities to the query, etc.
        // AsNoTracking is used for not tracking the returned entities to
        // improve performance. If not written, the default
        // behavior is tracking.
        protected virtual IQueryable<TEntity> DbQuery()
            => _db.Set<TEntity>().AsNoTracking();

        // Method to return a single tracked (AsTracking) entity for update
        // or delete operations by its ID using the base or derived overridden
        // DbQuery method. If the entity is not found, returns null since
        // OrDefault version of the Single method is used.
        protected async Task<TEntity> DbSingleAsync(int id,
            CancellationToken cancellationToken = default)
        {
            return await DbQuery().AsTracking()
                .SingleOrDefaultAsync(entity => entity.Id == id,
                    cancellationToken);
        }

        // Method to return a single tracked (AsTracking) entity
        // according to a lambda expression filter parameter (predicate)
        // which may contain one or more conditions using the base or
        // derived overridden DbQuery method. If the entity is not found,
        // returns null since OrDefault version of the Single method is used.
        protected async Task<TEntity> DbSingleAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
            => await DbQuery().AsTracking().SingleOrDefaultAsync(predicate, 
                cancellationToken);

        // Method to persist changes to the database and return the number
        // of effected rows of the table.
        // Defined as virtual to allow derived classes to override the behavior
        // if needed such as for extra logging, error handling, etc.
        protected virtual async Task<int> DbSaveAsync(
            CancellationToken cancellationToken = default) 
            => await _db.SaveChangesAsync(cancellationToken);

        // Method to insert a new entity to the entity DbSet and persist changes
        // to the database if the save default parameter value is true.
        // If the save parameter value is false, the changes will not be
        // persisted to the database, which can be used for multiple insert
        // operations. Then DbSave method can be invoked to persist all changes
        // to the database once (Unit of Work) to increase performance.
        protected async Task DbAddAsync(TEntity entity, 
            CancellationToken cancellationToken,
            bool save = true)
        {
            _db.Set<TEntity>().Add(entity);
            if (save)
                await DbSaveAsync(cancellationToken);
        }

        // Method to update an existing entity retrieved by DbSingle method
        // and persist changes to the database if the save default parameter
        // value is true. If the save parameter value is false, the changes will
        // not be persisted to the database, which can be used for multiple
        // update operations. Then DbSave method can be invoked to persist all
        // changes to the database once (Unit of Work) to increase performance.
        protected async Task DbUpdateAsync(TEntity entity,
            CancellationToken cancellationToken,
            bool save = true)
        {
            _db.Set<TEntity>().Update(entity);
            if (save)
                await DbSaveAsync(cancellationToken);
        }

        // Method to delete an existing entity retrieved by DbSingle method
        // and persist changes to the database if the save default parameter
        // value is true. If the save parameter value is false, the changes will
        // not be persisted to the database, which can be used for multiple
        // delete operations. Then DbSave method can be invoked to persist all
        // changes to the database once (Unit of Work) to increase performance.
        protected async Task DbRemoveAsync(TEntity entity,
            CancellationToken cancellationToken,
            bool save = true)
        {
            _db.Set<TEntity>().Remove(entity);
            if (save)
                await DbSaveAsync(cancellationToken);
        }

        // Method to delete the related navigation entities of an entity.
        // The changes will not be persisted to the database, therefore
        // DbAddAsync, DbUpdateAsync, DbRemoveAsync or DbSaveAsync methods
        // should be invoked after to persist all changes to the database.
        protected void DbRemove<TNavigationEntity>(List<TNavigationEntity>
            navigationEntities) where TNavigationEntity : Record, new()
        {
            _db.Set<TNavigationEntity>().RemoveRange(navigationEntities);
        }

        // Method to dispose the DbContext instance and suppress finalization
        // (don't call the finalizer) of the instance of this class to
        // improve performance and avoid memory leaks.
        public void Dispose()
        {
            _db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
