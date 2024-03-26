using Eventool.Domain.Organizers;
using Eventool.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace Eventool.Application.UseCases;

public record RegisterOrganizerRequest(
    string Username,
    string FullName,
    string Password
) : IRequest<Organizer>;

public class RegisterOrganizerHandler(
    IUnitOfWork unitOfWork,
    IValidator<RegisterOrganizerRequest> validator,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterOrganizerRequest, Organizer>
{
    public async Task<Organizer> Handle(
        RegisterOrganizerRequest request,
        CancellationToken cancellationToken)
        => await unitOfWork.ExecuteAndCommitAsync<Organizer>(async repositories =>
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var organizer = new Organizer(
                id: Guid.NewGuid(),
                request.FullName,
                request.Username,
                passwordHasher.Hash(request.Password));
            repositories.OrganizersRepository.Save(organizer);
            return organizer;
        }, cancellationToken);
}

public class RegisterOrganizerRequestValidator : AbstractValidator<RegisterOrganizerRequest>
{
    public RegisterOrganizerRequestValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(c => c.Username)
            .NotEmpty()
            .WithMessage("Имя пользователя должно быть заполнено!")
            .Matches("^[a-zA-Z0-9_-]*$")
            .WithMessage("Имя пользователя должно содержать только латинские буквы, цифры, символы \"_\" и \"-\"!")
            .Length(2, 100)
            .WithMessage("Имя пользователя должно содержать от 2 до 100 символов!")
            .MustAsync(async (x, _) => !await UsernameTaken(x))
            .WithMessage("Имя пользователя должно быть уникально!");

        RuleFor(c => c.FullName)
            .NotEmpty()
            .WithMessage("Поле \"Полное имя\" должно быть заполнено!")
            .Length(2, 100)
            .WithMessage("Поле \"Полное имя\" должно содержать от 2 до 100 символов!");

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("Пароль должен быть заполнен!")
            .Length(6, 100)
            .WithMessage("Пароль должен содержать от 6 до 100 символов!")
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).*$")
            .WithMessage("Пароль должен содержать хотя бы одну строчную и заглавную латинскую букву, а также цифру!");
        return;

        async Task<bool> UsernameTaken(string username) => await unitOfWork.ExecuteReadOnlyAsync(
            async repo => await repo.OrganizersRepository.UsernameTakenAsync(username, CancellationToken.None),
            CancellationToken.None);
    }
}