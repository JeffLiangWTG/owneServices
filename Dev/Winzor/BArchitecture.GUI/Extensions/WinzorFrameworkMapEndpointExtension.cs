using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace WinzorFramework.Extensions;

public static class WinzorFrameworkMapEndpointExtension
{
	const int CacheDuration = 60 * 60 * 24; // Cache for a day

	public static void MapImageCacherEndpoint(this IEndpointRouteBuilder endpoint)
	{
		endpoint.MapGet($"{IImageCacher.ApiPrefix}{{imageId}}", (string imageId, IImageCacher imageCacher, HttpResponse response) =>
		{
			var base64Image = imageCacher.GetImageBase64ById(imageId);
			if (string.IsNullOrWhiteSpace(base64Image))
			{
				return Results.NotFound();
			}

			response.Headers["Cache-Control"] = $"public, max-age={CacheDuration}";
			return Results.Text(base64Image);
		});
	}
}
