using Eventool.Domain.Common;
using Marten;

namespace Eventool.Infrastructure.Persistence;

public abstract class Repository<TId, TEntity, TDocument>(IDocumentSession session)
    where TId : struct
    where TEntity : Entity<TId>, IAggregateRoot
    where TDocument : IDocument<TDocument, TEntity>
{
    protected IDocumentSession Session { get; } = session;
}