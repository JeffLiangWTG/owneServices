using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.Dms.Core.ServiceDefaults.OpenApi;
using eServices.Dms.Core.StorageRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace eServices.Dms.Core.StorageApi.Services
{
	public static class BlobsEndpoints
	{
		[ExcludeFromCodeCoverage]
		public static RouteGroupBuilder MapBlobsEndpoints(this RouteGroupBuilder group)
		{
			group.HasApiVersion(DmsDefaults.DefaultApiVersion);

			group.MapGet("", GetObject)
				.WithOpenApiProducesBinaryContent(Results.Ok());

			group.MapPut("", PutObject)
				.WithOpenApiConsumesBinaryContent();

			return group;
		}

		public static async Task<Results<UnauthorizedHttpResult, NotFound, FileStreamHttpResult>> GetObject(
			IDmsStorageRepository storageRepository,
			HttpContext context,
			string owner,
			[FromRoute(Name = "table-name")] string tableName,
			string key)
		{
			if (!context.User.IsInRole($"{AppRoles.ReadStorage}:{owner}"))
				return TypedResults.Unauthorized();

			if (!await storageRepository.TableExistsAsync(owner, tableName, DmsTableTypes.Object))
				return TypedResults.NotFound();

			var blob = await storageRepository.GetObjectAsStreamAsync(owner, tableName, key);

			return blob is null
				? TypedResults.NotFound()
				: TypedResults.Stream(blob, MediaTypeNames.Application.Octet);
		}

		public static async Task<Results<UnauthorizedHttpResult, NotFound, Ok>> PutObject(
			IDmsStorageRepository storageRepository,
			HttpContext context,
			string owner,
			[FromRoute(Name = "table-name")] string tableName,
			string key)
		{
			if (!context.User.IsInRole($"{AppRoles.ReadStorage}:{owner}"))
				return TypedResults.Unauthorized();

			if (!await storageRepository.TableExistsAsync(owner, tableName, DmsTableTypes.Object))
				return TypedResults.NotFound();

			await storageRepository.PutObjectAsStreamAsync(owner, tableName, key, context.Request.Body, context.User.Identity!.Name!);

			return TypedResults.Ok();
		}
	}
}
