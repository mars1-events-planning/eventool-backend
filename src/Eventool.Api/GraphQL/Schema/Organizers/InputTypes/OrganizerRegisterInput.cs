namespace Eventool.Api.GraphQL.Schema;

[GraphQLName("OrganizerRegisterInput")]
public record OrganizerRegisterInput(
    [GraphQLName("fullname")] string FullName,
    [GraphQLName("username")] string Username,
    [GraphQLName("password")] string Password);