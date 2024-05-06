using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using MediatR;
using UnauthorizedAccessException = Eventool.Application.Utility.UnauthorizedAccessException;

namespace Eventool.Application.UseCases;

public record DeleteGuestRequest(
    Guid OrganizerId,
    Guid EventId,
    Guid GuestId
) : IRequest<Event>;

public class DeleteGuestRequestHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<DeleteGuestRequest, Event>
{
    public async Task<Event> Handle(DeleteGuestRequest request, CancellationToken cancellationToken) =>
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

            var guestToRemove = @event.Guests.FirstOrDefault(x => x.Id == request.GuestId)
                ?? throw new InvalidOperationException("Гость не найден")
            {
                Data = { ["eventId"] = request.EventId, ["guestId"] = request.GuestId }
            };
                
            @event.RemoveGuest(guestToRemove);
            
            repositories.EventRepository.Save(@event);

            return @event;
        }, cancellationToken);
}