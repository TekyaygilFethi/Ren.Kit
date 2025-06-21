using EFCore.BulkExtensions;
using System.Linq.Expressions;

namespace Ren.Kit.DataKit.Abstractions;

/// <summary>
/// Defines a generic repository contract for EF Core-based data access,
/// including CRUD operations, bulk actions, querying, and existence/count checks.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
public interface IRENRepository<TEntity> where TEntity : class
{
    #region Create

    /// <summary>
    /// Inserts a single entity into the context.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    void Insert(TEntity entity);

    /// <summary>
    /// Asynchronously inserts a single entity into the context.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts multiple entities into the context.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    void Insert(IEnumerable<TEntity> entities);

    /// <summary>
    /// Asynchronously inserts multiple entities into the context.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk inserts entities using high-performance EFCore.BulkExtensions.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="bulkConfig">Optional bulk operation configuration.</param>
    void BulkInsert(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null);

    /// <summary>
    /// Asynchronously bulk inserts entities using high-performance EFCore.BulkExtensions.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="bulkConfig">Optional bulk operation configuration.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task BulkInsertAsync(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null, CancellationToken cancellationToken = default);

    #endregion

    #region Existence Checks

    /// <summary>
    /// Checks if any entities satisfy the given predicate.
    /// </summary>
    /// <param name="predicate">Optional predicate filter.</param>
    /// <returns>True if any match, otherwise false.</returns>
    bool Any(Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// Asynchronously checks if any entities satisfy the given predicate.
    /// </summary>
    /// <param name="predicate">Optional predicate filter.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if any match, otherwise false.</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity with the given key exists.
    /// </summary>
    /// <param name="key">Primary key value.</param>
    /// <returns>True if exists, otherwise false.</returns>
    bool Exists<T>(T key);

    /// <summary>
    /// Asynchronously checks if an entity with the given key exists.
    /// </summary>
    /// <param name="key">Primary key value.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if exists, otherwise false.</returns>
    Task<bool> ExistsAsync<T>(T key, CancellationToken cancellationToken = default);

    #endregion

    #region Counting

    /// <summary>
    /// Counts all entities or entities matching a predicate.
    /// </summary>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <returns>Count of entities.</returns>
    int Count(Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// Asynchronously counts all entities or entities matching a predicate.
    /// </summary>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Count of entities.</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    #endregion

    #region Find by key

    /// <summary>
    /// Finds an entity by its primary key.
    /// </summary>
    /// <param name="key">Primary key value.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    TEntity? Find<TKey>(TKey key);

    /// <summary>
    /// Asynchronously finds an entity by its primary key.
    /// </summary>
    /// <param name="key">Primary key value.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    Task<TEntity?> FindAsync<TKey>(TKey key, CancellationToken cancellationToken = default);

    #endregion

    #region Read

    /// <summary>
    /// Gets a queryable of entities, with optional filtering, ordering, and including navigation properties.
    /// </summary>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="orderBy">Optional ordering expression.</param>
    /// <param name="include">Optional include navigation expression.</param>
    /// <param name="isReadOnly">If true, disables change tracking.</param>
    /// <returns>IQueryable for further composition.</returns>
    IQueryable<TEntity> GetQueryable(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false);

    /// <summary>
    /// Gets a list of entities, optionally filtered, ordered, and including navigation properties.
    /// </summary>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="orderBy">Optional ordering expression.</param>
    /// <param name="include">Optional include navigation expression.</param>
    /// <param name="isReadOnly">If true, disables change tracking.</param>
    /// <returns>List of entities.</returns>
    List<TEntity> GetList(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false);

    /// <summary>
    /// Asynchronously gets a list of entities, optionally filtered, ordered, and including navigation properties.
    /// </summary>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="orderBy">Optional ordering expression.</param>
    /// <param name="include">Optional include navigation expression.</param>
    /// <param name="isReadOnly">If true, disables change tracking.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>List of entities.</returns>
    Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity by filter, optionally including navigation properties.
    /// </summary>
    /// <param name="filter">Filter predicate (required).</param>
    /// <param name="include">Optional include navigation expression.</param>
    /// <param name="isReadOnly">If true, disables change tracking.</param>
    /// <returns>Single entity if found, otherwise null.</returns>
    TEntity? GetSingle(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false);

    /// <summary>
    /// Asynchronously gets a single entity by filter, optionally including navigation properties.
    /// </summary>
    /// <param name="filter">Filter predicate (required).</param>
    /// <param name="include">Optional include navigation expression.</param>
    /// <param name="isReadOnly">If true, disables change tracking.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Single entity if found, otherwise null.</returns>
    Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false,
        CancellationToken cancellationToken = default);

    #endregion

    #region Update

    /// <summary>
    /// Marks an entity as modified in the context.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Asynchronously marks an entity as modified in the context.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk updates entities using high-performance EFCore.BulkExtensions.
    /// </summary>
    /// <param name="entities">Entities to update.</param>
    /// <param name="bulkConfig">Optional bulk operation configuration.</param>
    void BulkUpdate(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null);

    /// <summary>
    /// Asynchronously bulk updates entities using high-performance EFCore.BulkExtensions.
    /// </summary>
    /// <param name="entities">Entities to update.</param>
    /// <param name="bulkConfig">Optional bulk operation configuration.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task BulkUpdateAsync(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null, CancellationToken cancellationToken = default);

    #endregion

    #region Delete

    /// <summary>
    /// Removes an entity from the context.
    /// </summary>
    /// <param name="entity">Entity to remove.</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Asynchronously removes an entity from the context.
    /// </summary>
    /// <param name="entity">Entity to remove.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple entities from the context.
    /// </summary>
    /// <param name="entities">Entities to remove.</param>
    void Delete(IEnumerable<TEntity> entities);

    /// <summary>
    /// Asynchronously removes multiple entities from the context.
    /// </summary>
    /// <param name="entities">Entities to remove.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk deletes entities using high-performance EFCore.BulkExtensions.
    /// </summary>
    /// <param name="entities">Entities to delete.</param>
    /// <param name="bulkConfig">Optional bulk operation configuration.</param>
    void BulkDelete(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null);

    /// <summary>
    /// Asynchronously bulk deletes entities using high-performance EFCore.BulkExtensions.
    /// </summary>
    /// <param name="entities">Entities to delete.</param>
    /// <param name="bulkConfig">Optional bulk operation configuration.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task BulkDeleteAsync(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null, CancellationToken cancellationToken = default);

    #endregion
}
