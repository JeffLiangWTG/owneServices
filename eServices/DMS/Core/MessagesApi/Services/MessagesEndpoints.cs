using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net.Mime;
using System.Security.Cryptography;
using eServices.Dms.Core.MessagesRepository;
using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.Dms.Core.ServiceDefaults.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace eServices.Dms.Core.MessagesApi.Services
{
	public static class MessagesEndpoints
	{
		[ExcludeFromCodeCoverage]
		public static RouteGroupBuilder MapMessagesEndpoints(this RouteGroupBuilder group)
		{
			group.HasApiVersion(DmsDefaults.DefaultApiVersion);

			group.MapPost("/{recipient-id}", PostMessage)
				.WithOpenApiConsumesBinaryContent();

			group.MapGet("/{recipient-id}", GetMessageForRecipient)
				.WithOpenApiProducesBinaryContent(Results.Ok());

			group.MapGet("/", GetMessageById)
				.WithOpenApiProducesBinaryContent(Results.Ok());

			group.MapPatch("/", PatchMessage);

			return group;
		}

		public static async Task<Results<UnauthorizedHttpResult, Created>> PostMessage(
			HttpContext context,
			IDmsMessagesRepository_V0_1 messagesRepository,
			[FromRoute(Name = "recipient-id")] string recipientId,
			[FromHeader(Name = DmsHeaders.MessageSenderId)] string senderId)
		{
			if (context.User.Identity?.Name is not string username
				|| username is not AppIds.DmsGateway
					&& !context.User.IsInRole($"{AppRoles.SendMessage}:{senderId}"))
			{
				return TypedResults.Unauthorized();
			}

			var headers = new DmsMessageMetadata
			{
				[DmsHeaders.MessageStatus] = DmsStatus.Received,
				[DmsHeaders.MessageSenderId] = senderId,
				[DmsHeaders.MessageRecipientId] = recipientId
			};
			var allowedHeaders = new[]
			{
				DmsHeaders.MessageType,
				DmsHeaders.MessageAppCode,
				DmsHeaders.MessageDescription,
				DmsHeaders.MessageFileName,
				DmsHeaders.MessageTrackingId,
				DmsHeaders.MessageParentId,
				DmsHeaders.MessageAckReqd
			};
			foreach (var header in allowedHeaders)
			{
				if (context.Request.Headers.TryGetValue(header, out var headerValue))
				{
					headers[header] = headerValue;
				}
			}

			IDmsMessageMetadata responseMetadata;

			if (username is AppIds.DmsGateway)
			{
				responseMetadata = await messagesRepository.PutMessageAsync(headers, context.Request.Body);
			}
			else
			{
				using var compressedBody = new CompressingStream(context.Request.Body, true);
				using var compressedAndEncodedBody = new CryptoStream(compressedBody, new ToBase64Transform(), CryptoStreamMode.Read, true);
				responseMetadata = await messagesRepository.PutMessageAsync(headers, compressedAndEncodedBody);
			}

			foreach (var dmsHeader in DmsHeaders.AllHeaders.Values)
			{
				if (responseMetadata.TryGetValue(dmsHeader, out var value))
				{
					context.Response.Headers[dmsHeader] = value;
				}
			}

			return TypedResults.Created(responseMetadata[DmsHeaders.MessageUri]);
		}

		public static async Task<Results<UnauthorizedHttpResult, NoContent, FileStreamHttpResult>> GetMessageForRecipient(
			HttpContext context,
			IDmsMessagesRepository_V0_1 messagesRepository,
			[FromRoute(Name = "recipient-id")] string recipientId,
			[FromHeader(Name = DmsHeaders.MessageBatchId)] Guid? batchId)
		{
			if (context.User.Identity?.Name is not string username
				|| username is not AppIds.DmsGateway
					&& !context.User.IsInRole($"{AppRoles.ReceiveMessage}:{recipientId}"))
			{
				return TypedResults.Unauthorized();
			}

			var headers = await messagesRepository.GetQueuedMessageAsync(recipientId, batchId);
			if (headers is null)
				return TypedResults.NoContent();

			foreach (var dmsHeader in DmsHeaders.AllHeaders.Values)
			{
				if (headers.TryGetValue(dmsHeader, out var value))
				{
					context.Response.Headers[dmsHeader] = value;
				}
			}

			var id = Guid.Parse(headers[DmsHeaders.MessageId]!);
			var content = await messagesRepository.GetMessageContentAsync(id);

			if (username is AppIds.DmsGateway)
			{
				return TypedResults.Stream(content!, MediaTypeNames.Application.Octet);
			}
			else
			{
				var decodedStream = new CryptoStream(content!, new FromBase64Transform(), CryptoStreamMode.Read, true);
				var decompressedStream = new GZipStream(decodedStream, CompressionMode.Decompress, true);
				return TypedResults.Stream(decompressedStream, MediaTypeNames.Application.Octet);
			}
		}

		public static async Task<Results<UnauthorizedHttpResult, NotFound, NoContent, FileStreamHttpResult>> GetMessageById(
			HttpContext context,
			IDmsMessagesRepository_V0_1 messagesRepository,
			Guid id,
			[FromQuery(Name = "info-only")] bool? infoOnly = true)
		{
			if (context.User.Identity?.Name is not string username
				|| username is not AppIds.DmsGateway
					&& username is not AppIds.DmsOpsPortal)
			{
				return TypedResults.Unauthorized();
			}

			var headers = await messagesRepository.GetMessageMetadataAsync(id);

			if (headers is null)
				return TypedResults.NotFound();

			foreach (var dmsHeader in DmsHeaders.AllHeaders.Values)
			{
				if (headers.TryGetValue(dmsHeader, out var value))
				{
					context.Response.Headers[dmsHeader] = value;
				}
			}

			if (infoOnly == true)
			{
				return TypedResults.NoContent();
			}
			else
			{
				var content = await messagesRepository.GetMessageContentAsync(id);
				if (username is AppIds.DmsGateway)
				{
					return TypedResults.Stream(content!, MediaTypeNames.Application.Octet);
				}
				else
				{
					var decodedStream = new CryptoStream(content!, new FromBase64Transform(), CryptoStreamMode.Read, true);
					var decompressedStream = new GZipStream(decodedStream, CompressionMode.Decompress, true);
					return TypedResults.Stream(decompressedStream, MediaTypeNames.Application.Octet);
				}
			}
		}

		public static async Task<Results<UnauthorizedHttpResult, BadRequest, NotFound, NoContent>> PatchMessage(
			HttpContext context,
			IDmsMessagesRepository_V0_1 messagesRepository,
			Guid id,
			string status,
			string? error,
			[FromQuery(Name = "issue-manager-report-id")] Guid? issueManagerReportId)
		{
			if (context.User.Identity?.Name is not string username)
			{
				return TypedResults.Unauthorized();
			}

			bool failedStatus = status.Equals(DmsStatus.Failed, StringComparison.OrdinalIgnoreCase);
			if (!DmsStatus.AllStatuses.ContainsKey(status)
				|| (failedStatus && (error is null || issueManagerReportId is null))
				|| (!failedStatus && (error is not null || issueManagerReportId is not null)))
			{
				return TypedResults.BadRequest();
			}

			var headers = await messagesRepository.GetMessageMetadataAsync(id);

			if (headers is null)
				return TypedResults.NotFound();

			if (username is not AppIds.DmsGateway
				&& !context.User.IsInRole($"{AppRoles.ReceiveMessage}:{headers[DmsHeaders.MessageRecipientId]}"))
			{
				return TypedResults.Unauthorized();
			}

			headers[DmsHeaders.MessageStatus] = DmsStatus.AllStatuses[status];

			if (failedStatus)
			{
				headers[DmsHeaders.MessageError] = error!;
				headers[DmsHeaders.MessageIssueManagerReportId] = issueManagerReportId!.ToString();
			}

			var responseMetadata = await messagesRepository.PatchMessageMetadataAsync(id, headers);

			foreach (var dmsHeader in DmsHeaders.AllHeaders.Values)
			{
				if (responseMetadata!.TryGetValue(dmsHeader, out var value))
				{
					context.Response.Headers[dmsHeader] = value;
				}
			}

			return TypedResults.NoContent();
		}
	}
}