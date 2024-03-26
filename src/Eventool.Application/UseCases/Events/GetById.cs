using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using MediatR;

namespace Eventool.Application.UseCases;

public record GetByIdRequest(
    Guid EventId,
    Guid RequestingOrganizerId
) : IRequest<Event?>;

public class GetByIdRequestHandler(IUnitOfWork uow) : IRequestHandler<GetByIdRequest, Event?>
{
    public async Task<Event?> Handle(GetByIdRequest request, CancellationToken cancellationToken) =>
        await uow.ExecuteReadOnlyAsync(async repositories =>
        {
            var @event = await repositories.EventRepository.GetByIdAsync(request.EventId, cancellationToken);
            if (@event is not null && @event.CreatorId != request.RequestingOrganizerId)
                return null;

            return @event;
        }, cancellationToken);
}