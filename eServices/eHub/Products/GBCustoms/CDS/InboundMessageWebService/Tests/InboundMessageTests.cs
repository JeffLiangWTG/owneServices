using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.Controllers;
using CargoWise.eHub.Products.GBCustoms.Core;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.Tests
{
    [TestFixture]
	public class InboundMessageTests
	{
		const string Client = "GBCustomsTest-Direct";
		const string RegistrationType = "GBCustoms-Direct";

		[Test]
		public void TestReceiveResponseToDelcarations_InvalidXML_Failure()
		{
			var config = new HandlerTestBase.TestConfiguration
			{
				ConnectionState = ConnectionState.Open,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration - Invalid XML.xml"),
				Header = "CURRENT_AUTHORISATION_HEADER",
				UseMockForLogger = true,
				UseMockForMessageContentLogger = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForInboxAccessor = false,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = false,
			};

			var inboundService = new InboundMessageForTesting(config);
			inboundService.EhubTransactionsContext = () => MockeHubTransactionsContext();

			var response = inboundService.ReceiveResponseToDeclarations();
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.IsTrue(response.Content.ReadAsStringAsync().Result.Contains("Message is not valid XML"));
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToDelcarations_InvalidMessage_Failure()
		{
			var config = new HandlerTestBase.TestConfiguration
			{
				ConnectionState = ConnectionState.Open,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration - Invalid Message.xml"),
				Header = "CURRENT_AUTHORISATION_HEADER",
				UseMockForLogger = true,
				UseMockForMessageContentLogger = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForInboxAccessor = false,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = false,
				SchemaValidation = true
			};

			var inboundService = new InboundMessageForTesting(config);
			inboundService.EhubTransactionsContext = () => MockeHubTransactionsContext();

			var response = inboundService.ReceiveResponseToDeclarations();
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.IsTrue(response.Content.ReadAsStringAsync().Result.Contains("Invalid Message"));
			config.VerifyAll();
		}

		[Test]
		public void TestReceiveResponseToDelcarations_FailureLog()
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();
			mockLogger.Stub(x => x.IsDebugEnabled).Return(true);
			mockLogger.Stub(x => x.IsErrorEnabled).Return(true);
			mockLogger.Stub(x => x.IsInfoEnabled).Return(true);
			mockLogger.Expect(_ => _.Error(Arg<string>.Matches(x => x.Contains("ResponseToDeclaration threw exception.")), Arg<Exception>.Is.Anything));

			var mockMessageContentLogger = MockRepository.GenerateMock<ILog>();
			mockMessageContentLogger.Stub(x => x.IsDebugEnabled).Return(true);
			mockMessageContentLogger.Stub(x => x.IsErrorEnabled).Return(true);
			mockMessageContentLogger.Stub(x => x.IsInfoEnabled).Return(true);
			mockMessageContentLogger.Expect(_ => _.Debug(Arg<string>.Matches(x => x.Contains("ResponseToDeclaration Content:"))));
			mockMessageContentLogger.Expect(_ => _.Error("ResponseToDeclarationException"));

			var config = new HandlerTestBase.TestConfiguration
			{
				ConnectionState = ConnectionState.Open,
				SenderID = Client,
				SubscriptionType = "GBCCID",
				SubscriptionValue = "ConversationID",
				ClientRegistrationType = "GBCustoms-Direct",
				ExpectedRecipients = new[] { "Recipient" },
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				RequestContent = GetMessage("ResponseToDeclaration.xml"),
				MessageDoc = XDocument.Parse(GetMessage("ResponseToDeclaration.xml")),
				UseMockForLogger = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForConnection = true,
				Logger = mockLogger,
				MessageContentLogger = mockMessageContentLogger
			};

			var handler = MockRepository.GeneratePartialMock<BusinessNotificationHandlerForTest>(ProviderType.CDS);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("INVALID_AUTHORISATION_HEADER").Repeat.Once();
			handler.Expect(x => x.ExtractHeader("X-Conversation-ID")).Return("ConversationID").Repeat.Once();
			var inboundService = new InboundMessageForTesting(config, handler);
			inboundService.EhubTransactionsContext = () => MockeHubTransactionsContext();
			var response = inboundService.ReceiveResponseToDeclarations();
			Assert.AreEqual(461, (int)response.StatusCode);
			Assert.IsTrue(response.Content.ReadAsStringAsync().Result.Contains("Invalid AuthorisationHeader [INVALID_AUTHORISATION_HEADER] received from GBCustoms."));
			config.VerifyAll();
		}

		string GetMessage(string source)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.Tests.TestFiles.{source}").ReadToEnd();
		}

		eHubTransactionsContext MockeHubTransactionsContext()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var ehubClient = new TestDbSet<eHubClient>();
			ehubClient.Add(new eHubClient()
			{
				CC_PK = new Guid("00000000-0000-0000-0000-000000000000"),
				CC_ID = "GBCustoms"
			});
			mockContext.Stub(x => x.eHubClients).Return(ehubClient);
			mockContext.Stub(x => x.eHubInboxMessages).Return(new TestDbSet<eHubInboxMessage>());
			mockContext.Stub(x => x.eHubErrors).Return(new TestDbSet<eHubError>());
			return mockContext;
		}
	}

	#region Implementation

	public class InboundMessageForTesting : InboundMessageController
	{
		const string ConversationID = "ConversationID";
		const string GBCustomsID = "GBCustoms-Direct";
		const string GBCustomsTestID = "GBCustomsTest-Direct";
		const string RegistrationType = "GBCustoms-Direct";
		const string SubscriptionType = "GBCCID";
		string header = string.Empty;
		private HandlerTestBase.TestConfiguration config;
		BusinessNotificationHandlerForTest handler;

		public InboundMessageForTesting(HandlerTestBase.TestConfiguration config, BusinessNotificationHandlerForTest handler = null) : base()
		{
			this.config = config;
			header = this.config.Header;
			SchemaValidation = this.config.SchemaValidation;
			this.handler = handler ?? MockRepository.GeneratePartialMock<BusinessNotificationHandlerForTest>(ProviderType.CDS);
			this.handler.SetUpMock(this.config);
			logger = this.config.Logger;
			messageContentLogger = this.config.MessageContentLogger;
		}

		protected internal override HttpRequestMessage GetRequest()
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "");
			request.SetConfiguration(new System.Web.Http.HttpConfiguration());
			request.Content = new StringContent(config.RequestContent);
			return request;
		}

		protected internal override IMessageHandler GetHandler()
		{
			return this.handler;
		}
    }
	#endregion
}
