using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Ren.Kit.DataKit.Abstractions;

/// <summary>
/// Defines a contract for managing database transactions and coordinating multiple repositories
/// as a single unit of work within a specific <typeparamref name="RenDbContext"/>.
/// </summary>
/// <typeparam name="RenDbContext">The Entity Framework DbContext type.</typeparam>
public interface IRENUnitOfWork<RenDbContext> : IDisposable where RenDbContext : DbContext
{
    /// <summary>
    /// Indicates if a transaction is currently in progress.
    /// </summary>
    bool IsInTransaction { get; }

    /// <summary>
    /// Commits all changes made in this unit of work to the database.
    /// </summary>
    void SaveChanges(bool useTransaction = false);

    /// <summary>
    /// Asynchronously commits all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="useTransaction">Whether to wrap the operation in an explicit transaction (default: false).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task SaveChangesAsync(bool useTransaction = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a repository instance for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A repository instance for <typeparamref name="TEntity"/>.</returns>
    IRENRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    /// <summary>
    /// Gets a custom repository instance for the specified entity and repository types.
    /// </summary>
    /// <typeparam name="TRepository">The custom repository type implementing <see cref="IRENRepository{TEntity}"/>.</typeparam>
    /// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
    /// <returns>An instance of <typeparamref name="TRepository"/> for <typeparamref name="TEntity"/>.</returns>
    TRepository GetRepository<TRepository, TEntity>()
        where TEntity : class
        where TRepository : IRENRepository<TEntity>;

    /// <summary>
    /// Asynchronously retrieves a repository instance for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A Task representing the repository instance for <typeparamref name="TEntity"/>.</returns>
    Task<IRENRepository<TEntity>> GetRepositoryAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Asynchronously gets a custom repository instance for the specified entity and repository types.
    /// </summary>
    /// <typeparam name="TRepository">The custom repository type implementing <see cref="IRENRepository{TEntity}"/>.</typeparam>
    /// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with an instance of <typeparamref name="TRepository"/> for <typeparamref name="TEntity"/>.</returns>
    Task<TRepository> GetRepositoryAsync<TRepository, TEntity>(CancellationToken cancellationToken = default)
        where TEntity : class
        where TRepository : IRENRepository<TEntity>;

    /// <summary>
    /// Begins a database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The desired transaction isolation level.</param>
    void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    /// <summary>
    /// Asynchronously begins a database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The desired transaction isolation level.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    void CommitTransaction();

    /// <summary>
    /// Asynchronously commits the current database transaction.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    void RollbackTransaction();

    /// <summary>
    /// Asynchronously rolls back the current database transaction.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
