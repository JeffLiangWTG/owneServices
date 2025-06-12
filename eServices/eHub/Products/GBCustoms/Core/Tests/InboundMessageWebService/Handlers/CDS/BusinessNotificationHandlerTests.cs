using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Sql;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.CDS
{
	[TestFixture]
	public class BusinessNotificationHandlerTests : HandlerTestBase
	{
		const string Client = "GBCustomsTest-Direct";
		const string RegistrationType = "GBCustoms-Direct";

		[Test]
		public void TestReceiveResponseToDelcarations_CurrentOrOldAuthorisationHeader_Success()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				SubscriptionProviderIDList = new string[]{ "GBCustoms","GBCustomsTest"},
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new[] { "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToDeclaration.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForConnection = true,
				ClientPermittedRecipient = true
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("CURRENT_AUTHORISATION_HEADER").Repeat.Once();

			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(
					Arg<SqlTransaction>.Is.Equal(config.Transaction),
					Arg<string>.Is.Equal(Client),
					Arg<string>.Is.Equal(Client.Replace("-Direct", "")),
					Arg<Guid>.Is.Anything,
					Arg<eHubGatewayMessage>.Is.Anything))
				.WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					Assert.AreEqual("CDS", message.ApplicationCode);
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, message.SchemaType);
					Assert.AreEqual("Recipient", message.ClientID);
					Assert.AreEqual(GetMessage("ResponseToDeclaration - InboxAndOutbox.xml"), message.MessageStream.DecodeAndDecompress().ReadToEnd());
				});

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToDelcarations_NoConversationID_Success()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ARPXYZPRD0123456789",
				ClientRegistrationType = "GBCustoms-Direct",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedRecipients = new[] { "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToDeclaration.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForConnection = true,
				ClientPermittedRecipient = true
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("CURRENT_AUTHORISATION_HEADER").Repeat.Once();

			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(
					Arg<SqlTransaction>.Is.Equal(config.Transaction),
					Arg<string>.Is.Equal(Client),
					Arg<string>.Is.Equal(Client.Replace("-Direct", "")),
					Arg<Guid>.Is.Anything,
					Arg<eHubGatewayMessage>.Is.Anything))
				.WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					Assert.AreEqual("CDS", message.ApplicationCode);
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, message.SchemaType);
					Assert.AreEqual("Recipient", message.ClientID);
					Assert.AreEqual(GetMessage("ResponseToDeclaration - InboxAndOutbox.xml"), message.MessageStream.DecodeAndDecompress().ReadToEnd());
				});

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToDelcarations_NextAuthorisationHeader_Success()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new[] { "Recipient" },
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToDeclaration.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForConnection = true,
				ClientPermittedRecipient = true
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("NEXT_AUTHORISATION_HEADER").Repeat.Once();
			
			config.ClientRegistrationAccessor.Expect(x => x.UpdateRegistrationCode("CURRENT_AUTHORISATION_HEADER", config.Transaction, Client, RegistrationType, "OLD"));
			config.ClientRegistrationAccessor.Expect(x => x.UpdateRegistrationCode("NEXT_AUTHORISATION_HEADER", config.Transaction, Client, RegistrationType, "CURRENT"));
			config.ClientRegistrationAccessor.Expect(x => x.UpdateRegistrationCode(string.Empty, config.Transaction, Client, RegistrationType, "NEXT"));
			config.InboxAccessor
				.Expect(x => x.InsertToInboxAndOutbox(
					Arg<SqlTransaction>.Is.Equal(config.Transaction),
					Arg<string>.Is.Equal(Client),
					Arg<string>.Is.Equal(Client.Replace("-Direct", string.Empty)),
					Arg<Guid>.Is.Anything,
					Arg<eHubGatewayMessage>.Is.Anything))
				.WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					Assert.AreEqual("CDS", message.ApplicationCode);
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, message.SchemaType);
					Assert.AreEqual("Recipient", message.ClientID);
					Assert.AreEqual(GetMessage("ResponseToDeclaration - InboxAndOutbox.xml"), message.MessageStream.DecodeAndDecompress().ReadToEnd());
				}).Repeat.Once();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToDelcarations_InvalidAuthorisationHeader_Failure()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new[] { "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToDeclaration.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = true,
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("INVALID_AUTHORISATION_HEADER").Repeat.Once();

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());
			Assert.AreEqual(461, exception.GetHttpCode());
			Assert.AreEqual("Invalid AuthorisationHeader [INVALID_AUTHORISATION_HEADER] received from GBCustoms.", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToDelcarations_NoSubscriberFound_Failure()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new string[0],
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToDeclaration.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientAccessor = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForConnection = true,
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("CURRENT_AUTHORISATION_HEADER").Repeat.Once();
			handler.Expect(x => x.GetRecipientsWaitForRetry()).Repeat.Times(3);

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual(465, exception.GetHttpCode());
			Assert.AreEqual("No subscriber found for Conversation ID: [ConversationID]", exception.Message);

			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);

			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToDelcarations_InternalExceptions_Failure()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Broken,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new string[0],
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToDeclaration.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = true,
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("INVALID_AUTHORISATION_HEADER").Repeat.Once();

			var exception = Assert.Throws<Exception>(() => handler.ExecuteProcessTest());
			Assert.AreEqual("DataAccess exception", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToInventoryRequests_CurrentOrOldAuthorisationHeader_Success()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedRecipients = new []{ "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToInventoryRequest.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToInventoryRequest.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForConnection = true,
				ClientPermittedRecipient = true
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("CURRENT_AUTHORISATION_HEADER").Repeat.Once();
			
			config.InboxAccessor
				.Expect(x => x.InsertToInboxAndOutbox(
					Arg<SqlTransaction>.Is.Equal(config.Transaction),
					Arg<string>.Is.Equal(Client),
					Arg<string>.Is.Equal(Client.Replace("-Direct", string.Empty)),
					Arg<Guid>.Is.Anything,
					Arg<eHubGatewayMessage>.Is.Anything))
				.WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					Assert.AreEqual("CDS", message.ApplicationCode);
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, message.SchemaType);
					Assert.AreEqual("Recipient", message.ClientID);
					Assert.AreEqual(GetMessage("ResponseToInventoryRequest - InboxAndOutbox.xml"), message.MessageStream.DecodeAndDecompress().ReadToEnd());
				}).Repeat.Once();
			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToInventoryRequests_NextAuthorisationHeader_Success()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedRecipients = new[] { "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToInventoryRequest.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToInventoryRequest.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForConnection = true,
				ClientPermittedRecipient = true
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("NEXT_AUTHORISATION_HEADER").Repeat.Once();

			config.ClientRegistrationAccessor.Expect(x => x.UpdateRegistrationCode("CURRENT_AUTHORISATION_HEADER", config.Transaction, Client, RegistrationType, "OLD"));
			config.ClientRegistrationAccessor.Expect(x => x.UpdateRegistrationCode("NEXT_AUTHORISATION_HEADER", config.Transaction, Client, RegistrationType, "CURRENT"));
			config.ClientRegistrationAccessor.Expect(x => x.UpdateRegistrationCode(string.Empty, config.Transaction, Client, RegistrationType, "NEXT"));
			config.InboxAccessor
				.Expect(x => x.InsertToInboxAndOutbox(
					Arg<SqlTransaction>.Is.Equal(config.Transaction),
					Arg<string>.Is.Equal(Client),
					Arg<string>.Is.Equal(Client.Replace("-Direct", string.Empty)),
					Arg<Guid>.Is.Anything,
					Arg<eHubGatewayMessage>.Is.Anything))
				.WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					Assert.AreEqual("CDS", message.ApplicationCode);
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, message.SchemaType);
					Assert.AreEqual("Recipient", message.ClientID);
					Assert.AreEqual(GetMessage("ResponseToInventoryRequest - InboxAndOutbox.xml"), message.MessageStream.DecodeAndDecompress().ReadToEnd());
				}).Repeat.Once();
			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToInventoryRequests_InvalidAuthorisationHeader_Failure()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new[] { "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToInventoryRequest.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToInventoryRequest.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = false,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = true,
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("INVALID_AUTHORISATION_HEADER").Repeat.Once();

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());
			Assert.AreEqual(461, exception.GetHttpCode());
			Assert.AreEqual("Invalid AuthorisationHeader [INVALID_AUTHORISATION_HEADER] received from GBCustoms.", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToDeclaration_InvalidHeader_Failure()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new[] { "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToDeclaration.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForInboxAccessor = false,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = true,
			};
			
			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("").Repeat.Once();

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());
			Assert.AreEqual(462, exception.GetHttpCode());
			Assert.AreEqual("Authorisation credentials in HTTP header not supplied", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToInventoryRequest_InvalidHeader_Failure()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new[] { "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToInventoryRequest.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToInventoryRequest.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForInboxAccessor = false,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = true,
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("").Repeat.Once();

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());
			Assert.AreEqual(462, exception.GetHttpCode());
			Assert.AreEqual("Authorisation credentials in HTTP header not supplied", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToInventoryRequests_InternalExceptions_Failure()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Broken,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new[] { "Recipient" },
				ExpectedClientRegistration = CreateClientRegistrations(),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToInventoryRequest.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToInventoryRequest.xml")),
				MessageBodyDoc = null,
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForInboxAccessor = false,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = true,
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("INVALID_AUTHORISATION_HEADER").Repeat.Once();

			var exception = Assert.Throws<Exception>(() => handler.ExecuteProcessTest());
			Assert.AreEqual("DataAccess exception", exception.Message);
			config.VerifyAll();
		}

		[SetUp]
		public void SetupTestHandler()
		{
			handler = MockRepository.GeneratePartialMock<BusinessNotificationHandlerForTest>(ProviderType.CDS);
			foreach (var settting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(settting.Key)).Repeat.Any().Return(settting.Value);
			}

			foreach (var settting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(settting.Key)).Repeat.Any().Return(settting.Value);
			}
		}

		private BusinessNotificationHandlerForTest handler;
		Dictionary<string, string> AppSettingConfiguration = new Dictionary<string, string>
		{
			{"SqlDeadlockRetryCount", "1"},
			{"SqlDeadlockRetryWait", "1"},
			{"GBCustomsID", "GBCustoms-Direct"},
			{"GBCustomsTestID", "GBCustomsTest-Direct"},
			{"GBCustomsRegistrationType", "GBCustoms-Direct"},
			{"AuthorisationHeaderKey", "Authorization"},
			{"GBCustomsSubscriptionType", "GBCCID"},
			{"ConversationIDKey", "X-Conversation-ID"},
			{"GetRecipientsRetryAttempts", "3"},
			{"GetRecipientsRetryWaitTime", "1"},
			{"SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};

		Dictionary<string, string> MessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"X-Conversation-ID", "ConversationID"}
		};

		SqlDataReader[] CreateClientRegistrations()
		{
			var current = MockRepository.GenerateMock<SqlDataReader>();
			current.Expect(x => x["CX_Code"]).Return("CURRENT_AUTHORISATION_HEADER");
			current.Expect(x => x["CX_Qualifier"]).Return("CURRENT");
			var old = MockRepository.GenerateMock<SqlDataReader>();
			old.Expect(x => x["CX_Code"]).Return("OLD_AUTHORISATION_HEADER");
			old.Expect(x => x["CX_Qualifier"]).Return("OLD");
			var next = MockRepository.GenerateMock<SqlDataReader>();
			next.Expect(x => x["CX_Code"]).Return("NEXT_AUTHORISATION_HEADER");
			next.Expect(x => x["CX_Qualifier"]).Return("NEXT");
			return new[] { old, current, next };
		}

		public override string Provider => "CDS";
	}
}
