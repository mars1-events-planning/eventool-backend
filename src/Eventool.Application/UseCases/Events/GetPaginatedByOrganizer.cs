using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using MediatR;

namespace Eventool.Application.UseCases;

public record GetEventsByOrganizerRequest(Guid OrganizerId, int PageNumber)
    : IRequest<IEnumerable<Event>>;

public class GetEventsByOrganizerRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetEventsByOrganizerRequest, IEnumerable<Event>>
{
    public async Task<IEnumerable<Event>> Handle(
        GetEventsByOrganizerRequest request,
        CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteReadOnlyAsync(async repositories =>
        {
            await repositories.OrganizersRepository.EnsureOrganizerExistsAsync(request.OrganizerId, cancellationToken);
            return await repositories.EventRepository.GetListByCreatorIdAsync(
                request.OrganizerId,
                request.PageNumber,
                cancellationToken);
        }, cancellationToken);
}