using Eventool.Application.Utility;
using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using UnauthorizedAccessException = Eventool.Application.Utility.UnauthorizedAccessException;

namespace Eventool.Application.UseCases;

public record EventChanges(
    Optional<Guid?> EventId,
    Optional<string?> Title,
    Optional<string?> Description,
    Optional<string?> Address,
    Optional<DateTime?> StartDateTimeUtc,
    Optional<IEnumerable<ChecklistChanges>> Checklists,
    Optional<IEnumerable<GuestChanges>> Guests);

public record GuestChanges(
    Optional<Guid?> Id,
    string Name,
    string Contact,
    IEnumerable<string> Tags);

public record ChecklistChanges(
    Optional<Guid?> ChecklistId,
    string Title,
    IEnumerable<ChecklistItem> ChecklistItems);

public record SaveEventRequest(
    Guid OrganizerId,
    EventChanges EventChanges
) : IRequest<Event>;

public class SaveEventRequestHandler(
    IValidator<SaveEventRequest> validator,
    IUnitOfWork unitOfWork
) : IRequestHandler<SaveEventRequest, Event>
{
    public async Task<Event> Handle(SaveEventRequest request, CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteAndCommitAsync(async repositories =>
        {
            var organizers = repositories.OrganizersRepository;
            var events = repositories.EventRepository;

            await organizers.EnsureOrganizerExistsAsync(request.OrganizerId, cancellationToken);

            await validator.ValidateAndThrowAsync(request, cancellationToken);

            Event? @event = null!;
            if (request.EventChanges.EventId.IsSet)
            {
                @event = await events.GetByIdAsync(request.EventChanges.EventId.Value!.Value, cancellationToken);

                if (@event is null)
                    throw new InvalidOperationException("Событие не найдено")
                    {
                        Data = { ["eventId"] = request.EventChanges.EventId.Value }
                    };

                if (@event.CreatorId != request.OrganizerId)
                    throw new UnauthorizedAccessException();

                request.EventChanges.Title.IfSet(v => @event.SetTitle(v!));
            }
            else
            {
                @event = new Event(
                    Guid.NewGuid(),
                    request.OrganizerId,
                    DateTime.UtcNow,
                    request.EventChanges.Title.Value!);
            }

            request.EventChanges.Address.IfSet(v => @event.SetAddress(v));
            request.EventChanges.Description.IfSet(v => @event.SetDescription(v));
            request.EventChanges.StartDateTimeUtc.IfSet(v => @event.SetStartAtUtc(v));
            request.EventChanges.Checklists.IfSet(checklistsChanges => ProcessCollectionChanges(
                changes: checklistsChanges,
                isOld: x => x.ChecklistId is { IsSet: true, Value: not null },
                isNew: x => x.ChecklistId.IsSet == false ||
                            (x.ChecklistId.IsSet && x.ChecklistId.Value.HasValue == false),
                processOldEntity: oldChecklist =>
                {
                    var checklist = new Checklist(id: oldChecklist.ChecklistId.Value!.Value, title: oldChecklist.Title);
                    checklist.SetItems(oldChecklist.ChecklistItems);

                    var deprecatedChecklist =
                        @event.Checklists.FirstOrDefault(x => x.Id == oldChecklist.ChecklistId.Value);

                    if (deprecatedChecklist is null)
                        throw new InvalidOperationException("No checklist with such ID in event");

                    @event.RemoveChecklist(deprecatedChecklist);
                    @event.AddChecklist(checklist);
                },
                processNewEntity: newChecklist =>
                {
                    var checklist = new Checklist(id: Guid.NewGuid(), title: newChecklist.Title);
                    checklist.SetItems(newChecklist.ChecklistItems);

                    @event.AddChecklist(checklist);
                }));
            request.EventChanges.Guests.IfSet(guestsChanges => ProcessCollectionChanges(
                changes: guestsChanges,
                isOld: x => x.Id is { IsSet: true, Value: not null },
                isNew: x => x.Id.IsSet == false || (x.Id.IsSet && x.Id.Value.HasValue == false),
                processOldEntity: oldGuest =>
                {
                    var guest = new Guest(id: oldGuest.Id.Value!.Value, name: oldGuest.Name, contact: oldGuest.Contact)
                    {
                        Tags = oldGuest.Tags
                    };

                    var deprecatedGuest =
                        @event.Guests.FirstOrDefault(x => x.Id == oldGuest.Id.Value) ??
                        throw new InvalidOperationException("No guest with such ID in event");

                    @event.RemoveGuest(deprecatedGuest);
                    @event.AddGuest(guest);
                },
                processNewEntity: newGuest =>
                {
                    var guest = new Guest(id: Guid.NewGuid(), name: newGuest.Name, contact: newGuest.Contact)
                    {
                        Tags = newGuest.Tags
                    };

                    @event.AddGuest(guest);
                }));

            events.Save(@event);

            return @event;
        }, cancellationToken);


    private static void ProcessCollectionChanges<TChanges>(
        IEnumerable<TChanges> changes,
        Func<TChanges, bool> isOld,
        Func<TChanges, bool> isNew,
        Action<TChanges> processNewEntity,
        Action<TChanges> processOldEntity)
    {
        var enumeratedChanges = changes.ToList();
        var oldEntities = enumeratedChanges.Where(isOld);
        var newEntities = enumeratedChanges.Where(isNew);

        foreach (var newEntity in newEntities)
            processNewEntity(newEntity);

        foreach (var oldEntity in oldEntities)
            processOldEntity(oldEntity);
    }
}

public class SaveEventRequestValidator : AbstractValidator<SaveEventRequest>
{
    public SaveEventRequestValidator()
    {
        RuleFor(x => x.OrganizerId).NotEmpty();

        this
            .When(x => x.EventChanges.EventId.IsSet, () =>
            {
                RuleFor(x => x.EventChanges.EventId.Value)
                    .NotEmpty()
                    .WithMessage("Идентификатор мероприятия должен быть заполнен!");

                When(x => x.EventChanges.Title.IsSet, ValidateEventTitle);
                ValidateDescription();
                ValidateAddress();
                ValidateEventPeriod();
                ValidateChecklists();
            })
            .Otherwise(() =>
            {
                ValidateEventTitle();
                ValidateDescription();
                ValidateAddress();
                ValidateEventPeriod();
                ValidateChecklists();
            });
    }

    private void ValidateEventPeriod()
    {
        When(x => x.EventChanges.StartDateTimeUtc is { IsSet: true, Value: not null }, () =>
        {
            RuleFor(x => x.EventChanges.StartDateTimeUtc.Value)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Время начала мероприятия должно быть в будущем");
        });
    }

    private void ValidateAddress()
    {
        When(x => x.EventChanges.Address is { IsSet: true, Value: not null and not "" }, () =>
        {
            RuleFor(x => x.EventChanges.Address.Value)
                .Length(2, 150)
                .WithMessage("Адрес должен быть от 2 до 500 символов");
        });
    }

    private void ValidateDescription()
    {
        When(x => x.EventChanges.Description is { IsSet: true, Value: not null and not "" }, () =>
        {
            RuleFor(x => x.EventChanges.Description.Value)
                .Length(2, 500)
                .WithMessage("Описание должно быть от 2 до 500 символов");
        });
    }

    private void ValidateEventTitle()
    {
        RuleFor(x => x.EventChanges.Title)
            .Must(x => x.IsSet)
            .WithMessage("Значение для поля \"Название\" должно быть указано");

        When(x => x.EventChanges.EventId.IsSet, () =>
        {
            RuleFor(x => x.EventChanges.Title.Value)
                .NotEmpty()
                .WithMessage("Название мероприятия должно быть заполнено!")
                .Length(2, 100)
                .WithMessage("Название мероприятия должно содержать от 2 до 100 символов!");
        });
    }

    private void ValidateChecklists()
    {
        When(x => x.EventChanges.Checklists.IsSet, () =>
        {
            RuleForEach(x => x.EventChanges.Checklists.Value).ChildRules(checklist =>
            {
                checklist
                    .RuleFor(x => x.Title)
                    .NotEmpty()
                    .WithMessage("Название чеклиста должно быть заполнено!")
                    .Length(2, 100)
                    .WithMessage("Название чеклиста должно содержать от 2 до 100 символов!");

                checklist.RuleForEach(x => x.ChecklistItems).ChildRules(item =>
                {
                    item.RuleFor(x => x.Title)
                        .NotEmpty()
                        .WithMessage("Название пункта чеклиста должно быть заполнено!")
                        .Length(2, 100)
                        .WithMessage("Название пункта чеклиста должно содержать от 2 до 100 символов!");

                    item.RuleFor(x => x.Done).NotNull();
                });
            });
        });
    }
}