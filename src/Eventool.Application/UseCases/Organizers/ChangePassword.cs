using Eventool.Domain.Organizers;
using Eventool.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace Eventool.Application.UseCases;

public record ChangePasswordRequest(
    string OldPassword,
    string NewPassword,
    Guid OrganizerId
) : IRequest<Organizer>;

public class ChangePasswordRequestHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IValidator<ChangePasswordRequest> validator
) : IRequestHandler<ChangePasswordRequest, Organizer>
{
    public async Task<Organizer> Handle(ChangePasswordRequest request, CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteAndCommitAsync(action: async repositories =>
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            
            var organizer = await repositories
                .OrganizersRepository
                .GetByIdAsync(request.OrganizerId, cancellationToken);

            var oldPasswordIsSame = passwordHasher.IsSame(request.OldPassword, organizer.HashedPassword);

            if (!oldPasswordIsSame)
                throw new WrongPasswordException();
            
            organizer.ChangePassword(passwordHasher.Hash(request.NewPassword));
            
            repositories.OrganizersRepository.Save(organizer);

            return organizer;
        }, cancellationToken);
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .WithMessage("Новый пароль должен быть заполнен!")
            .Length(6, 100)
            .WithMessage("Новый пароль должен содержать от 6 до 100 символов!")
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).*$")
            .WithMessage("Новый пароль должен содержать хотя бы одну строчную и заглавную латинскую букву, а также цифру!");
    }
}

