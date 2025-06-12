using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.CNS
{
	[TestFixture]
	public class TransportAcknowledgementHandlerTests : HandlerTestBase
	{
		[Test]
		public void TestTransportHandlerHandleSuccessResponse()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-CNS",
				SubscriptionType = "GBCCNS",
				SubscriptionValue = "CAW-1538042784198",
				SubscribedMessage = GetMessage("CNS_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage("CNS_TransportSuccessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CNS_TransportSuccessResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CNS_TransportSuccessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustomsTest-CNS"),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					var messageDdoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
					var namespaceManager = new XmlNamespaceManager(new NameTable());
					namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
					Assert.AreEqual("Accepted", messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Status", namespaceManager).Value);
					Assert.AreEqual("a2a62d75-d008-4666-9578-80f949f0758b", messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/ConversationId", namespaceManager).Value);
					Assert.AreEqual("recipient", message.ClientID);
					Assert.IsNotNull(messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Response/notifications", namespaceManager));
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsTransportResponse", message.SchemaName);
				});

			config.SubscriptionAccessor.Expect(_ =>
				_.InsertSubscribedClients(
					Arg<SqlTransaction>.Is.Anything,
					Arg<string>.Is.Equal("GBCCNC"),
					Arg<string>.Is.Equal("GBCustomsTest-CNS"),
					Arg<string[]>.Is.Equal(config.ExpectedRecipients),
					Arg<string>.Is.Equal("a2a62d75-d008-4666-9578-80f949f0758b"),
					Arg<string>.Is.Equal("CAW-1538042784198"),
					Arg<string>.Is.Equal("CSP-ID"))).Repeat.Once();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestTransportHandlerHandleErrorResponseServiceUnavailable()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-CNS",
				SubscriptionType = "GBCCNS",
				SubscriptionValue = "CAW-1571358882612",
				SubscribedMessage = GetMessage("CNS_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage("CNS_TransportErrorResponseServiceUnavailable.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CNS_TransportErrorResponseServiceUnavailable.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CNS_TransportErrorResponseBodyServiceUnavailable.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustomsTest-CNS"),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
					var namespaceManager = new XmlNamespaceManager(new NameTable());
					namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
					Assert.AreEqual("Error", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Status", namespaceManager).Value);
					Assert.AreEqual("503", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Error", namespaceManager).Value);
					Assert.AreEqual("CDS_SERVICE_UNAVAILABLE", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/ErrorText", namespaceManager).Value);
					Assert.AreEqual("recipient", message.ClientID);
					Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Response/notifications", namespaceManager));
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsTransportResponse", message.SchemaName);
				});

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestTransportHandlerHandleErrorResponse()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-CNS",
				SubscriptionType = "GBCCNS",
				SubscriptionValue = "CAW-1538042784198",
				SubscribedMessage = GetMessage("CNS_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage("CNS_TransportErrorResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CNS_TransportErrorResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CNS_TransportErrorResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustomsTest-CNS"),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).WhenCalled(x =>
				{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDdoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual("Error", messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Status", namespaceManager).Value);
				Assert.AreEqual("401", messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Error", namespaceManager).Value);
				Assert.AreEqual("Something's Wrong", messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/ErrorText", namespaceManager).Value);
				Assert.AreEqual("recipient", message.ClientID);
				Assert.IsNotNull(messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Response/notifications", namespaceManager));
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsTransportResponse", message.SchemaName);
				});
			
			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestTransportHandlerValidateCSPId_WhenNoSubscriberIsFound_ShouldAcceptMessage()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-CNS",
				SubscriptionType = "GBCCNS",
				SubscriptionValue = "CAW-1538042784198",
				SubscribedMessage = "",
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "GBCustoms" },
				RequestContent = GetMessage("CNS_TransportSuccessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CNS_TransportSuccessResponse.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
			};

			handler.SetUpMock(config);

			var expectedError = new eHubError()
			{
				EE_Alerted = false,
				EE_Description = "GBCustoms Inbound Message - Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message.",
				EE_ErrorDetail = "<ErrorDetail>GBCustoms Inbound Message - Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message.</ErrorDetail>",
				EE_ErrorType = "EXP",
				EE_Source = "HUB"
			};

			var expectedMessage = new eHubInboxMessage()
			{
				EI_CC_Recipient = new Guid("00000000-0000-0000-0000-000000000000"),
				EI_ApplicationCode = "HUB",
				EI_MessageType = "CDS",
				EI_CC_Sender = new Guid("00000000-0000-0000-0000-000000000000"),
				EI_Status = 255,
				EI_Content = "H4sIAAAAAAAEAG1S21KjQBB91ir/YYpXFzMQCGAFrU3wkpSiUXaJeRuYCRl3mMEwxJiv3ybR3bhr8TB09+nTpy8X65xVmiuJIqYJF/Xp0eEtq2tSMEQVq5FUGpGqYmSJtEILsmJILXnBJdGMovlSlSjlNdMsX6AroTIiII5ywZnUiO/yK7YsuW7xQLFkOePAUu6q1N9QrmTdlFwWSC/++E+ODttvJDPVSIo+NF0zQtmyVfl/bKDoGwT65+tSoBWgoK3QsE6wgZjMFYUKofEjuTR9A9WaSEqEkiw03lhtnJ9BImjlc56Tdh41aK14HhrD7+nUAI2N1EBmAO7gExBxGhpu19mFDvovDWsYjWA8CS/ZmY0t38SBaXuJZZ3i3qntnXhO9xiDgfudf9Bbhgz6OLu/Hou8O1hlMhb3z+vnzH6Ad/R6+/y6erKDX7PE8bP05ya3L+UscS9maVw/TccLeiVWGR9saDrWZBpvSBo0o+tYZHJS0Cu/mOCH5H4Y6BnEnlLacjsT7CaP6eR4gi+nN8mFdbspXuNo1L2LJuu7pPBvuosoEfF4Fjnwv6erCMN+Zyt3K3yxW87W+LCQJCXMeKhku5HtxEaRgVZENOAmNunZ1HNNirFvOr1ezwxczzd9PA+cYI4918+MzleEU3NAaMHMPTLY1NdYKK7hHM3krWJ/S1eVeF9hBw7m68z1MH4c0f0KpuV2fezYnu9Ygf+e1e/std7v7J9He1efHC3mN974aw52AwAA"
			};

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(x => x.eHubClients).Return(CreateGBCustomsClient());
			mockContext.Stub(x => x.eHubInboxMessages).Return(new TestDbSet<eHubInboxMessage>());
			mockContext.Stub(x => x.eHubErrors).Return(new TestDbSet<eHubError>());
			handler.ContextFactory = () => mockContext;

			var response = handler.ExecuteProcessTest();

			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			Assert.AreEqual(1, mockContext.eHubErrors.Count());
			Assert.AreEqual(1, mockContext.eHubInboxMessages.Count());

			var error = mockContext.eHubErrors.First();
			var inboxMessage = mockContext.eHubInboxMessages.First();

			Assert.AreEqual(expectedError.EE_Alerted, error.EE_Alerted);
			Assert.AreEqual(expectedError.EE_Description, error.EE_Description);
			Assert.AreEqual(expectedError.EE_ErrorDetail, error.EE_ErrorDetail);
			Assert.AreEqual(expectedError.EE_ErrorType, error.EE_ErrorType);
			Assert.AreEqual(expectedError.EE_Source, error.EE_Source);

			Assert.AreEqual(expectedMessage.EI_Status, inboxMessage.EI_Status);
			Assert.AreEqual(expectedMessage.EI_CC_Sender, inboxMessage.EI_CC_Sender);
			Assert.AreEqual(expectedMessage.EI_CC_Recipient, inboxMessage.EI_CC_Recipient);
			Assert.AreEqual(expectedMessage.EI_ApplicationCode, inboxMessage.EI_ApplicationCode);
			Assert.AreEqual(expectedMessage.EI_MessageType, inboxMessage.EI_MessageType);
			Assert.AreEqual(expectedMessage.EI_Content, inboxMessage.EI_Content);

			config.VerifyAll();
		}


		[SetUp]
		public void SetupTestHandler()
		{
			handler = MockRepository.GeneratePartialMock<TransportAcknowledgementHandlerForTest>(ProviderType.CNS);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}

			foreach (var setting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
		}

		private TransportAcknowledgementHandlerForTest handler;
		Dictionary<string, string> AppSettingConfiguration = new Dictionary<string, string>
		{
			{"SqlDeadlockRetryCount", "1"},
			{"SqlDeadlockRetryWait", "1"},
			{"GBCustomsID", "GBCustoms-CNS"},
			{"GBCustomsTestID", "GBCustomsTest-CNS"},
			{"GBCustomsRegistrationType", "GBCustoms-CNS"},
			{"AuthorisationHeaderKey", "Authorization"},
			{"GBCustomsSubscriptionTypeByCSPID", "GBCCNS" },
			{"GBCustomsSubscriptionTypeByConversationID", "GBCCNC"},
			{"GetRecipientsRetryAttempts", "3"},
			{"GetRecipientsRetryWaitTime", "1"},
			{"SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};
		Dictionary<string, string> MessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"Authorization", "HeaderAuthorization"}
		};
		public override string Provider => "CNS";

		private TestDbSet<eHubClient> CreateGBCustomsClient()
		{
			var ehubClients = new TestDbSet<eHubClient>();
			ehubClients.Add(new eHubClient()
			{
				CC_PK = new Guid("00000000-0000-0000-0000-000000000000"),
				CC_ID = "GBCustoms"
			});

			return ehubClients;
		}
	}
}
