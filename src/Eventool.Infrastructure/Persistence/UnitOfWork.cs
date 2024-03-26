using Marten;

namespace Eventool.Infrastructure.Persistence;

public class UnitOfWork(IDocumentStore store) : IUnitOfWork
{
    public async Task<TResult> ExecuteAndCommitAsync<TResult>(
        Func<IRepositoryRegistry, Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        await using var session = await store.LightweightSerializableSessionAsync(cancellationToken);
        await session.BeginTransactionAsync(cancellationToken);
        var result = await action(new RepositoryRegistry(session));
        await session.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task ExecuteAndCommitAsync(Func<IRepositoryRegistry, Task> action, CancellationToken cancellationToken)
    {
        await using var session = await store.LightweightSerializableSessionAsync(cancellationToken);
        await session.BeginTransactionAsync(cancellationToken);
        await action(new RepositoryRegistry(session));
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task<TResult> ExecuteReadOnlyAsync<TResult>(
        Func<IRepositoryRegistry, Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        await using var session = await store.LightweightSerializableSessionAsync(cancellationToken);
        await session.BeginTransactionAsync(cancellationToken);
        var result = await action(new RepositoryRegistry(session));
        await session.SaveChangesAsync(cancellationToken);
        return result;
    }
}