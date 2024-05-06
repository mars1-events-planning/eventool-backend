using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using MediatR;
using UnauthorizedAccessException = Eventool.Application.Utility.UnauthorizedAccessException;

namespace Eventool.Application.UseCases;

public record DeleteChecklistRequest(
    Guid OrganizerId,
    Guid EventId,
    Guid ChecklistId
) : IRequest<Event>;

public class DeleteChecklistRequestHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<DeleteChecklistRequest, Event>
{
    public async Task<Event> Handle(DeleteChecklistRequest request, CancellationToken cancellationToken) =>
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

            var checklistToRemove = @event.Checklists.FirstOrDefault(x => x.Id == request.ChecklistId)
                ?? throw new InvalidOperationException("Список задач не найден")
            {
                Data = { ["eventId"] = request.EventId, ["checklistId"] = request.ChecklistId }
            };
            @event.RemoveChecklist(checklistToRemove);
            
            repositories.EventRepository.Save(@event);

            return @event;
        }, cancellationToken);
}