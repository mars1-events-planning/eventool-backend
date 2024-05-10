using System.Security.Claims;
using Eventool.Api.GraphQL.Schema.Utility;
using Eventool.Domain.Common;
using Eventool.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Path = System.IO.Path;

public static class ImageEndpoints
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp"
    };
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/bmp"
    };
    
    public static void MapImageEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/set-avatar", SetUserAvatarAsync).DisableAntiforgery();
        routes.MapPost("/api/set-guest-avatar", SetGuestAvatarAsync).DisableAntiforgery();
        routes.MapPost("/api/add-event-image", AddEventImageAsync).DisableAntiforgery();
    }

    private static async Task<IResult> SetUserAvatarAsync(
        [FromServices] IImageStorage imageStorage,
        [FromServices] IUnitOfWork unitOfWork,
        IFormFile image,
        ClaimsPrincipal claims,
        CancellationToken ct) => await unitOfWork.ExecuteAndCommitAsync(async repositories =>
    {
        var organizer = await repositories.OrganizersRepository.TryGetByIdAsync(claims.GetOrganizerId(), ct);
        if (organizer is null)
            return Results.Unauthorized();
        
        if (!IsValidImage(image))
            return Results.BadRequest();

        await using var stream = image.OpenReadStream();
        var filename = $"organizers/{organizer.Id}/avatar";
        var imageUrl = await imageStorage.UploadImageAsync(stream, filename);

        organizer.PhotoUrl = imageUrl;

        repositories.OrganizersRepository.Save(organizer);

        return Results.Ok();
    }, ct);

    private static async Task<IResult> SetGuestAvatarAsync(
        [FromServices] IImageStorage imageStorage,
        [FromServices] IUnitOfWork unitOfWork,
        [FromForm] IFormFile image,
        [FromForm] Guid eventId,
        [FromForm] Guid guestId,
        ClaimsPrincipal claims,
        CancellationToken ct) => await unitOfWork.ExecuteAndCommitAsync(
        async repositories =>
        {
            var organizer = await repositories.OrganizersRepository.TryGetByIdAsync(claims.GetOrganizerId(), ct);
            if (organizer is null)
                return Results.Unauthorized();

            var @event = await repositories.EventRepository.GetByIdAsync(eventId, ct);
            if (@event is null)
                return Results.NotFound();

            if (@event.CreatorId != organizer.Id)
                return Results.Unauthorized();

            var guest = @event.Guests.FirstOrDefault(x => x.Id == guestId);
            if (guest is null)
                return Results.NotFound();
            
            if (!IsValidImage(image))
                return Results.BadRequest();

            await using var stream = image.OpenReadStream();
            var filename = $"events/{@event.Id}/guests/{guest.Id}/photo";
            var imageUrl = await imageStorage.UploadImageAsync(stream, filename);

            guest.PhotoUrl = imageUrl;

            repositories.EventRepository.Save(@event);

            return Results.Ok();
        }, ct);

    private static async Task<IResult> AddEventImageAsync(
        [FromServices] IImageStorage imageStorage,
        [FromServices] IUnitOfWork unitOfWork,
        [FromForm] IFormFile image,
        [FromForm] Guid eventId,
        ClaimsPrincipal claims,
        CancellationToken ct) => await unitOfWork.ExecuteAndCommitAsync(async repositories =>
    {
        var organizer = await repositories.OrganizersRepository.TryGetByIdAsync(claims.GetOrganizerId(), ct);
        if (organizer is null)
            return Results.Unauthorized();

        var @event = await repositories.EventRepository.GetByIdAsync(eventId, ct);
        if (@event is null)
            return Results.NotFound();

        if (@event.CreatorId != organizer.Id)
            return Results.Unauthorized();

        if (!IsValidImage(image))
            return Results.BadRequest();

        await using var stream = image.OpenReadStream();
        var filename = $"events/{@event.Id}/images/{Guid.NewGuid()}";
        var imageUrl = await imageStorage.UploadImageAsync(stream, filename);

        @event.AddImageUrl(imageUrl);

        repositories.EventRepository.Save(@event);

        return Results.Ok();
    }, ct);
    
    private static bool IsValidImage(IFormFile image)
    {
        if (image.Length is 0 or > MaxFileSize)
            return false;

        var fileExtension = Path.GetExtension(image.FileName);
        return AllowedFileExtensions.Contains(fileExtension) &&
               AllowedContentTypes.Contains(image.ContentType);
    }
}