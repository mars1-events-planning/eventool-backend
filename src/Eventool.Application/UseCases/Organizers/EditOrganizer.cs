using Eventool.Domain.Organizers;
using Eventool.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace Eventool.Application.UseCases;

public record EditOrganizerRequest(
    Guid OrganizerId,
    string? Fullname,
    string? Username
) : IRequest<Organizer>;

public class EditOrganizerRequestHandler(
    IUnitOfWork unitOfWork,
    IValidator<EditOrganizerRequest> validator)
    : IRequestHandler<EditOrganizerRequest, Organizer>
{
    public async Task<Organizer> Handle(EditOrganizerRequest request, CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteAndCommitAsync(action: async repositories =>
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var organizer = await repositories
                .OrganizersRepository
                .GetByIdAsync(request.OrganizerId, cancellationToken);

            if (request.Fullname is not null)
                organizer.ChangeFullname(request.Fullname);
            
            if (request.Username is not null)
                organizer.ChangeUsername(request.Username);

            repositories.OrganizersRepository.Save(organizer);

            return organizer;
        }, cancellationToken);
}

public class EditOrganizerRequestValidator : AbstractValidator<EditOrganizerRequest>
{
    public EditOrganizerRequestValidator(IUnitOfWork unitOfWork)
    {
        When(c => c.Fullname is not null, () =>
            RuleFor(c => c.Fullname)
                .NotEmpty()
                .WithMessage("Поле \"Полное имя\" должно быть заполнено!")
                .Length(2, 100)
                .WithMessage("Поле \"Полное имя\" должно содержать от 2 до 100 символов!"));

        When(c => c.Username is not null, () =>
            RuleFor(c => c.Username)
                .NotEmpty()
                .WithMessage("Имя пользователя должно быть заполнено!")
                .Matches("^[a-zA-Z0-9_-]*$")
                .WithMessage("Имя пользователя должно содержать только латинские буквы, цифры, символы \"_\" и \"-\"!")
                .Length(2, 100)
                .WithMessage("Имя пользователя должно содержать от 2 до 100 символов!")
                .MustAsync(async (x, _) => !await UsernameTaken(x))
                .WithMessage("Имя пользователя должно быть уникально!"));
        return;

        async Task<bool> UsernameTaken(string username) => await unitOfWork.ExecuteReadOnlyAsync(
            async repo => await repo.OrganizersRepository.UsernameTakenAsync(username, CancellationToken.None),
            CancellationToken.None);
    }
}