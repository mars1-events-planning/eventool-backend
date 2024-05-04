using Eventool.Application.UseCases;

namespace Eventool.Api.GraphQL.Schema.Utility;

public static class InputToRequestMappingExtensions
{
    public static EventChanges MapToRequest(this EventInput input)
    {
        var checklists =
            input.Checklists.HasValue
                ? Application.Utility.Optional<IEnumerable<ChecklistChanges>>.From(input.Checklists.Value.Select(x => x.MapToRequest()))
                : Application.Utility.Optional<IEnumerable<ChecklistChanges>>.NotSet();


        return new(
                    input.EventId.FromGqlOptional(),
                    input.Title.FromGqlOptional(),
                    input.Description.FromGqlOptional(),
                    input.Address.FromGqlOptional(),
                    input.StartDateTimeUtc.FromGqlOptional(), 
                    checklists
            );
    }

    public static ChecklistChanges MapToRequest(this ChecklistInput input) => new(
        input.Id.FromGqlOptional(),
        input.Title,
        input.ChecklistItems
    );


    private static Application.Utility.Optional<T> FromGqlOptional<T>(this Optional<T> gqlOptional) =>
        gqlOptional.HasValue
            ? Application.Utility.Optional<T>.From(gqlOptional.Value)
            : Application.Utility.Optional<T>.NotSet();
}