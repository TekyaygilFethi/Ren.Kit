using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ren.Kit.DataKit.Abstractions;
using System.Data;

namespace Ren.Kit.DataKit.Services;

/// <summary>
///     Represents a unit of work for managing database transactions and repositories in a generic context.
/// </summary>
/// <typeparam name="RenDbContext">The type of the database context.</typeparam>
public class RENUnitOfWork<TDbContext> : IRENUnitOfWork<TDbContext> where TDbContext : DbContext
{
    protected readonly TDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    /// <inheritdoc/>
    public bool IsInTransaction => _currentTransaction != null;

    /// <inheritdoc/>
    public RENUnitOfWork(TDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public virtual void SaveChanges(bool useTransaction = false)
    {
        EnsureNotDisposed();

        if (IsInTransaction)
        {
            _context.SaveChanges();
            return;
        }

        if (!useTransaction)
        {
            _context.SaveChanges();
            return;
        }

        using var transaction = _context.Database.BeginTransaction();
        try
        {
            _context.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task SaveChangesAsync(bool useTransaction = false, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        if (IsInTransaction)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (!useTransaction)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual IRENRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        EnsureNotDisposed();
        return (IRENRepository<TEntity>)Activator.CreateInstance(typeof(RENRepository<TEntity>), _context)
            ?? throw new InvalidOperationException($"Could not create repository for type {typeof(TEntity).Name}");
    }

    /// <inheritdoc/>
    public virtual TRepository GetRepository<TRepository, TEntity>()
        where TEntity : class
        where TRepository : IRENRepository<TEntity>
    {
        EnsureNotDisposed();
        return (TRepository)Activator.CreateInstance(typeof(TRepository), _context)
            ?? throw new InvalidOperationException($"Could not create repository for type {typeof(TEntity).Name}");
    }

    /// <inheritdoc/>
    public virtual Task<IRENRepository<TEntity>> GetRepositoryAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetRepository<TEntity>());
    }

    /// <inheritdoc/>
    public virtual Task<TRepository> GetRepositoryAsync<TRepository, TEntity>(
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TRepository : IRENRepository<TEntity>
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetRepository<TRepository, TEntity>());
    }


    /// <inheritdoc/>
    public virtual void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        EnsureNotDisposed();
        if (_currentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _currentTransaction = _context.Database.BeginTransaction(isolationLevel);
    }

    /// <inheritdoc/>
    public virtual async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        if (_currentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual void CommitTransaction()
    {
        EnsureNotDisposed();
        if (_currentTransaction == null)
            throw new InvalidOperationException("There is no transaction to commit.");

        _currentTransaction.Commit();
        _currentTransaction.Dispose();
        _currentTransaction = null;
    }

    /// <inheritdoc/>
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        if (_currentTransaction == null)
            throw new InvalidOperationException("There is no transaction to commit.");

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    /// <inheritdoc/>
    public virtual void RollbackTransaction()
    {
        EnsureNotDisposed();
        if (_currentTransaction == null)
            throw new InvalidOperationException("There is no transaction to rollback.");

        _currentTransaction.Rollback();
        _currentTransaction.Dispose();
        _currentTransaction = null;
    }

    /// <inheritdoc/>
    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        if (_currentTransaction == null)
            throw new InvalidOperationException("There is no transaction to rollback.");

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose pattern for correct resource management.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Throws if object has been disposed.
    /// </summary>
    protected void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RENUnitOfWork<TDbContext>));
    }
}
