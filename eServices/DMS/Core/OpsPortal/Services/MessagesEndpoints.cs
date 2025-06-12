using System.IO.Compression;
using System.Net.Mime;
using eServices.Dms.Core.ServiceDefaults;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.EntityFrameworkCore;

namespace eServices.Dms.Core.OpsPortal.Services
{
	public static class MessagesEndpoints
	{
		public static RouteGroupBuilder MapMessagesEndpoints(this RouteGroupBuilder group)
		{
			group.HasApiVersion(DmsDefaults.DefaultApiVersion);
			group.RequireAuthorization("MessagesUser");

			group.MapGet("/content", GetContent);

			return group;
		}

		public static async Task<IResult> GetContent(IHttpClientFactory ClientFactory, IDbContextFactory<eHubTransactionsContext> DbFactory, Guid id, string? format)
		{
			using var context = DbFactory?.CreateDbContext();
			if (context?.Database.IsSqlServer() ?? false)
			{
				var content = await context.eHubInboxMessage.Where(i => i.EI_PK == id).Select(i => i.Content.EI_Content).FirstOrDefaultAsync();
				if (content is null)
					return Results.Problem("Message not found", statusCode: StatusCodes.Status404NotFound);

				var outputStream = new MemoryStream();
				var compressedStream = new MemoryStream(Convert.FromBase64String(content));
				var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress, true);
				await gzipStream.CopyToAsync(outputStream);
				outputStream.Position = 0;

				return format switch
				{
					"text" => Results.Text(await new StreamReader(outputStream).ReadToEndAsync()),
					_ => Results.Stream(outputStream, MediaTypeNames.Application.Octet, $"{id}.txt")
				};
			}
			else
			{
				var messagesClient = ClientFactory.CreateClient("DmsMessagesApi");
				var messagesResponse = await messagesClient.GetAsync($"/messages?id={id}&info-only=false");
				messagesResponse.EnsureSuccessStatusCode();

				return format switch
				{
					"text" => Results.Text(await messagesResponse.Content.ReadAsStringAsync()),
					_ => Results.Stream(messagesResponse.Content.ReadAsStream(), MediaTypeNames.Application.Octet, $"{id}.txt")
				};
			}
		}
	}
}
