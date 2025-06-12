using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataModel.Accessors;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ZACustoms.PipelineComponents;
using CargoWise.eHub.Products.ZACustoms.Tests.TestFiles;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using CargoWise.eHub.Shared.Mime;
using Common.Logging;
using Common.Logging.Simple;
using KellermanSoftware.CompareNetObjects;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ZACustoms.Tests.PipelineComponents
{
	[TestClass]
	public class AS2DisassemblerTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AS2Disassembler_Disassemble_Success()
		{
			// Arrange
			var inputData = TestFileHelpers.GetResourceStream("TestFiles.CONTRL-encrypted.txt");
			var logger = new TraceLogger(false, "AS2Disassembler", LogLevel.All, true, false, false, null);
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			AS2Disassembler.GetDbContext = () => mockContext;
			eHubTransactionsAccessor.GetDbContext = () => mockContext;
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			AS2Disassembler.GetLogger = (m) => logger;
			AS2Disassembler.GetUtcNow = () => new DateTime(2016, 11, 1);
			var guids = new Queue<Guid>(new []
			{
				new Guid("00000000-0000-0000-0000-000000000000"),
				new Guid("11111111-1111-1111-1111-111111111111"),
				new Guid("22222222-2222-2222-2222-222222222222"),
				new Guid("33333333-3333-3333-3333-333333333333"),
				new Guid("44444444-4444-4444-4444-444444444444"),
				new Guid("55555555-5555-5555-5555-555555555555")
			});
			AS2Disassembler.GetNewGuid = MimePart.GetNewGuid = () => guids.Dequeue();

			var clientCert = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx");
			var clientPublicCert = TestFileHelpers.GetResourceData("TestFiles.00505655TST.cer");
			var customsCert = TestFileHelpers.GetResourceData("TestFiles.SARS_2016_Sha2_Public.cer");
			var testCertificates = new TestDbSet<eHubCertificate>
			{
				new eHubCertificate {
					CE_Category = "ZACustoms",
					CE_Issuer = "CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA",
					CE_SerialNumber = "551AA44B",
					CE_BinaryContainer = clientCert,
					CE_Password = "miragef7",
					eHubClient = new eHubClient { CC_ID = "HYEDAUBLN" }
				},
				new eHubCertificate {
					CE_Category = "ZACustoms",
					CE_Issuer = "CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA",
					CE_SerialNumber = "551BFEF4",
					CE_BinaryContainer = customsCert
				}
			};
			mockContext.Stub(c => c.eHubCertificates).Return(testCertificates);
			var testClients = new TestDbSet<eHubClient>
			{
				new eHubClient { CC_ID = "ZACustoms", CC_PK = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD") },
				new eHubClient { CC_ID = "HYEDAUBLN", CC_PK = new Guid("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE") }
			};
			mockContext.Stub(c => c.eHubClients).Return(testClients);
			var testMessageTypes = new TestDbSet<eHubMessageType>
			{
				new eHubMessageType { DT_Code = "MESSAGETYPE", DT_PK = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF") }
			};
			mockContext.Stub(c => c.eHubMessageTypes).Return(testMessageTypes);
			var testOutboxMessages = new TestDbSet<eHubOutboxMessage>();
			mockContext.Stub(c => c.eHubOutboxMessages).Return(testOutboxMessages);
			var testInboxMessages = new TestDbSet<eHubInboxMessage>();
			mockContext.Stub(c => c.eHubInboxMessages).Return(testInboxMessages);

			var messageFactory = new MessageFactory();
			var message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.InterchangeID>("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
			message.Context.WriteProperty<HTTP.InboundHttpHeaders>(
@"Message-Id: <HYEDAUBLN_AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA>
AS2-Version: 1.2
MIME-Version: 1.0
EDIINT-Features: multiple-attachments
AS2-To: 00505655TST
AS2-From: SARS");
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = inputData;
			var pipelineContext = new PipelineContext();

			var component = new AS2Disassembler
			{
				ApplicationCode = "ZAC",
				SenderID = "ZACustoms",
				SchemaName = "MESSAGETYPE"
			};

			var actualInboxContent = new List<Tuple<char[], int, Guid>>();
			mockContext.Stub(x => x.ExecuteSqlCommand(Arg.Is("UPDATE eHubInboxMessage SET EI_Content.Write({0}, {1}, 0) WHERE EI_PK = {2}"), Arg<object[]>.Is.Anything))
				.Do(new Func<string, object[], int>((sql, args) =>
				{
					char[] inChars = (char[])args[0];
					char[] outChars = new char[inChars.Length];
					Array.Copy(inChars, outChars, inChars.Length);
					actualInboxContent.Add(new Tuple<char[], int, Guid>(outChars, (int)args[1], (Guid)args[2]));
					return 1;
				}));

			Tuple<char[], int, Guid> actualOutboxContent = null;
			mockContext.Stub(x => x.ExecuteSqlCommand(Arg.Is("UPDATE eHubOutboxMessage SET OI_Content.Write({0}, {1}, 0) WHERE OI_PK = {2}"), Arg<object[]>.Is.Anything))
				.Do(new Func<string, object[], int>((sql, args) =>
				{
					char[] inChars = (char[])args[0];
					char[] outChars = new char[inChars.Length];
					Array.Copy(inChars, outChars, inChars.Length);
					actualOutboxContent = new Tuple<char[], int, Guid>(outChars, (int)args[1], (Guid)args[2]);
					return 1;
				}));

			// Act
			component.Disassemble(pipelineContext, message);

			// Assert
			var expectedMdnBodyReport = TestFileHelpers.GetResourceText("TestFiles.CONTRL-MDN-Success.txt");
			string expectedMdnHeaders =
@"Message-Id: <HYEDAUBLN_BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB>
MIME-Version: 1.0
AS2-Version: 1.2
AS2-To: SARS
AS2-From: 00505655TST
EDIINT-Features: multiple-attachments";
			var expectedSignatureHdr =
@"Content-Type: application/pkcs7-signature; name=""smime.p7s""
Content-Transfer-Encoding: base64
";
			var expectedInboxMessages = new TestDbSet<eHubInboxMessage>
			{
				new eHubInboxMessage
				{
					EI_PK = new Guid("00000000-0000-0000-0000-000000000000"),
					EI_MessageTrackingID = "BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB",
					EI_CC_Sender = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
					EI_CC_Recipient = null,
					EI_EnvelopeTrackingID = "00000000-0000-0000-0000-000000000000",
					EI_MessageType = "MESSAGETYPE",
					EI_ApplicationCode = "ZAC",
					EI_Content = String.Empty,
					EI_InsertUTC = new DateTime(2016, 11, 1),
					EI_LastUpdateUTC = new DateTime(2016, 11, 1),
					EI_Status = 1
				}
			};
			var expectedOutboxMessages = new TestDbSet<eHubOutboxMessage>
			{
				new eHubOutboxMessage
				{
					OI_PK = new Guid("11111111-1111-1111-1111-111111111111"),
					OI_EI_InboxPK = new Guid("00000000-0000-0000-0000-000000000000"),
					OI_CC_Sender = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
					OI_CC_Recipient = new Guid("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE"),
					OI_DT_Target= new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
					OI_MessageTrackingID = "BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB",
					OI_Content = String.Empty,
					OI_InsertUTC = new DateTime(2016, 11, 1),
					OI_Status = 0
				}
			};
			var expectedInboxData = TestFileHelpers.GetResourceText("TestFiles.CONTRL-encrypted-compressed-encoded.txt").ToCharArray();
			var expectedInboxContent = new List<Tuple<char[], int, Guid>>
			{
				new Tuple<char[], int, Guid>(expectedInboxData.Take(2048).ToArray(), 0, new Guid("00000000-0000-0000-0000-000000000000")),
				new Tuple<char[], int, Guid>(expectedInboxData.Skip(2048).Take(2048).ToArray(), 2048, new Guid("00000000-0000-0000-0000-000000000000")),
				new Tuple<char[], int, Guid>(expectedInboxData.Skip(4096).Take(2048).ToArray(), 4096, new Guid("00000000-0000-0000-0000-000000000000"))
			};

			var actualMdn = component.GetNext(pipelineContext);
			var actualMdnParts = new StreamReader(actualMdn.BodyPart.Data).ReadToEnd().Split(new[] { "\r\n--_55555555-5555-5555-5555-555555555555_\r\n", "\r\n--_55555555-5555-5555-5555-555555555555_--\r\n", "--_55555555-5555-5555-5555-555555555555_\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual(expectedMdnBodyReport, actualMdnParts[0]);
			Assert.AreEqual(expectedMdnHeaders, actualMdn.Context.ReadPropertyString<HTTP.UserHttpHeaders>());
			Assert.AreEqual("multipart/signed; protocol=\"application/pkcs7-signature\"; micalg=\"sha256\"; boundary=\"_55555555-5555-5555-5555-555555555555_\"", actualMdn.Context.ReadPropertyString<HTTP.ContentType>());
			Assert.AreEqual("True", actualMdn.Context.ReadPropertyString<BTS.RouteDirectToTP>());
			Assert.IsTrue(actualMdnParts[1].StartsWith(expectedSignatureHdr));
			VerifySignature(Encoding.UTF8.GetBytes(actualMdnParts[0]), Convert.FromBase64String(actualMdnParts[1].Substring(expectedSignatureHdr.Length).Trim()), clientPublicCert);
			Assert.IsNull(component.GetNext(pipelineContext));
			Assert.AreEqual("ZACustomsAS2Receive", actualMdn.Context.ReadPropertyString<BTS.MessageType>());
			Assert.AreEqual("ZACustoms", actualMdn.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("00000000-0000-0000-0000-000000000000", actualMdn.Context.ReadPropertyString<InternalTrackingID>());

			var compare = new CompareLogic();
			var compareResult = compare.Compare(expectedInboxMessages, testInboxMessages);
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
			compareResult = compare.Compare(expectedOutboxMessages, testOutboxMessages);
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
			compareResult = compare.Compare(expectedInboxContent, actualInboxContent);
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
			var expectedOutboxContent = TestFileHelpers.GetResourceText("TestFiles.CONTRL-compressed-encoded.txt").ToCharArray();
			compareResult = compare.Compare(new Tuple<char[], int, Guid>(expectedOutboxContent, 0, new Guid("11111111-1111-1111-1111-111111111111")), actualOutboxContent);
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
			mockContext.AssertWasCalled(x => x.SaveChanges(), x => x.Repeat.Twice());
			mockTransaction.AssertWasCalled(x => x.Commit(), x => x.Repeat.Twice());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AS2Disassembler_Disassemble_ZACustomsTestSuccess()
		{
			// Arrange
			var inputData = TestFileHelpers.GetResourceStream("TestFiles.CONTRL-encrypted.txt");
			var logger = new TraceLogger(false, "AS2Disassembler", LogLevel.All, true, false, false, null);
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			AS2Disassembler.GetDbContext = () => mockContext;
			eHubTransactionsAccessor.GetDbContext = () => mockContext;
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			AS2Disassembler.GetLogger = (m) => logger;
			AS2Disassembler.GetUtcNow = () => new DateTime(2016, 11, 1);
			var guids = new Queue<Guid>(new[]
			{
				new Guid("00000000-0000-0000-0000-000000000000"),
				new Guid("11111111-1111-1111-1111-111111111111"),
				new Guid("22222222-2222-2222-2222-222222222222"),
				new Guid("33333333-3333-3333-3333-333333333333"),
				new Guid("44444444-4444-4444-4444-444444444444"),
				new Guid("55555555-5555-5555-5555-555555555555")
			});
			AS2Disassembler.GetNewGuid = MimePart.GetNewGuid = () => guids.Dequeue();

			var clientCert = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx");
			var customsCert = TestFileHelpers.GetResourceData("TestFiles.SARS_2016_Sha2_Public.cer");
			var testCertificates = new TestDbSet<eHubCertificate>
			{
				new eHubCertificate {
					CE_Category = "ZACustoms",
					CE_Issuer = "CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA",
					CE_SerialNumber = "551AA44B",
					CE_BinaryContainer = clientCert,
					CE_Password = "miragef7",
					eHubClient = new eHubClient { CC_ID = "HYEDAUBLN" }
				},
				new eHubCertificate {
					CE_Category = "ZACustomsTest",
					CE_Issuer = "CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA",
					CE_SerialNumber = "551AA44B",
					CE_BinaryContainer = clientCert,
					CE_Password = "miragef7",
					eHubClient = new eHubClient { CC_ID = "HYEDAUJEY" }
				},
				new eHubCertificate {
					CE_Category = "ZACustomsTest",
					CE_Issuer = "CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA",
					CE_SerialNumber = "551BFEF4",
					CE_BinaryContainer = customsCert
				}
			};
			mockContext.Stub(c => c.eHubCertificates).Return(testCertificates);
			var testClients = new TestDbSet<eHubClient>
			{
				new eHubClient { CC_ID = "ZACustoms", CC_PK = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD") },
				new eHubClient { CC_ID = "HYEDAUBLN", CC_PK = new Guid("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE") },
				new eHubClient { CC_ID = "ZACustomsTest", CC_PK = new Guid("77777777-7777-7777-7777-777777777777") },
				new eHubClient { CC_ID = "HYEDAUJEY", CC_PK = new Guid("88888888-8888-8888-8888-888888888888") }
			};
			mockContext.Stub(c => c.eHubClients).Return(testClients);
			var testMessageTypes = new TestDbSet<eHubMessageType>
			{
				new eHubMessageType { DT_Code = "MESSAGETYPE", DT_PK = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF") }
			};
			mockContext.Stub(c => c.eHubMessageTypes).Return(testMessageTypes);
			var testOutboxMessages = new TestDbSet<eHubOutboxMessage>();
			mockContext.Stub(c => c.eHubOutboxMessages).Return(testOutboxMessages);
			var testInboxMessages = new TestDbSet<eHubInboxMessage>();
			mockContext.Stub(c => c.eHubInboxMessages).Return(testInboxMessages);

			var messageFactory = new MessageFactory();
			var message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.InterchangeID>("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
			message.Context.WriteProperty<BTS.SourceParty>("ZACustomsTest");
			message.Context.WriteProperty<HTTP.InboundHttpHeaders>(
@"Message-Id: <HYEDAUJEY_AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA>
AS2-Version: 1.2
MIME-Version: 1.0
EDIINT-Features: multiple-attachments
AS2-To: 00505655TST
AS2-From: SARS");
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = inputData;
			var pipelineContext = new PipelineContext();

			var component = new AS2Disassembler
			{
				ApplicationCode = "ZAC",
				SenderID = "ZACustomsTest",
				SchemaName = "MESSAGETYPE"
			};

			var actualInboxContent = new List<Tuple<char[], int, Guid>>();
			mockContext.Stub(x => x.ExecuteSqlCommand(Arg.Is("UPDATE eHubInboxMessage SET EI_Content.Write({0}, {1}, 0) WHERE EI_PK = {2}"), Arg<object[]>.Is.Anything))
				.Do(new Func<string, object[], int>((sql, args) =>
				{
					char[] inChars = (char[])args[0];
					char[] outChars = new char[inChars.Length];
					Array.Copy(inChars, outChars, inChars.Length);
					actualInboxContent.Add(new Tuple<char[], int, Guid>(outChars, (int)args[1], (Guid)args[2]));
					return 1;
				}));

			Tuple<char[], int, Guid> actualOutboxContent = null;
			mockContext.Stub(x => x.ExecuteSqlCommand(Arg.Is("UPDATE eHubOutboxMessage SET OI_Content.Write({0}, {1}, 0) WHERE OI_PK = {2}"), Arg<object[]>.Is.Anything))
				.Do(new Func<string, object[], int>((sql, args) =>
				{
					char[] inChars = (char[])args[0];
					char[] outChars = new char[inChars.Length];
					Array.Copy(inChars, outChars, inChars.Length);
					actualOutboxContent = new Tuple<char[], int, Guid>(outChars, (int)args[1], (Guid)args[2]);
					return 1;
				}));

			// Act
			component.Disassemble(pipelineContext, message);

			// Assert
			var expectedOutboxMessages = new TestDbSet<eHubOutboxMessage>
			{
				new eHubOutboxMessage
				{
					OI_PK = new Guid("11111111-1111-1111-1111-111111111111"),
					OI_EI_InboxPK = new Guid("00000000-0000-0000-0000-000000000000"),
					OI_CC_Sender = new Guid("77777777-7777-7777-7777-777777777777"),
					OI_CC_Recipient = new Guid("88888888-8888-8888-8888-888888888888"),
					OI_DT_Target= new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
					OI_MessageTrackingID = "BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB",
					OI_Content = String.Empty,
					OI_InsertUTC = new DateTime(2016, 11, 1),
					OI_Status = 0
				}
			};

			var compare = new CompareLogic();
			var compareResult = compare.Compare(expectedOutboxMessages, testOutboxMessages);
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AS2Disassembler_Disassemble_Error()
		{
			// Arrange
			var inputData = TestFileHelpers.GetResourceStream("TestFiles.CONTRL-encrypted.txt");
			var logger = new TraceLogger(false, "AS2Disassembler", LogLevel.All, true, false, false, null);
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			AS2Disassembler.GetDbContext = eHubTransactionsAccessor.GetDbContext = () => mockContext;
			AS2Disassembler.GetLogger = (m) => logger;
			AS2Disassembler.GetUtcNow = eHubTransactionsAccessor.GetDateTimeUtcNow = () => new DateTime(2016, 11, 1);
			var guids = new Queue<Guid>(new[]
			{
				new Guid("99999999-9999-9999-9999-999999999999"),
				new Guid("11111111-1111-1111-1111-111111111111"),
				new Guid("22222222-2222-2222-2222-222222222222"),
				new Guid("33333333-3333-3333-3333-333333333333"),
				new Guid("44444444-4444-4444-4444-444444444444"),
				new Guid("55555555-5555-5555-5555-555555555555")
			});
			AS2Disassembler.GetNewGuid = MimePart.GetNewGuid = eHubTransactionsAccessor.GetNewGuid = () => guids.Dequeue();

			var clientCert = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx");
			var clientPublicCert = TestFileHelpers.GetResourceData("TestFiles.00505655TST.cer");
			var testCertificates = new TestDbSet<eHubCertificate>
			{
				new eHubCertificate {
					CE_Category = "ZACustoms",
					CE_Issuer = "CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA",
					CE_SerialNumber = "551AA44B",
					CE_BinaryContainer = clientCert,
					CE_Password = "miragef7",
					eHubClient = new eHubClient { CC_ID = "HYEDAUBLN" }
				}
			};
			mockContext.Stub(c => c.eHubCertificates).Return(testCertificates);
			var testClients = new TestDbSet<eHubClient>
			{
				new eHubClient { CC_ID = "ZACustoms", CC_PK = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD") },
				new eHubClient { CC_ID = "HYEDAUBLN", CC_PK = new Guid("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE") }
			};
			mockContext.Stub(c => c.eHubClients).Return(testClients);
			var testInboxMessages = new TestDbSet<eHubInboxMessage>();
			mockContext.Stub(c => c.eHubInboxMessages).Return(testInboxMessages);
			var testOutboxMessages = new TestDbSet<eHubOutboxMessage>();
			mockContext.Stub(c => c.eHubOutboxMessages).Return(testOutboxMessages);

			var messageFactory = new MessageFactory();
			var message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.Context.WriteProperty<HTTP.InboundHttpHeaders>(
@"Message-Id: <HYEDAUBLN_AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA>
AS2-Version: 1.2
MIME-Version: 1.0
EDIINT-Features: multiple-attachments
AS2-To: 00505655TST
AS2-From: SARS");
			message.Context.WriteProperty<BTS.InterchangeID>("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = inputData;
			var pipelineContext = new PipelineContext();

			object[] actualErrorArgs = null;
			mockContext.Stub(x => x.ExecuteSqlCommand(Arg.Is("EXEC [dbo].[InsertError] @ErrorPK = @p0, @Source = 'BIZ', @ErrorType = 'Fai', @Description = @p1, @InboxPK = @p2, @OutboxPK = @p3, @CurrentDateTimeUTC = @p4"), Arg<object[]>.Is.Anything))
				.Do(new Func<string, object[], int>((sql, objs) => { actualErrorArgs = objs; return 1; }));


			var component = new AS2Disassembler
			{
				ApplicationCode = "ZAC",
				SenderID = "ZACustoms",
				SchemaName = "MESSAGETYPE"
			};

			// Act
			component.Disassemble(pipelineContext, message);

			// Assert
			var expectedMdnBodyReport = TestFileHelpers.GetResourceText("TestFiles.CONTRL-MDN-Error.txt");
			string expectedMdnHeaders =
@"Message-Id: <HYEDAUBLN_BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB>
MIME-Version: 1.0
AS2-Version: 1.2
AS2-To: SARS
AS2-From: 00505655TST
EDIINT-Features: multiple-attachments";
			var expectedSignatureHdr =
@"Content-Type: application/pkcs7-signature; name=""smime.p7s""
Content-Transfer-Encoding: base64
";
			var expectedInboxMessages = new TestDbSet<eHubInboxMessage>
			{
				new eHubInboxMessage
				{
					EI_PK = new Guid("99999999-9999-9999-9999-999999999999"),
					EI_MessageTrackingID = "BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB",
					EI_CC_Sender = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
					EI_CC_Recipient = null,
					EI_EnvelopeTrackingID = "00000000-0000-0000-0000-000000000000",
					EI_MessageType = "MESSAGETYPE",
					EI_ApplicationCode = "ZAC",
					EI_Content = String.Empty,
					EI_InsertUTC = new DateTime(2016, 11, 1),
					EI_LastUpdateUTC = new DateTime(2016, 11, 1),
					EI_Status = 1
				}
			};

			var actualMdn = component.GetNext(pipelineContext);
			var actualMdnParts = new StreamReader(actualMdn.BodyPart.Data).ReadToEnd().Split(new[] { "\r\n--_44444444-4444-4444-4444-444444444444_\r\n", "\r\n--_44444444-4444-4444-4444-444444444444_--\r\n", "--_44444444-4444-4444-4444-444444444444_\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual(expectedMdnBodyReport, actualMdnParts[0]);
			Assert.AreEqual(expectedMdnHeaders, actualMdn.Context.ReadPropertyString<HTTP.UserHttpHeaders>());
			Assert.AreEqual("multipart/signed; protocol=\"application/pkcs7-signature\"; micalg=\"sha256\"; boundary=\"_44444444-4444-4444-4444-444444444444_\"", actualMdn.Context.ReadPropertyString<HTTP.ContentType>());
			Assert.AreEqual("True", actualMdn.Context.ReadPropertyString<BTS.RouteDirectToTP>());
			Assert.IsTrue(actualMdnParts[1].StartsWith(expectedSignatureHdr));
			VerifySignature(Encoding.UTF8.GetBytes(actualMdnParts[0]), Convert.FromBase64String(actualMdnParts[1].Substring(expectedSignatureHdr.Length).Trim()), clientPublicCert);
			Assert.IsNull(component.GetNext(pipelineContext));

			var compare = new CompareLogic();
			var compareResult = compare.Compare(expectedInboxMessages, testInboxMessages);
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
			mockContext.AssertWasCalled(x => x.ExecuteSqlCommand(Arg.Is("EXEC [dbo].[InsertError] @ErrorPK = @p0, @Source = 'BIZ', @ErrorType = 'Fai', @Description = @p1, @InboxPK = @p2, @OutboxPK = @p3, @CurrentDateTimeUTC = @p4"), Arg<object[]>.Is.Anything));
			Assert.AreEqual(new Guid("55555555-5555-5555-5555-555555555555"), actualErrorArgs[0], "Error PK");
			Assert.IsTrue(actualErrorArgs[1].ToString().StartsWith("CargoWise.eHub.Products.ZACustoms.PipelineComponents.AS2Disassembler+ExceptionForMdn: processed/error: authentication-failed ---> System.Exception: Signing certificate not available"), "Error description does not contain with exception message");
			Assert.IsTrue(actualErrorArgs[1].ToString().Contains("\r\n----------BIZTALK PROPERTIES----------\r\n"), "Error description does not contain delimiter");
			Assert.IsTrue(actualErrorArgs[1].ToString().Contains("http://cargowise.com/ehub/processing/2010/06#SubjectIdentifier = {IssueName: CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA, SerialNumber: 551BFEF4}"), "Error description does not contain SubjectIdentifier");
			Assert.AreEqual(new Guid("99999999-9999-9999-9999-999999999999"), actualErrorArgs[2], "Inbox PK");
			Assert.IsNull(actualErrorArgs[3], "Outbox PK");
			Assert.AreEqual("2016-11-01T00:00:00", actualErrorArgs[4], "Outbox PK");
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AS2Disassembler_Disassemble_Error_InvalidCertificate()
		{
			// Arrange
			var inputData = TestFileHelpers.GetResourceStream("TestFiles.CONTRL-encrypted.txt");
			var logger = new TraceLogger(false, "AS2Disassembler", LogLevel.All, true, false, false, null);
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			AS2Disassembler.GetDbContext = eHubTransactionsAccessor.GetDbContext = () => mockContext;
			AS2Disassembler.GetLogger = (m) => logger;
			AS2Disassembler.GetUtcNow = eHubTransactionsAccessor.GetDateTimeUtcNow = () => new DateTime(2016, 11, 1);
			var guids = new Queue<Guid>(new[]
			{
				new Guid("99999999-9999-9999-9999-999999999999"),
				new Guid("11111111-1111-1111-1111-111111111111"),
				new Guid("22222222-2222-2222-2222-222222222222"),
				new Guid("33333333-3333-3333-3333-333333333333"),
				new Guid("44444444-4444-4444-4444-444444444444"),
				new Guid("55555555-5555-5555-5555-555555555555")
			});
			AS2Disassembler.GetNewGuid = MimePart.GetNewGuid = eHubTransactionsAccessor.GetNewGuid = () => guids.Dequeue();

			var testClients = new TestDbSet<eHubClient>
			{
				new eHubClient { CC_ID = "ZACustoms", CC_PK = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD") },
				new eHubClient { CC_ID = "HYEDAUBLN", CC_PK = new Guid("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE") }
			};
			mockContext.Stub(c => c.eHubClients).Return(testClients);
			var testInboxMessages = new TestDbSet<eHubInboxMessage>();
			mockContext.Stub(c => c.eHubInboxMessages).Return(testInboxMessages);
			var testOutboxMessages = new TestDbSet<eHubOutboxMessage>();
			mockContext.Stub(c => c.eHubOutboxMessages).Return(testOutboxMessages);

			var messageFactory = new MessageFactory();
			var message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.Context.WriteProperty<HTTP.InboundHttpHeaders>(
@"Message-Id: <HYEDAUBLN_AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA>
AS2-Version: 1.2
MIME-Version: 1.0
EDIINT-Features: multiple-attachments
AS2-To: 00505655TST
AS2-From: SARS");
			message.Context.WriteProperty<BTS.InterchangeID>("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = inputData;
			var pipelineContext = new PipelineContext();

			object[] actualErrorArgs = null;
			mockContext.Stub(x => x.ExecuteSqlCommand(Arg.Is("EXEC [dbo].[InsertError] @ErrorPK = @p0, @Source = 'BIZ', @ErrorType = 'Fai', @Description = @p1, @InboxPK = @p2, @OutboxPK = @p3, @CurrentDateTimeUTC = @p4"), Arg<object[]>.Is.Anything))
				.Do(new Func<string, object[], int>((sql, objs) => { actualErrorArgs = objs; return 1; }));


			var component = new AS2DisassemblerForTesting()
			{
				ApplicationCode = "ZAC",
				SenderID = "ZACustoms",
				SchemaName = "MESSAGETYPE"
			};

			// Act
			component.Disassemble(pipelineContext, message);
			var certificateError = actualErrorArgs[1].ToString();

			Assert.IsTrue(certificateError.Contains("Recipient certificate:"));
			Assert.IsTrue(certificateError.Contains("Password: 01251883WTG\r\n\tIssuer: CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA\r\n\tSerial Number: 551FE925\r\n\tSubject Key Identifier:"));
		}

		private static void VerifySignature(byte[] content, byte[] signature, byte[] certificate)
		{
			var contentInfo = new ContentInfo(content);
			var signedCms = new SignedCms(contentInfo, true);
			signedCms.Decode(signature);
			var x509CertCollection = new X509Certificate2Collection();
			x509CertCollection.Import(certificate);
			signedCms.CheckSignature(x509CertCollection, true);
		}
	}

	public class AS2DisassemblerForTesting : AS2Disassembler
	{

		protected override eHubCertificate GetCertificateFromIdentifier(IBaseMessage pInMsg, SubjectIdentifier subjectIdentifier, bool privateKey,
			ILog logger)
		{
			var clientCert = TestFileHelpers.GetResourceData("TestFiles.01251883WTG.pfx");
			return new eHubCertificate
			{
				CE_Category = "ZACustoms",
				CE_Issuer = "CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA",
				CE_SerialNumber = "551FE925",
				CE_BinaryContainer = clientCert,
				CE_Password = "01251883WTG",
				eHubClient = new eHubClient {CC_ID = "HYEDAUBLN"}
			};
		}
	}
}
