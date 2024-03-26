using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace Eventool.Application.UseCases;

public record SaveChecklistRequest(
    Guid EventId,
    Guid OrganizerId,
    string Title,
    IEnumerable<ChecklistItem> Items,
    Guid? ChecklistId = null
) : IRequest<Event>;

public class SaveChecklistHandler(
    IValidator<SaveChecklistRequest> validator,
    IUnitOfWork unitOfWork) : IRequestHandler<SaveChecklistRequest, Event>
{
    public async Task<Event> Handle(SaveChecklistRequest request, CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteAndCommitAsync(async repositories =>
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            
            var @event = await repositories.EventRepository.GetByIdAsync(request.EventId, cancellationToken);
            if (@event is null)
                throw new InvalidOperationException("Событие не найдено")
                {
                    Data = { ["eventId"] = request.EventId }
                };

            if (@event.CreatorId != request.OrganizerId)
                throw new UnauthorizedAccessException();

            if (request.ChecklistId.HasValue &&
                @event.Checklists.FirstOrDefault(x => x.Id == request.ChecklistId.Value) is { } foundedChecklist)
            {
                foundedChecklist.SetTitle(request.Title);
                foundedChecklist.SetItems(request.Items);
            }
            else
            {
                var checklist = new Checklist(Guid.NewGuid(), request.Title);
                checklist.SetItems(request.Items);
                @event.AddChecklist(checklist);
            }

            repositories.EventRepository.Save(@event);

            return @event;
        }, cancellationToken);
}

public class SaveChecklistRequestValidator : AbstractValidator<SaveChecklistRequest>
{
    public SaveChecklistRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(2, 80)
            .WithMessage("Название должно быть от 2 до 80 символов");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Список элементов не может быть пустым");
        
        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.Title)
                    .NotEmpty()
                    .Length(2, 80)
                    .WithMessage("Название элемента не может быть пустым");

                item.RuleFor(x => x.Done)
                    .NotNull()
                    .WithMessage("Поле \"done\" должно быть заполнено");
            });
    }
}