using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace WinzorTestFramework;

public static class TestEndpointExtensions
{
	public static void UseTestEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapImageCacherEndpoint();
		endpoints.MapGet("/download/{objectId}", async (HttpContext ctx, IFileService fileService) =>
		{
			var objectId = ctx.Request.RouteValues["objectId"] as string;
			ctx.Response.ContentType = "application/octet-stream";
			var result = fileService.GetAndRemoveDownloadObject(objectId, out var downloadObject);
			if (result == ResultOfGetDownloadObject.NotExist)
			{
				ctx.Response.StatusCode = 404;
				return;
			}
			else
			{
				ctx.Response.Headers.ContentDisposition = $"attachment;fileName={UrlEncoder.Default.Encode(downloadObject.Name)}";
				var cts = ctx.RequestAborted;

				await ctx.Response.StartAsync(cts);
				await downloadObject.DownloadAsync(ctx.Response.Body, cts);
				await ctx.Response.CompleteAsync();
			}
		});
	}
}
