using Eventool.Domain.Common;
using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace Eventool.Application.UseCases;

public record EditEventRequest(
    Guid EventId,
    Guid OrganizerId,
    string Title,
    string? Address = null,
    string? Description = null,
    DateTime? StartDateTimeUtc = null
) : IRequest<Event>;

public class EditEventHandler(
    IValidator<EditEventRequest> validator,
    IUnitOfWork unitOfWork) : IRequestHandler<EditEventRequest, Event>
{
    public async Task<Event> Handle(EditEventRequest request, CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteAndCommitAsync(async repositories =>
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var @event = await repositories
                .EventRepository
                .GetByIdAsync(request.EventId, cancellationToken);

            if (@event is null)
                throw new InvalidOperationException("Событие не найдено")
                {
                    Data = { ["eventId"] = request.EventId }
                };

            if (@event.CreatorId != request.OrganizerId)
                throw new UnauthorizedAccessException();

            @event.SetTitle(request.Title);
            @event.SetAddress(request.Address);
            @event.SetDescription(request.Description);
            @event.SetStartAtUtc(request.StartDateTimeUtc?.ToUniversalTime());

            repositories.EventRepository.Save(@event);

            return @event;
        }, cancellationToken);
}

public class EditEventRequestValidator : AbstractValidator<EditEventRequest>
{
    public EditEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(2, 80)
            .WithMessage("Название должно быть от 2 до 80 символов");

        When(x => x.Description is not null and not "", () =>
        {
            RuleFor(x => x.Description)
                .Length(2, 500)
                .WithMessage("Описание должно быть от 2 до 500 символов");
        });

        When(x => x.Address is not null and not "", () =>
        {
            RuleFor(x => x.Address)
                .Length(2, 150)
                .WithMessage("Адрес должен быть от 2 до 500 символов");
        });

        When(x => x.StartDateTimeUtc is not null, () =>
        {
            RuleFor(x => x.StartDateTimeUtc)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Начало мероприятия должно быть в будущем");
        });
    }
}

public class UnauthorizedAccessException() : DomainException("У пользователя нет доступа к данному ресурсу");