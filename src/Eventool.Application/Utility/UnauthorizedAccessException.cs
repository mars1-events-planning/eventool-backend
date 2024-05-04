using Eventool.Domain.Common;

namespace Eventool.Application.Utility;

public class UnauthorizedAccessException() : DomainException("У пользователя нет доступа к данному ресурсу");