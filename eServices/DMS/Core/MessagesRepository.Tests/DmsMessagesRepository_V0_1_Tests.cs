using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using eServices.Dms.Core.ServiceDefaults;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace eServices.Dms.Core.MessagesRepository.Tests;

[Property("DAT:CapabilityRequirements", "SQL")]
public class DmsMessagesRepository_V0_1_Tests
{
	private IConfiguration configuration = null!;
	private DbContextOptions<eHubTransactionsContext> messagesOptions = null!;
	private eHubTransactionsContext messagesContext = null!;
	private DmsMessagesRepository_V0_1 messagesRepository = null!;

	[OneTimeSetUp]
	public async Task OneTimeSetUp()
	{
		configuration = new ConfigurationBuilder()
			.AddJsonFile(@"appSettings.json", false, false)
			.Build();

		var messagesConnectionString = configuration.GetConnectionString("DmsMessages")
				?? throw new InvalidOperationException("Missing connection string: DmsMessages");

		messagesOptions = new DbContextOptionsBuilder<eHubTransactionsContext>()
			.UseSqlServer(messagesConnectionString)
			.Options;

#if !DEBUG
		using var setupMessagesContext = new eHubTransactionsContext(messagesOptions);
		await setupMessagesContext.Database.EnsureDeletedAsync();
		await setupMessagesContext.Database.MigrateAsync();
#else
		await Task.CompletedTask;
#endif
	}

#if !DEBUG
	[OneTimeTearDown]
	public async Task OneTimeTearDown()
	{
		using var tearDownMessagesContext = new eHubTransactionsContext(messagesOptions);
		await tearDownMessagesContext.Database.EnsureDeletedAsync();
	}
#endif

	[SetUp]
	public async Task Setup()
	{
		messagesContext = new eHubTransactionsContext(messagesOptions);
		messagesRepository = new DmsMessagesRepository_V0_1(messagesContext, configuration);
		await messagesRepository.BeginTransactionAsync();
	}

	[TearDown]
	public async Task TearDown()
	{
		if (messagesRepository is not null)
			await messagesRepository.DisposeAsync();
		if (messagesContext is not null)
			await messagesContext.DisposeAsync();
	}

	[Test]
	public async Task PutMessageTests()
	{
		var appClient = GetClient("APP");
		await messagesContext.eHubClient.AddAsync(appClient);
		await messagesContext.SaveChangesAsync();

		var trackingId = Guid.NewGuid();
		var headers = new DmsMessageMetadata
		{
			[DmsHeaders.MessageSenderId] = "APP",
			[DmsHeaders.MessageRecipientId] = "APP",
			[DmsHeaders.MessageAppCode] = "DMS",
			[DmsHeaders.MessageType] = "MessageType",
			[DmsHeaders.MessageTrackingId] = trackingId.ToString(),
			[DmsHeaders.MessageDescription] = "Message Description",
			[DmsHeaders.MessageFileName] = "file-name.msg"
		};
		using var rawContent = new MemoryStream(Encoding.UTF8.GetBytes("""
			<Message>
				<Content>Message Content</Content>
			</Message>
			"""));
		using var compressedStream = new CompressingStream(rawContent, true);
		using var compressedAndEncodedStream = new CryptoStream(compressedStream, new ToBase64Transform(), CryptoStreamMode.Read, true);
		using var contentStream = new MemoryStream();
		compressedAndEncodedStream.CopyTo(contentStream);
		rawContent.Position = contentStream.Position = 0;

		var result = await messagesRepository.PutMessageAsync(headers, contentStream);

		messagesContext.ChangeTracker.Clear();
		Assert.Multiple(async () =>
		{
			var id = await messagesContext.eHubInboxMessage.Where(i => i.EI_MessageTrackingID == trackingId
				.ToString()).Select(i => i.EI_PK).FirstOrDefaultAsync();
			Assert.That(id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(result[DmsHeaders.MessageUri].ToString(), Is.EqualTo($"http://localhost/messages?id={id}"));
			Assert.That(messagesContext.eHubInboxMessage.Count(), Is.EqualTo(1));
		});
	}

	[Test]
	public async Task GetQueuedMessageTests()
	{
		var appClient = GetClient("APP");
		await messagesContext.eHubClient.AddAsync(appClient);
		await messagesContext.SaveChangesAsync();
		var headers = new DmsMessageMetadata
		{
			[DmsHeaders.MessageSenderId] = "APP",
			[DmsHeaders.MessageRecipientId] = "APP"
		};
		using var contentStream = new MemoryStream();
		var queuedResult = await messagesRepository.PutMessageAsync(headers, contentStream);

		messagesContext.ChangeTracker.Clear();

		var dequeuedResult = await messagesRepository.GetQueuedMessageAsync("APP");

		Assert.That(queuedResult[DmsHeaders.MessageUri].ToString(), Is.EqualTo(dequeuedResult?[DmsHeaders.MessageUri].ToString()));
	}

	[Test]
	public async Task GetMessageContentTests()
	{
		var emptyResult = await messagesRepository.GetMessageContentAsync(Guid.NewGuid());
		Assert.That(emptyResult, Is.Null);

		var appClient = GetClient("APP");
		await messagesContext.eHubClient.AddAsync(appClient);
		await messagesContext.SaveChangesAsync();

		var trackingId = Guid.NewGuid();
		var headers = new DmsMessageMetadata { [DmsHeaders.MessageSenderId] = "APP", [DmsHeaders.MessageRecipientId] = "APP" };
		var msg = """
			<Message>
				<Content>Message Content</Content>
			</Message>
			""";
		using var rawContent = new MemoryStream(Encoding.UTF8.GetBytes(msg));
		using var compressedStream = new CompressingStream(rawContent, true);
		using var compressedAndEncodedStream = new CryptoStream(compressedStream, new ToBase64Transform(), CryptoStreamMode.Read, true);
		using var contentStream = new MemoryStream();
		compressedAndEncodedStream.CopyTo(contentStream);
		rawContent.Position = contentStream.Position = 0;

		var result = await messagesRepository.PutMessageAsync(headers, contentStream);
		var id = Guid.Parse(result[DmsHeaders.MessageId].ToString());

		messagesContext.ChangeTracker.Clear();

		var contentResult = await messagesRepository.GetMessageContentAsync(id);

		Assert.Multiple(() =>
		{
			Assert.That(contentResult, Is.Not.Null);
			using var decodedStream = new CryptoStream(contentResult!, new FromBase64Transform(), CryptoStreamMode.Read, true);
			using var decompressedStream = new GZipStream(decodedStream, CompressionMode.Decompress, true);
			using var contentReader = new StreamReader(decompressedStream);
			Assert.That(contentReader.ReadToEnd(), Is.EqualTo(msg));
		});
	}

	[Test]
	public async Task GetMessageMetadataTests()
	{
		var appClient = GetClient("APP");
		await messagesContext.eHubClient.AddAsync(appClient);
		await messagesContext.SaveChangesAsync();

		var trackingId = Guid.NewGuid();
		var headers = new DmsMessageMetadata
		{
			[DmsHeaders.MessageSenderId] = "APP",
			[DmsHeaders.MessageRecipientId] = "APP",
			[DmsHeaders.MessageAppCode] = "DMS",
			[DmsHeaders.MessageType] = "MessageType",
			[DmsHeaders.MessageTrackingId] = trackingId.ToString(),
			[DmsHeaders.MessageDescription] = "Message Description",
			[DmsHeaders.MessageFileName] = "file-name.msg"
		};
		using var contentStream = new MemoryStream();
		var putResult = await messagesRepository.PutMessageAsync(headers, contentStream);
		var id = Guid.Parse(putResult[DmsHeaders.MessageId].ToString());

		messagesContext.ChangeTracker.Clear();

		var getResult = await messagesRepository.GetMessageMetadataAsync(id);

		Assert.Multiple(() =>
		{
			Assert.That(getResult, Is.Not.Null);
			Assert.That(getResult, Is.EquivalentTo(putResult));
			Assert.That(getResult, Is.EquivalentTo(new DmsMessageMetadata
			{
				[DmsHeaders.MessageId] = id.ToString(),
				[DmsHeaders.MessageUri] = $"http://localhost/messages?id={id}",
				[DmsHeaders.MessageTimeReceived] = putResult[DmsHeaders.MessageTimeReceived],
				[DmsHeaders.MessageTimeLastUpdate] = putResult[DmsHeaders.MessageTimeLastUpdate],
				[DmsHeaders.MessageSenderId] = "APP",
				[DmsHeaders.MessageRecipientId] = "APP",
				[DmsHeaders.MessageAppCode] = "DMS",
				[DmsHeaders.MessageType] = "MessageType",
				[DmsHeaders.MessageTrackingId] = trackingId.ToString(),
				[DmsHeaders.MessageDescription] = "Message Description",
				[DmsHeaders.MessageFileName] = "file-name.msg",
				[DmsHeaders.MessageStatus] = DmsStatus.Received
			}));
		});
	}

	[TestCase(DmsStatus.Delivered, 3, 3, null, null)]
	[TestCase(DmsStatus.Failed, 255, 255, "Error message", "6D1DF060-E9D4-48F0-8201-986EAC0B1056")]
	public async Task PatchMessageMetadataTests(string dmsStatus, int ei_status, int oi_status, string? error, string? reportId)
	{
		var appClient = GetClient("APP");
		await messagesContext.eHubClient.AddAsync(appClient);
		await messagesContext.SaveChangesAsync();
		var headers = new DmsMessageMetadata
		{
			[DmsHeaders.MessageSenderId] = "APP",
			[DmsHeaders.MessageRecipientId] = "APP"
		};
		using var contentStream = new MemoryStream();
		var putResult = await messagesRepository.PutMessageAsync(headers, contentStream);
		var id = Guid.Parse(putResult[DmsHeaders.MessageId].ToString());

		messagesContext.ChangeTracker.Clear();

		var patchHeaders = new DmsMessageMetadata { [DmsHeaders.MessageStatus] = dmsStatus };
		if (error is not null && reportId is not null)
		{
			patchHeaders[DmsHeaders.MessageError] = error;
			patchHeaders[DmsHeaders.MessageIssueManagerReportId] = reportId;
		}
		var result = await messagesRepository.PatchMessageMetadataAsync(id, patchHeaders);

		messagesContext.ChangeTracker.Clear();
		Assert.Multiple(() =>
		{
			Assert.That(result, Is.Not.Null);
			Assert.That(messagesContext.eHubInboxMessage.Find(id)?.EI_Status, Is.EqualTo(ei_status));
			Assert.That(messagesContext.eHubOutboxMessage.Find(id)?.OI_Status, Is.EqualTo(oi_status));
			if (error is not null && reportId is not null)
			{
				Assert.That(messagesContext.eHubError.ToList().Select(e => (e.EE_EI_Inbox, e.EE_Description, e.EE_ErrorDetail)), Is.EquivalentTo(
					new[] { (EE_EI_Inbox: id, EE_Description: error, EE_ErrorDetail: $"<ErrorDetail><IssueManagerReportId>{reportId}</IssueManagerReportId></ErrorDetail>") }));
			}
		});
	}

	[Test]
	public async Task SendReceiveWorkflowTest()
	{
		await messagesContext.eHubClient.AddRangeAsync([
			GetClient("SND", ackReqd: true),
			GetClient("RCV")]);
		await SetupStatusMessages();
		await messagesContext.SaveChangesAsync();

		// SND to RCV
		var trackingId = Guid.NewGuid();
		using var msg1SndStream = new MemoryStream(Encoding.UTF8.GetBytes($"<Msg><Id>{trackingId}</Id></Msg>"));
		var msg1SndHeaders = await messagesRepository.PutMessageAsync(
			new DmsMessageMetadata
			{
				[DmsHeaders.MessageSenderId] = "SND",
				[DmsHeaders.MessageRecipientId] = "RCV",
				[DmsHeaders.MessageTrackingId] = trackingId.ToString()
			},
			msg1SndStream);
		var msg1Id = Guid.Parse(msg1SndHeaders[DmsHeaders.MessageId]!);
		var msg1RcvHeaders = await messagesRepository.GetQueuedMessageAsync("RCV");
		Assert.That(Guid.Parse(msg1RcvHeaders![DmsHeaders.MessageId]!), Is.EqualTo(msg1Id));
		var msg1RcvStream = await messagesRepository.GetMessageContentAsync(msg1Id);

		var rcvPatchResult = await messagesRepository.PatchMessageMetadataAsync(msg1Id,
			new DmsMessageMetadata { [DmsHeaders.MessageStatus] = DmsStatus.Delivered });

		var sndFinalResult = await messagesRepository.GetMessageMetadataAsync(msg1Id, true);
		Assert.That(sndFinalResult![DmsHeaders.MessageStatus].ToString(), Is.EqualTo(DmsStatus.Delivered));

		messagesContext.ChangeTracker.Clear();

		Assert.That(messagesContext.eHubInboxMessage.OrderBy(i => i.EI_InsertUTC)
			.Select(i => new { SenderId = i.EI_CC_SenderNavigation.CC_ID, RecipientId = i.EI_CC_RecipientNavigation!.CC_ID, i.EI_Status })
			.ToList().Select(m => (m.SenderId, m.RecipientId, m.EI_Status)), Is.EquivalentTo(new[]
		{
			("SND", "RCV", 3 )
		}));
		Assert.That(messagesContext.eHubOutboxMessage.OrderBy(o => o.OI_InsertUTC)
			.Select(o => new { SenderId = o.OI_CC_SenderNavigation.CC_ID, RecipientId = o.OI_CC_RecipientNavigation.CC_ID, o.OI_Status })
			.ToList().Select(m => (m.SenderId, m.RecipientId, m.OI_Status)), Is.EquivalentTo(new[]
		{
			("SND", "RCV", 3 ),
			("eHub", "SND", 0 )
		}));
	}

	[Test]
	public async Task ForwardMessageWorkflowTest()
	{
		await messagesContext.eHubClient.AddRangeAsync([
			GetClient("SND", ackReqd: true),
			GetClient("RCV"),
			GetClient("APP1"),
			GetClient("APP2")]);
		await SetupStatusMessages();
		await messagesContext.SaveChangesAsync();

		// SND to APP1
		var trackingId = Guid.NewGuid();
		using var msg1SndStream = new MemoryStream(Encoding.UTF8.GetBytes($"<Msg><Id>{trackingId}</Id></Msg>"));
		var msg1SndHeaders = await messagesRepository.PutMessageAsync(
			new DmsMessageMetadata
			{
				[DmsHeaders.MessageSenderId] = "SND",
				[DmsHeaders.MessageRecipientId] = "APP1",
				[DmsHeaders.MessageTrackingId] = trackingId.ToString()
			},
			msg1SndStream);
		var msg1Id = Guid.Parse(msg1SndHeaders[DmsHeaders.MessageId]!);
		var msg1RcvHeaders = await messagesRepository.GetQueuedMessageAsync("APP1");
		Assert.That(Guid.Parse(msg1RcvHeaders![DmsHeaders.MessageId]!), Is.EqualTo(msg1Id));
		var msg1RcvStream = await messagesRepository.GetMessageContentAsync(msg1Id);

		// APP1 to APP2
		var msg2Xml = XDocument.Load(msg1RcvStream!);
		msg2Xml.Element("Msg")!.Add(new XAttribute("Step1", "Y"));
		using var msg2Content = new MemoryStream(Encoding.UTF8.GetBytes(msg2Xml.ToString()));
		var msg2SndHeaders = await messagesRepository.PutMessageAsync(
			new DmsMessageMetadata
			{
				[DmsHeaders.MessageSenderId] = "APP1",
				[DmsHeaders.MessageRecipientId] = "APP2",
				[DmsHeaders.MessageParentId] = msg1Id.ToString()
			},
			msg2Content);
		var msg2Id = Guid.Parse(msg2SndHeaders[DmsHeaders.MessageId]!);
		var msg2RcvHeaders = await messagesRepository.GetQueuedMessageAsync("APP2");
		Assert.That(Guid.Parse(msg2RcvHeaders![DmsHeaders.MessageId]!), Is.EqualTo(msg2Id));
		var msg2RcvStream = await messagesRepository.GetMessageContentAsync(msg2Id);

		// APP2 to RCV
		var msg3Xml = XDocument.Load(msg2RcvStream!);
		msg3Xml.Element("Msg")!.Add(new XAttribute("Step2", "Y"));
		using var msg3Content = new MemoryStream(Encoding.UTF8.GetBytes(msg3Xml.ToString()));
		var msg3SndHeaders = await messagesRepository.PutMessageAsync(
			new DmsMessageMetadata
			{
				[DmsHeaders.MessageSenderId] = "APP2",
				[DmsHeaders.MessageRecipientId] = "RCV",
				[DmsHeaders.MessageParentId] = msg2Id.ToString()
			},
			msg3Content);
		var msg3Id = Guid.Parse(msg3SndHeaders[DmsHeaders.MessageId]!);
		var msg3RcvHeaders = await messagesRepository.GetQueuedMessageAsync("RCV");
		Assert.That(Guid.Parse(msg3RcvHeaders![DmsHeaders.MessageId]!), Is.EqualTo(msg3Id));

		var rcvPatchResult = await messagesRepository.PatchMessageMetadataAsync(msg3Id,
			new DmsMessageMetadata { [DmsHeaders.MessageStatus] = DmsStatus.Delivered });

		var sndFinalResult = await messagesRepository.GetMessageMetadataAsync(msg1Id, true);
		Assert.That(sndFinalResult![DmsHeaders.MessageStatus].ToString(), Is.EqualTo(DmsStatus.Delivered));

		messagesContext.ChangeTracker.Clear();

		Assert.That(messagesContext.eHubInboxMessage.OrderBy(i => i.EI_InsertUTC)
			.Select(i => new { SenderId = i.EI_CC_SenderNavigation.CC_ID, RecipientId = i.EI_CC_RecipientNavigation!.CC_ID, i.EI_Status })
			.ToList().Select(m => (m.SenderId, m.RecipientId, m.EI_Status)), Is.EquivalentTo(new[]
		{
			("SND", "APP1", 3 ),
			("APP1", "APP2", 3),
			("APP2", "RCV", 3 )
		}));
		Assert.That(messagesContext.eHubOutboxMessage.OrderBy(o => o.OI_InsertUTC)
			.Select(o => new { SenderId = o.OI_CC_SenderNavigation.CC_ID, RecipientId = o.OI_CC_RecipientNavigation.CC_ID, o.OI_Status })
			.ToList().Select(m => (m.SenderId, m.RecipientId, m.OI_Status)), Is.EquivalentTo(new[]
		{
			("SND", "APP1", 3 ),
			("APP1", "APP2", 3),
			("APP2", "RCV", 3 ),
			("eHub", "SND", 0 )
		}));
	}

	private eHubClient GetClient(string id, bool ackReqd = false) => new()
	{
		CC_PK = Guid.NewGuid(),
		CC_ID = id,
		CC_Odyssey_OH = Guid.Empty,
		CC_EmailAddress = "",
		CC_Password = "",
		CC_OwnerCategory = "Service",
		CC_SystemCategory = "eHub",
		CC_PermitInboxSender = true,
		CC_PermitInboxRecipient = true,
		CC_RequireStatusResponse = ackReqd
	};

	private async Task SetupStatusMessages()
	{
		await messagesContext.eHubClient.AddAsync(new eHubClient
		{
			CC_PK = new Guid("9819EFF9-9CD8-4622-B58E-32A251115791"),
			CC_ID = "eHub",
			CC_Odyssey_OH = Guid.Empty,
			CC_EmailAddress = "",
			CC_Password = "",
			CC_OwnerCategory = "Service",
			CC_SystemCategory = "eHub",
			CC_PermitInboxSender = true
		});
		await messagesContext.eHubMessageType.AddRangeAsync(new[] {
			new eHubMessageType
			{
				DT_PK = new Guid("14d86704-5b69-499e-b71e-05a708d004aa"),
				DT_Code = "MessageStatusSuccess"
			},
			new eHubMessageType
			{
				DT_PK = new Guid("6e0425d6-5d3e-42a2-8b74-2ff03c4520b9"),
				DT_Code = "MessageStatusFailed"
			}
		});
	}
}