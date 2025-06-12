using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using System.Runtime.InteropServices;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using NUnit.Framework.Interfaces;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.CNS
{
	[TestFixture]
	public class BusinessNotificationHandlerTests : HandlerTestBase
	{
		[Test]
		public void TestBusinessHandlerHandleBusinessResponse()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-CNS",
				SubscriptionType = "GBCCNC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage("CNS_BusinessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CNS_BusinessResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CNS_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true
			};

			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustomsTest-CNS"), Arg<string>.Is.Equal("GBCustomsTest"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				namespaceManager.AddNamespace("urn", "urn:wco:datamodel:WCO:DocumentMetaData-DMS:2");
				Assert.AreEqual("40ad9ef5-2d57-4286-a12a-31675ffe4d9b", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:ConversationID", namespaceManager).Value);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody/urn:MetaData", namespaceManager));
				Assert.AreEqual("eHubTrackingID", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingId", namespaceManager).Value);
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
				Assert.AreEqual("recipient", message.ClientID);
			});

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerHandleBusinessResponse_LogProviderAndResponseStatus()
		{
			var logger = MockRepository.GenerateMock<ILog>();
			logger.Expect(_ => _.TraceFormat("GetResponseStatus returned Provider: {0}, Status: {1}, StatusInInteger: {2}", ProviderType.CNS, HttpStatusCode.Accepted, (int)HttpStatusCode.Accepted)).Repeat.Once();

			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-CNS",
				SubscriptionType = "GBCCNC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage("CNS_BusinessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CNS_BusinessResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CNS_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				ClientPermittedRecipient = true,
				UseMockForLogger = true,
				Logger = logger,
			};

			handler.SetUpMock(config);

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerHandleBusinessResponse_WhenConversationIdIsMissing_ShouldThrowHttpException()
		{
			var cspResponse = XDocument.Parse(GetMessage("CNS_BusinessResponse.xml"));

			cspResponse
				.Descendants("header")
				.Where(node => node
					.Attributes()
					.Any(attribute => attribute.Name == "name" && attribute.Value == "ConversationID"))
				.ToList()
				.ForEach(node => node.Remove());

			var cspResponseBody = XDocument.Parse(GetMessage("CNS_BusinessResponseBody.xml"));
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-CNS",
				SubscriptionType = "GBCCNS",
				SubscriptionValue = "CAW-1537963551839",
				ClientRegistrationType = "GBCustoms-CNS",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = cspResponse.ToString(),
				MessageDoc = cspResponse,
				MessageBodyDoc = cspResponseBody,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true
			};

			handler.SetUpMock(config);

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());
			Assert.AreEqual(501, exception.GetHttpCode());
			Assert.AreEqual("Provider: CNS. No Conversation ID found from the received notification.", exception.Message);
		}

		[Test] 
		public void TestCSPNotificationHandlerValidateCSPId_WhenNoSubscriberIsFound_FallbackNotSubscribed_DeriveRecipientFromFallback()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-CNS",
				SubscriptionType = "GBCCNC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				SubscribedMessage = null,
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "Sani_ST2_" },
				RequestContent = GetMessage("CNS_BusinessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CNS_BusinessResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CNS_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientAccessor = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientExist = true,
				ClientPermittedRecipient = true
			};

			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustoms-CNS"), Arg<string>.Is.Equal("GBCustoms"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual(config.ExpectedRecipients[0], message.ClientID);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager));
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
			}).Repeat.Once();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessNotificationHandler_WhenNoSubscriberIsFound_WhenRequestIsNotFromWTG_ShouldAcceptMessage()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-CNS",
				SubscriptionType = "GBCCNC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				SubscribedMessage = "",
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "GBCustoms" },
				RequestContent = GetMessage("CNS_BusinessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CNS_BusinessResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CNS_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientAccessor = false,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientExist = true
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
				EI_Content = "H4sIAAAAAAAEAK1WWXOiShR+nqma/0D5muuERYykkplSEcQRjMgivLF0ZGmWGwGFX38PaMwyuZWHmUpZFbrP8p3vO919ZkcP5UWYpQSPCifE+9tvX2W03zs7RPgZ2hNpVhBOniPniSgyInAqRGRP4S5MnQL5xONTlhBmuEcF8gJCxJnrYNgnPByitCDCk3+OnpKwaO0hxBPyUAhRklOW/T+El6X7MgnTHVEEl/Xv3762f1LqZmXqE8+Y5sjx0VOL8ve9SebXsHH385hgogIrKOu+R30newRKvcyHDPc9XRP6ox6xL5zUd3CWovtejfa9nz/AEbCGj6HntHzsAWseeve96djc9gBjmRYQrAd2X94YEqF/3xuyp50vd/+WqEQ+D+xoYYJ+0CQ16pNcnx5q1OCWZm8Z+jvLkVckdUuSd9fvrLsILpTx42Fu1G6UK/ZWDVRRIK3NZOA1eWPRAXZNQXZpJfBFXLkRGfrbRblK/cht8tgCe9fkYts8DA1SeVjFamUxRmGbLKmZBmnpKvjNCnVGaatI2i3DQaWKXOSbFHZT9SWfMaq3pKroNTvw6HUozYPCFdlmlba4/ijnq1hB42hk6MxV0uOzasn4jF+zjFyzlZd4lRyND/KGszT9qJ3qnhWOyTa+KJQWrb+J87BZZP5cPazCUQVRmGXqNcuEq+16VMv8+LikAkUTlMgRjcLaSFfTXTxyzXWHt6tXpypbNPZGYtQeDbzGCnzrOyRSezeVh2DLSV3tOWPRo6Hd+pgnnxX2ea3JZ6AJ5ZpG6c8oDBhnLS9LHThsslqKBs0yskZLmoo7e7LlRFBcWsWuYGMvVXKXZvn2+2GXLR7Esx2lsp5orCAXvtSbUDHUS3nJYOjTSrVKulhFF4vPt2tyNPxQ0w010wx5KIfSlToz+IcpV9idPaej7QRriVDY2uDXVDu26xt7qxwAU+OYi72tK5UPenop7jD+TSxafDjzssAeM6lc4MJKjnhNcxT41EjvtLhopidG44lcCTqFrmhMbNooLQb/fY7mRuAm+sfYTB+7icK+4+wZy8RjlNymWWzPFMpj1Mo18RD6odXYtLcL4JQrob7YhnPtmscSzjb4/9m5ekitehnNTnhjIQW7COlCAzylbmLEa8aAs8MVzjY/3R34g76bj1/xC/0/V/JVej43+rGyaIFssX/GrR4b2qX3p4NySbW9zG1UQ3mUMddyXELsWJpKlzPmfVY/Xgh6/aIP9EOHV01bztWXOqLxEXr7sIptCjQiL1y3Oq3jkcfnIvRy1HGQCHuoNwFNWj3xRl9fKbTEWKbO2pHOKLwSKrwEd0iQrHiLkSO5lmk1UkyZsvnxaMm8yR+4wgLbcIe03Ns6nrWcQm8AFhVb9DHwkjN/7foJy8SiFezNoVbI3/agbUAss+v3s400fFk3NH++yN3E29lwv8FbQJ5qI0O5GZNSeOmng/zZPf3S0xjueg0wPt9VUO+Mkdu6m/ER/j/I2qH6HxwnnN3+J7V0PLxoYpnHjW1CDybAH20sVJ7VLJPNt5Siy3jEyJoHvzH1Xsvf/Tpdu15o9ZO1YL6OsbMy7F8KKYgyOTjolMWe4pw1OXF/OYsnv66GN2tdj3V+v+vXrb/iENagH96929Bzd9fdk9497sFpfuk+nr+I1ElgDJlmaTu0dEOFxPeIysElLA9Ix+fQI9unffamP4C3p+9QtNNnqOEN+/iIBj7n9q4/CrjtTxx/h/qvgsEw87EtJC9gYutrdY4u1jD34fOUcw0z1ceex6mykfzXGfoUy9xwQ4ZlqRHDnb3url+Vfnf9eoJqR683C63Nt6//AakyTrubCgAA"
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
			handler = MockRepository.GeneratePartialMock<BusinessNotificationHandlerForTest>(ProviderType.CNS);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}

			foreach (var setting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
		}

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

		private BusinessNotificationHandlerForTest handler;
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
			{ "SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};
		Dictionary<string, string> MessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"Authorization", "HeaderAuthorization"}
		};
		public override string Provider => "CNS";
	}
}
