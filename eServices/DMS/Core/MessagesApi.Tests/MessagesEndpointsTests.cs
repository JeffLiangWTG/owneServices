using System.Security.Cryptography;
using System.Text;
using eServices.Dms.Core.MessagesApi.Services;
using eServices.Dms.Core.MessagesRepository;
using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework.Internal;

namespace eServices.Dms.Core.MessagesApi.Tests;

public class MessagesEndpointsTests
{
	[TestCase("USER", "APP", typeof(UnauthorizedHttpResult))]
	[TestCase(AppIds.DmsGateway, "APP", typeof(FileStreamHttpResult))]
	[TestCase("APP", "APP", typeof(FileStreamHttpResult))]
	[TestCase(AppIds.DmsGateway, "APP", typeof(NoContent))]
	[TestCase("APP", "APP", typeof(NoContent))]
	public async Task GetMessagesByRecipientTests(string userName, string recipientId, Type expectedResultType)
	{
		var httpContext = new Mock<HttpContext>();
		httpContext.SetupGet(c => c.User.Identity!.Name).Returns(userName);
		httpContext.Setup(c => c.User.IsInRole($"{AppRoles.ReceiveMessage}:{userName}")).Returns(userName == recipientId);
		var responseHeaders = new HeaderDictionary();
		httpContext.SetupGet(x => x.Response.Headers).Returns(responseHeaders);
		var messagesRepository = new Mock<IDmsMessagesRepository_V0_1>();

		var trackingId = Guid.NewGuid();
		using var content = new MemoryStream(trackingId.ToByteArray());

		using var rawStream = new MemoryStream(trackingId.ToByteArray());
		using var compressedStream = new CompressingStream(rawStream, true);
		using var compressedAndEncodedStream = new CryptoStream(compressedStream, new ToBase64Transform(), CryptoStreamMode.Read, true);
		using var contentStream = new MemoryStream();
		compressedAndEncodedStream.CopyTo(contentStream);
		contentStream.Position = 0;

		if (expectedResultType == typeof(FileStreamHttpResult))
		{
			var headers = new DmsMessageMetadata { [DmsHeaders.MessageId] = trackingId.ToString() };
			messagesRepository.Setup(x => x.GetQueuedMessageAsync(recipientId, null)).ReturnsAsync(headers);
			messagesRepository.Setup(x => x.GetMessageContentAsync(trackingId)).ReturnsAsync(contentStream);
		}

		var result = await MessagesEndpoints.GetMessageForRecipient(httpContext.Object, messagesRepository.Object, recipientId, null);

		Assert.That(result.Result, Is.TypeOf(expectedResultType));

		if (result.Result is FileStreamHttpResult streamResult)
		{
			Assert.Multiple(() =>
			{
				messagesRepository.Verify(x => x.GetQueuedMessageAsync(recipientId, null), Times.Once);
				messagesRepository.Verify(x => x.GetMessageContentAsync(trackingId), Times.Once);
				Assert.That(responseHeaders.Select(h => (h.Key, h.Value.ToString())), Is.EquivalentTo(new[] { (DmsHeaders.MessageId, trackingId.ToString()) }).IgnoreCase);
				var resultBuffer = new byte[1024];
				var resultLength = streamResult!.FileStream.Read(resultBuffer, 0, resultBuffer.Length);
				if (userName is AppIds.DmsGateway)
				{
					Assert.That(resultBuffer[..resultLength], Is.EquivalentTo(contentStream.ToArray()));
				}
				else
				{
					Assert.That(resultBuffer[..resultLength], Is.EquivalentTo(trackingId.ToByteArray()));
				}
			});
		}
	}

	[TestCase("APP", null, true, typeof(UnauthorizedHttpResult))]
	[TestCase(AppIds.DmsGateway, "9A3F60E0-F5D8-4205-B344-7101FC03937E", true, typeof(NotFound))]
	[TestCase(AppIds.DmsOpsPortal, "9A3F60E0-F5D8-4205-B344-7101FC03937E", false, typeof(NotFound))]
	[TestCase(AppIds.DmsGateway, "DEBE17C0-7415-426A-99BB-31A409218F62", true, typeof(NoContent))]
	[TestCase(AppIds.DmsOpsPortal, "DEBE17C0-7415-426A-99BB-31A409218F62", true, typeof(NoContent))]
	[TestCase(AppIds.DmsGateway, "DEBE17C0-7415-426A-99BB-31A409218F62", false, typeof(FileStreamHttpResult))]
	[TestCase(AppIds.DmsOpsPortal, "DEBE17C0-7415-426A-99BB-31A409218F62", false, typeof(FileStreamHttpResult))]
	public async Task GetMessagesByIdTests(string userName, string? trackingId, bool infoOnly, Type expectedResultType)
	{
		var httpContext = new Mock<HttpContext>();
		httpContext.SetupGet(c => c.User.Identity!.Name).Returns(userName);
		var responseHeaders = new HeaderDictionary();
		httpContext.SetupGet(x => x.Response.Headers).Returns(responseHeaders);
		var messagesRepository = new Mock<IDmsMessagesRepository_V0_1>();

		var testId = Guid.Parse("DEBE17C0-7415-426A-99BB-31A409218F62");
		using var rawStream = new MemoryStream(testId.ToByteArray());
		using var compressedStream = new CompressingStream(rawStream, true);
		using var compressedAndEncodedStream = new CryptoStream(compressedStream, new ToBase64Transform(), CryptoStreamMode.Read, true);
		using var contentStream = new MemoryStream();
		compressedAndEncodedStream.CopyTo(contentStream);
		contentStream.Position = 0;
		var headers = new DmsMessageMetadata { [DmsHeaders.MessageTrackingId] = trackingId };

		messagesRepository.Setup(x => x.GetMessageMetadataAsync(testId, false)).ReturnsAsync(headers);
		messagesRepository.Setup(x => x.GetMessageContentAsync(testId)).ReturnsAsync(contentStream);

		var id = trackingId is not null ? Guid.Parse(trackingId) : Guid.Empty;

		var result = await MessagesEndpoints.GetMessageById(httpContext.Object, messagesRepository.Object, id, infoOnly);

		Assert.That(result.Result, Is.TypeOf(expectedResultType));

		if (result.Result is NoContent or FileStreamHttpResult)
		{
			Assert.Multiple(() =>
			{
				messagesRepository.Verify(x => x.GetMessageMetadataAsync(id, false), Times.Once);
				Assert.That(responseHeaders.Select(h => (h.Key, h.Value.ToString())), Is.EquivalentTo(new[] { (DmsHeaders.MessageTrackingId, testId.ToString()) }).IgnoreCase);
				if (result.Result is NoContent)
				{
					messagesRepository.Verify(x => x.GetMessageContentAsync(id), Times.Never);
				}
				else
				{
					messagesRepository.Verify(x => x.GetMessageContentAsync(id), Times.Once);
					var streamResult = result.Result as FileStreamHttpResult;
					Assert.That(streamResult, Is.Not.Null);
					var resultBuffer = new byte[1024];
					var resultLength = streamResult!.FileStream.Read(resultBuffer, 0, resultBuffer.Length);
					if (userName is AppIds.DmsGateway)
					{
						Assert.That(resultBuffer[..resultLength], Is.EquivalentTo(contentStream.ToArray()));
					}
					else
					{
						Assert.That(resultBuffer[..resultLength], Is.EquivalentTo(testId.ToByteArray()));
					}
				}
			});
		}
	}

	[TestCase("TST", typeof(UnauthorizedHttpResult))]
	[TestCase("APP", typeof(Created))]
	[TestCase(AppIds.DmsGateway, typeof(Created))]
	public async Task PostMessagesTests(string userName, Type expectedResultType)
	{
		var httpContext = new Mock<HttpContext>();
		httpContext.SetupGet(c => c.User.Identity!.Name).Returns(userName);
		httpContext.Setup(c => c.User.IsInRole($"{AppRoles.SendMessage}:APP")).Returns(userName == "APP");
		var requestHeaders = new HeaderDictionary();
		var responseHeaders = new HeaderDictionary();
		httpContext.SetupGet(x => x.Request.Headers).Returns(requestHeaders);
		httpContext.SetupGet(x => x.Response.Headers).Returns(responseHeaders);
		var messagesRepository = new Mock<IDmsMessagesRepository_V0_1>();

		var id = Guid.NewGuid();
		var uri = $"http://localhost/messages?EI_PK={id}";
		requestHeaders[DmsHeaders.MessageRecipientId] = "USC";
		requestHeaders[DmsHeaders.MessageType] = "ABI";
		requestHeaders[DmsHeaders.MessageAppCode] = "USI";
		requestHeaders[DmsHeaders.MessageDescription] = "Test Message";
		requestHeaders[DmsHeaders.MessageFileName] = "File Name";
		requestHeaders[DmsHeaders.MessageTrackingId] = id.ToString();

		using var rawStream = new MemoryStream(Encoding.Default.GetBytes(uri));
		using var compressedStream = new CompressingStream(rawStream, true);
		using var compressedAndEncodedStream = new CryptoStream(compressedStream, new ToBase64Transform(), CryptoStreamMode.Read, true);
		using var gatewayStream = new MemoryStream();
		compressedAndEncodedStream.CopyTo(gatewayStream);
		rawStream.Position = gatewayStream.Position = 0;
		if (userName is AppIds.DmsGateway)
		{
			httpContext.SetupGet(x => x.Request.Body).Returns(gatewayStream);
		}
		else
		{
			httpContext.SetupGet(x => x.Request.Body).Returns(rawStream);
		}

		Stream postedMessageStream = new MemoryStream();
		messagesRepository.Setup(x => x.PutMessageAsync(It.IsAny<IDmsMessageMetadata>(), It.IsAny<Stream>()))
			.Callback<IDmsMessageMetadata, Stream>((headers, body) => body.CopyTo(postedMessageStream))
			.ReturnsAsync(new DmsMessageMetadata() { [DmsHeaders.MessageUri] = uri });

		var result = await MessagesEndpoints.PostMessage(httpContext.Object, messagesRepository.Object, "APP", "APP");
		Assert.That(result.Result, Is.TypeOf(expectedResultType));

		if (result.Result is Created createdResult)
		{
			Assert.That(createdResult?.Location, Is.EqualTo(uri));
			Assert.That(responseHeaders.Select(h => (h.Key, h.Value.ToString())), Is.EquivalentTo(new[] { (DmsHeaders.MessageUri, uri) }).IgnoreCase);
			postedMessageStream.Position = 0;
		}
	}

	[Test]
	public async Task PatchMessagesTests()
	{
		var httpContext = new Mock<HttpContext>();
		httpContext.SetupGet(c => c.User.Identity!.Name).Returns((string)null!);
		var requestHeaders = new HeaderDictionary();
		var responseHeaders = new HeaderDictionary();
		httpContext.SetupGet(x => x.Request.Headers).Returns(requestHeaders);
		httpContext.SetupGet(x => x.Response.Headers).Returns(responseHeaders);
		var messagesRepository = new Mock<IDmsMessagesRepository_V0_1>();

		var unAuthResult = await MessagesEndpoints.PatchMessage(httpContext.Object, messagesRepository.Object, Guid.NewGuid(), "invalid", null, null);
		Assert.That(unAuthResult.Result, Is.TypeOf<UnauthorizedHttpResult>());

		httpContext.SetupGet(c => c.User.Identity!.Name).Returns(AppIds.DmsGateway);

		var badResult = await MessagesEndpoints.PatchMessage(httpContext.Object, messagesRepository.Object, Guid.NewGuid(), "invalid", null, null);
		Assert.That(badResult.Result, Is.TypeOf<BadRequest>());
		badResult = await MessagesEndpoints.PatchMessage(httpContext.Object, messagesRepository.Object, Guid.NewGuid(), "Failed", null, Guid.NewGuid());
		Assert.That(badResult.Result, Is.TypeOf<BadRequest>());
		badResult = await MessagesEndpoints.PatchMessage(httpContext.Object, messagesRepository.Object, Guid.NewGuid(), "Delivered", "Error message", null);
		Assert.That(badResult.Result, Is.TypeOf<BadRequest>());

		var notFoundResult = await MessagesEndpoints.PatchMessage(httpContext.Object, messagesRepository.Object, Guid.NewGuid(), "delivered", null, null);
		Assert.That(notFoundResult.Result, Is.TypeOf<NotFound>());

		var uri = "http://localhost/messages?EI_PK=0000";
		messagesRepository.Setup(x => x.GetMessageMetadataAsync(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new DmsMessageMetadata { [DmsHeaders.MessageRecipientId] = "TST" });
		messagesRepository.Setup(x => x.PatchMessageMetadataAsync(It.IsAny<Guid>(), It.IsAny<IDmsMessageMetadata>())).ReturnsAsync(new DmsMessageMetadata() { [DmsHeaders.MessageUri] = uri });

		var gatewayResult = await MessagesEndpoints.PatchMessage(httpContext.Object, messagesRepository.Object, Guid.NewGuid(), "delivered", null, null);
		Assert.That(gatewayResult.Result, Is.TypeOf<NoContent>());
		Assert.That(responseHeaders.Select(h => (h.Key, h.Value.ToString())), Is.EquivalentTo(new[] { (DmsHeaders.MessageUri, uri) }).IgnoreCase);

		httpContext.SetupGet(c => c.User.Identity!.Name).Returns("APP");
		httpContext.Setup(c => c.User.IsInRole($"{AppRoles.ReceiveMessage}:APP")).Returns(true);

		messagesRepository.Setup(x => x.GetMessageMetadataAsync(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new DmsMessageMetadata { [DmsHeaders.MessageRecipientId] = "TST" });
		var msgUnAuthResult = await MessagesEndpoints.PatchMessage(httpContext.Object, messagesRepository.Object, Guid.NewGuid(), "delivered", null, null);
		Assert.That(msgUnAuthResult.Result, Is.TypeOf<UnauthorizedHttpResult>());

		messagesRepository.Setup(x => x.GetMessageMetadataAsync(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new DmsMessageMetadata { [DmsHeaders.MessageRecipientId] = "APP" });
		var appResult = await MessagesEndpoints.PatchMessage(httpContext.Object, messagesRepository.Object, Guid.NewGuid(), "FAILED", "Error Message", Guid.NewGuid());
		Assert.That(appResult.Result, Is.TypeOf<NoContent>());
		Assert.That(responseHeaders.Select(h => (h.Key, h.Value.ToString())), Is.EquivalentTo(new[] { (DmsHeaders.MessageUri, uri) }).IgnoreCase);
	}
}