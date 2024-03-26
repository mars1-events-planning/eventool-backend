using Eventool.Domain.Common;

namespace Eventool.Infrastructure.Persistence;

public interface IDocument<out TThis, TDomainObject>
{
    TDomainObject ToDomainObject();

    public static abstract TThis Create(TDomainObject domainObject);
}