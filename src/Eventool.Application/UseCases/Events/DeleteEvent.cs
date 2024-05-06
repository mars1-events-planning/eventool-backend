using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using MediatR;
using UnauthorizedAccessException = Eventool.Application.Utility.UnauthorizedAccessException;

namespace Eventool.Application.UseCases;

public record DeleteEventRequest(
    Guid OrganizerId,
    Guid EventId
) : IRequest<Event>;

public class DeleteEventRequestHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<DeleteEventRequest, Event>
{
    public async Task<Event> Handle(DeleteEventRequest request, CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteAndCommitAsync(async repositories =>
        {
            await repositories.OrganizersRepository.EnsureOrganizerExistsAsync(request.OrganizerId, cancellationToken);
            var @event = await repositories.EventRepository.GetByIdAsync(request.EventId, cancellationToken)
                ??  throw new InvalidOperationException("Событие не найдено")
                {
                    Data = { ["eventId"] = request.EventId }
                };

            if (@event.CreatorId != request.OrganizerId)
                throw new UnauthorizedAccessException();
            
            repositories.EventRepository.Remove(@event);

            return @event;
        }, cancellationToken);
}