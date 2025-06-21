using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Ren.Kit.DataKit.Abstractions;
using System.Linq.Expressions;

namespace Ren.Kit.DataKit.Services;

/// <summary>
///     Generic repository implementation for EF Core. 
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class RENRepository<TEntity> : IRENRepository<TEntity> where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <inheritdoc/>
    public RENRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    #region Create

    /// <inheritdoc/>
    public virtual void Insert(TEntity entity)
        => _dbSet.Add(entity);

    /// <inheritdoc/>
    public virtual Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _dbSet.AddAsync(entity, cancellationToken).AsTask();

    /// <inheritdoc/>
    public virtual void Insert(IEnumerable<TEntity> entities)
        => _dbSet.AddRange(entities);

    /// <inheritdoc/>
    public virtual Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => _dbSet.AddRangeAsync(entities, cancellationToken);

    /// <inheritdoc/>
    public virtual void BulkInsert(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null)
        => _context.BulkInsert(entities, bulkConfig ?? new BulkConfig());

    /// <inheritdoc/>
    public virtual Task BulkInsertAsync(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null, CancellationToken cancellationToken = default)
        => _context.BulkInsertAsync(entities, bulkConfig ?? new BulkConfig(), cancellationToken: cancellationToken);

    #endregion

    #region Existence Checks

    /// <inheritdoc/>
    public virtual bool Any(Expression<Func<TEntity, bool>>? predicate = null)
        => predicate == null ? _dbSet.Any() : _dbSet.Any(predicate);

    /// <inheritdoc/>
    public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate == null
            ? _dbSet.AnyAsync(cancellationToken)
            : _dbSet.AnyAsync(predicate, cancellationToken);

    /// <inheritdoc/>
    public virtual bool Exists<TKey>(TKey key)
        => _dbSet.Find(key) != null;

    /// <inheritdoc/>
    public virtual async Task<bool> ExistsAsync<TKey>(TKey key, CancellationToken cancellationToken = default)
        => (await _dbSet.FindAsync([key], cancellationToken)) != null;

    #endregion

    #region Counting

    /// <inheritdoc/>
    public virtual int Count(Expression<Func<TEntity, bool>>? predicate = null)
        => predicate == null ? _dbSet.Count() : _dbSet.Count(predicate);

    /// <inheritdoc/>
    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate == null ? _dbSet.CountAsync(cancellationToken) : _dbSet.CountAsync(predicate, cancellationToken);

    #endregion

    #region Find by key

    /// <inheritdoc/>
    public virtual TEntity? Find<TKey>(TKey key)
        => _dbSet.Find(key);

    /// <inheritdoc/>
    public virtual async Task<TEntity?> FindAsync<TKey>(TKey key, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([key], cancellationToken);

    #endregion

    #region Read

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> GetQueryable(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (include != null)
            query = include(query);

        if (isReadOnly)
            query = query.AsNoTracking();

        if (filter != null)
            query = query.Where(filter);

        return orderBy != null ? orderBy(query) : query;
    }

    /// <inheritdoc/>
    public virtual List<TEntity> GetList(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false)
    {
        return GetQueryable(filter, orderBy, include, isReadOnly).ToList();
    }

    /// <inheritdoc/>
    public virtual Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(filter, orderBy, include, isReadOnly).ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual TEntity? GetSingle(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false)
    {
        return GetQueryable(filter, null, include, isReadOnly).SingleOrDefault();
    }

    /// <inheritdoc/>
    public virtual Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(filter, null, include, isReadOnly).SingleOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region Update

    /// <inheritdoc/>
    public virtual void Update(TEntity entity)
        => _context.Entry(entity).State = EntityState.Modified;

    /// <inheritdoc/>
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Bu işlem EF Core'da sync, awaitable bir şey yok.
        _context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual void BulkUpdate(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null)
        => _context.BulkUpdate(entities, bulkConfig ?? new BulkConfig());

    /// <inheritdoc/>
    public virtual Task BulkUpdateAsync(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null, CancellationToken cancellationToken = default)
        => _context.BulkUpdateAsync(entities, bulkConfig ?? new BulkConfig(), cancellationToken: cancellationToken);

    #endregion

    #region Delete

    /// <inheritdoc/>
    public virtual void Delete(TEntity entity)
        => _dbSet.Remove(entity);

    /// <inheritdoc/>
    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual void Delete(IEnumerable<TEntity> entities)
        => _dbSet.RemoveRange(entities);

    /// <inheritdoc/>
    public virtual Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual void BulkDelete(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null)
        => _context.BulkDelete(entities, bulkConfig ?? new BulkConfig());

    /// <inheritdoc/>
    public virtual Task BulkDeleteAsync(IEnumerable<TEntity> entities, BulkConfig? bulkConfig = null, CancellationToken cancellationToken = default)
        => _context.BulkDeleteAsync(entities, bulkConfig ?? new BulkConfig(), cancellationToken: cancellationToken);

    #endregion
}
