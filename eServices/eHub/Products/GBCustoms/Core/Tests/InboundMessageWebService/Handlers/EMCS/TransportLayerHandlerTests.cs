using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.EMCS
{
	[TestFixture]
	public class TransportLayerHandlerTests : HandlerTestBase
	{
		const string RegistrationType = "GBCustoms-Transport";

		[Test]
		[TestCase(new object[] { "EMCS_TransportLayerResponse.json", "EMCS_Expected_Output.xml", "F0DFFC88B9D5084ADD91B8C4B63EE50ED12AB6B2", "EMCS_SubscribedMessage.xml", "3f757fe2-6185-42f6-be24-cfca61e5f82a", "Consignor" })]
		public void TestTransportLayerHandlerHandleBusinessResponseForUnsolicited(object[] testParams)
		{
			string inputFile = testParams[0].ToString(), outputFile = testParams[1].ToString(), xHubSignature = testParams[2].ToString(), subscribedMessage = testParams[3].ToString(), subscriptionValue = testParams[4].ToString(), referenceType = testParams[5].ToString();
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-EMCS",
				SubscriptionType = "GBCTID",
				SubscriptionValue = subscriptionValue,
				ExpectedRecipients = new[] { "GBCustomsTest-EMCS" },
				ClientRegistrationType = RegistrationType,
				ExpectedClientRegistrationForCorrelation = CreateClientRegistrations(),
				RequestContent = GetMessage(inputFile),
				MessageDoc = null,
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessorForCorrelation = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				IncludeMessageHeaders = true,
				ClientPermittedRecipient = true,
				ReferenceType = referenceType,
			};

			handler.SetUpMock(config, new Dictionary<string, string> { { "x-hub-signature", xHubSignature } });

			var mockContext = GenerateMockContext(GetMessage(subscribedMessage));
			handler.ContextFactory = () => mockContext;

			config.InboxAccessor.Expect(x => x.InsertToInbox(Arg<string>.Is.Anything, Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[2] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				Assert.AreEqual($"GBCustomsTest", message.ClientID);
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustoms", message.SchemaName);
				Assert.AreEqual(GetMessage(outputFile), message.MessageStream.DecodeAndDecompress().ReadToEnd());
			});

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			config.VerifyAll();
		}

		[Test]
		public void TestTransportLayerHandlerHandleBusinessResponseForUnsolicited_WhenNotSubscribed_ShouldAcceptForEMCS()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-EMCS",
				SubscriptionType = "GBCTID",
				SubscriptionValue = "3f757fe2-6185-42f6-be24-cfca61e5f82a",
				ExpectedRecipients = new[] { "GBCustoms" },
				ClientRegistrationType = RegistrationType,
				ExpectedClientRegistrationForCorrelation = CreateClientRegistrations(),
				RequestContent = GetMessage("EMCS_TransportLayerResponse.json"),
				MessageDoc = null,
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessorForCorrelation = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				IncludeMessageHeaders = true,
				ReferenceType = "Consignor"
			};

			handler.SetUpMock(config, new Dictionary<string, string> { { "x-hub-signature", "F0DFFC88B9D5084ADD91B8C4B63EE50ED12AB6B2" } });

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
				EI_Content = "H4sIAAAAAAAEAHVSyW7bMBQ814D/gdC1YcxVi49uAtcoGvSQIj34wuXRZmGRgsQECYL8e0XLbt0iJQEdOMsbjN7ts4Eu+RjQDSTlD8NyPvsKw6B2gGyEAYWYkOo6UD1KEe3VE6DY+50PKoFFro8tevADJDB7tD5ErQ4jjszBQ0jIT/oO+tanzB8tejDgR5d2mjJcIRPD8Nj6sENp//v9ej7LdxN0fAwWnTN9BmWhzynvYvLOG5XD41V8xhuLMJLEGsWdwYZXBgtHONYVbXDJZUU55yUT5h/tyXrSl6WUQnDOGKXvzV9F+zIOf53PPhThwmVjiyUqTOWkFlpjKnWNRU0k1hYIdmAEcNE0ysniKmt1fJ4kSsmKUOKwGnEsaNngmjmNbQ5MmkrzmkySUzGfYkhjt/cvHRz1XXc4ZVj8HGL4i5sJr9uijU/QjpqN3RbLbcFdJSsHDJe0llgwV2INTGDjjCopSFcztS2utmeX770/6hZnn2EhrBxLogwbIvhoISusS1phpoxkshGEULs4/+HFekXJ6XAiGasv3U+h3ufk1fC7EPsj58fm4UtFBWOZdYkD/AdXvTkiTKxX5M/htWyq8siAPrynfZt6HJJKj0Ou8dvt3c3mbj09mx7y+t+Mn3vfHmtmZGxw3DYq72m5pGxJxXUp5MdsV8xnb3mZfgGv0bLIbQMAAA=="
			};

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(x => x.eHubClients).Return(CreateGBCustomsClient());
			mockContext.Stub(x => x.eHubInboxMessages).Return(new TestDbSet<eHubInboxMessage>());
			mockContext.Stub(x => x.eHubErrors).Return(new TestDbSet<eHubError>());
			handler.ContextFactory = () => mockContext;

			var response = handler.ExecuteProcessTest();

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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
			handler = MockRepository.GeneratePartialMock<TransportLayerHandlerForTest>(ProviderType.EMCS);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}
		}

		private TransportLayerHandlerForTest handler;

		Dictionary<string, string> AppSettingConfiguration = new Dictionary<string, string>
		{
			{"GBCustomsID", "GBCustoms"},
			{"GBCustomsTestID", "GBCustomsTest"},
			{"GBCustomsRegistrationType", "GBCustoms-Direct"},
			{"AuthorisationHeaderKey", "Authorization"},
			{"GBCustomsSubscriptionType", "GBCTID"},
			{"DatabaseAccessRetryTimeLimit", "1"},
			{"SchemaValidation", "False"},
			{"PushSecretTest", "L5HRYEDCF7GWLHUWPIF5HYIZ6GPSEBMF" },
			{"PushSecretUat", "YJ2CRF2USGNW3OC2E3B7FB265XG33MBP" },
			{"PushSecretProd", "TBC" },
			{"GetRecipientsRetryAttempts", "3"},
			{"GetRecipientsRetryWaitTime", "1"},
			{"SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};

		List<Dictionary<string, object>> CreateClientRegistrations()
		{
			return new List<Dictionary<string, object>>()
			{
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-EMCS" },
					{ "CX_Qualifier", "Notification-Movement" },
					{ "CX_Code", "JSONBody: message.movementId" },
					{ "CX_Flag1", "Notification"},
					{ "CX_Flag2", "Subscribed" },
					{ "CX_Attr1", "MovementID" }
				},
			};
		}

		private eHubTransactionsContext GenerateMockContext(string subscribedMessage)
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(mock => mock.eHubClients).Return(new TestDbSet<eHubClient>()
			{
				new eHubClient
				{
					CC_PK = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					CC_ID = "recipient"
				},
				new eHubClient
				{
					CC_PK = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					CC_ID = "GBCustomsTest-EMCS"
				}
			});
			mockContext.Stub(mock => mock.eHubMessageTypes).Return(new TestDbSet<eHubMessageType>()
			{
				new eHubMessageType
				{
					DT_PK = Guid.Parse("33333333-3333-3333-3333-333333333333"),
					DT_Code = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange"
				}
			});
			mockContext.Stub(mock => mock.eHubSubscriptionTypes).Return(new TestDbSet<eHubSubscriptionType>()
			{
				new eHubSubscriptionType
				{
					ST_PK = Guid.Parse("44444444-4444-4444-4444-444444444444"),
					ST_ID = "GBCTID"
				}
			});
			mockContext.Stub(mock => mock.eHubSubscriptionValues).Return(new TestDbSet<eHubSubscriptionValue>()
			{
				new eHubSubscriptionValue
				{
					SV_PK = Guid.Parse("55555555-5555-5555-5555-555555555555"),
					SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_CC_Sender = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
					SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444"),
					SV_Value = "3f757fe2-6185-42f6-be24-cfca61e5f82a",
					SV_Reference = subscribedMessage
				},
			});

			return mockContext;
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

		public override string Provider => "EMCS";
	}
}
