namespace Eventool.Infrastructure.Persistence;

public interface IUnitOfWork
{
    public Task<TResult> ExecuteAndCommitAsync<TResult>(
        Func<IRepositoryRegistry, Task<TResult>> action,
        CancellationToken cancellationToken);
    
    public Task<TResult> ExecuteReadOnlyAsync<TResult>(
        Func<IRepositoryRegistry, Task<TResult>> action,
        CancellationToken cancellationToken);
}