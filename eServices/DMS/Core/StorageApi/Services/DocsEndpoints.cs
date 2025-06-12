using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.Dms.Core.StorageRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace eServices.Dms.Core.StorageApi.Services
{
	public static class DocsEndpoints
	{
		[ExcludeFromCodeCoverage]
		public static RouteGroupBuilder MapDocsEndpoints(this RouteGroupBuilder group)
		{
			group.HasApiVersion(DmsDefaults.DefaultApiVersion);

			group.MapGet("", GetDocument);
			group.MapPut("", PutDocument);

			return group;
		}

		public static async Task<Results<UnauthorizedHttpResult, NotFound, Ok<JsonNode>>> GetDocument(
			IDmsStorageRepository storageRepository,
			HttpContext context,
			string owner,
			[FromRoute(Name = "table-name")] string tableName,
			string key)
		{
			if (!context.User.IsInRole($"{AppRoles.ReadStorage}:{owner}"))
				return TypedResults.Unauthorized();

			if (!await storageRepository.TableExistsAsync(owner, tableName, DmsTableTypes.Document))
				return TypedResults.NotFound();

			var document = await storageRepository.GetDocumentAsJsonAsync(owner, tableName, key);

			return document is null
				? TypedResults.NotFound()
				: TypedResults.Ok(document);
		}

		public static async Task<Results<UnauthorizedHttpResult, NotFound, Ok>> PutDocument(
			IDmsStorageRepository storageRepository,
			HttpContext context,
			string owner,
			[FromRoute(Name = "table-name")] string tableName,
			string key,
			[FromBody] JsonNode document)
		{
			if (!context.User.IsInRole($"{AppRoles.ReadStorage}:{owner}"))
				return TypedResults.Unauthorized();

			if (!await storageRepository.TableExistsAsync(owner, tableName, DmsTableTypes.Document))
				return TypedResults.NotFound();

			await storageRepository.PutDocumentAsJsonAsync(owner, tableName, key, document, context.User.Identity!.Name!);

			return TypedResults.Ok();
		}
	}
}
