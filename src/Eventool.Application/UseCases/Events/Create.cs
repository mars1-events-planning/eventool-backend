using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace Eventool.Application.UseCases;

public record CreateEventCommand(
    string Title,
    Guid CreatorId
) : IRequest<Event>;

public class CreateEventHandler(
    IUnitOfWork unitOfWork,
    IValidator<CreateEventCommand> validator
) : IRequestHandler<CreateEventCommand, Event>
{
    public async Task<Event> Handle(CreateEventCommand request, CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteAndCommitAsync(action: async repositories =>
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            await repositories.OrganizersRepository.EnsureOrganizerExistsAsync(request.CreatorId, cancellationToken);
            
            var @event = new Event(Guid.NewGuid(), request.CreatorId, DateTime.UtcNow, request.Title);
            return repositories.EventRepository.Save(@event);
        }, cancellationToken);
}

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Название мероприятия должно быть заполнено!")
            .Length(2, 100)
            .WithMessage("Название мероприятия должно содержать от 2 до 100 символов!");
    }
}