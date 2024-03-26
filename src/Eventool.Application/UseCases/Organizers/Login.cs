using Eventool.Domain.Common;
using Eventool.Domain.Organizers;
using Eventool.Infrastructure.Persistence;
using MediatR;

namespace Eventool.Application.UseCases;

public record LoginOrganizerRequest(
    string Username,
    string Password
) : IRequest<string>;

public class LoginOrganizerHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAuthenticationTokenGenerator authenticationTokenGenerator)
    : IRequestHandler<LoginOrganizerRequest, string>
{
    public Task<string> Handle(LoginOrganizerRequest request, CancellationToken cancellationToken) =>
        unitOfWork.ExecuteAndCommitAsync(action: async repositories =>
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new UserNameShouldBeFilledException();
            
            var organizer = await repositories
                .OrganizersRepository
                .GetByUsernameAsync(request.Username, cancellationToken);

            if (organizer is null)
                throw new UserNotFoundByUsernameException(request.Username);

            if (!passwordHasher.IsSame(request.Password, organizer.HashedPassword))
                throw new WrongPasswordException();

            return authenticationTokenGenerator.Generate(organizer);
        }, cancellationToken);
}

public class UserNameShouldBeFilledException()
    : DomainException("Имя пользователя должно быть заполнено!");

public class UserNotFoundByUsernameException(string username)
    : DomainException($"""Пользователь "{username}" не найден!""");

public class WrongPasswordException()
    : DomainException("Неверный пароль!");