using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;

using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.CTCGB
{
    [TestFixture]
    public class TransportLayerHandlerTests : HandlerTestBase
    {
        const string RegistrationType = "GBCustoms-Transport";

        [Test]
		[TestCase(new object[] { "CTCGB_TransportLayerResponse_WithBody_Arrival.json", "InboxAndOutbox_WithBody_Arrival.xml", "AEBFBDA3AD3D3E7DA8BAB15EE8EDFBDF537EE47F", "775", "Arrive" })]
		[TestCase(new object[] { "CTCGB_TransportLayerResponse_WithBody_Arrival.json", "InboxAndOutbox_WithBody_Arrival.xml", "6C29EFEE5710AA1C667FDD2D1B17A19452E7E1F9", "775", "Arrive" })]
		[TestCase(new object[] { "CTCGB_TransportLayerResponse_WithBody_Departure.json", "InboxAndOutbox_WithBody_Departure.xml", "D85556361B6F02B394BA5040142580D8DD7913F6", "123", "Depart" })]
        [TestCase(new object[] { "CTCGB_TransportLayerResponse_WithBody_Arrival_InvalidXML.json", "InboxAndOutbox_WithBody_Arrival_InvalidXML.xml", "0953CFA4B55B6724261D1B1EF603B0901ACC8447", "340052", "Arrive" })]
		[TestCase(new object[] { "CTCGB_TransportLayerResponse_WithBody_Departure_ComAccRefMES21.json", "InboxAndOutbox_WithBody_Departure_ComAccRefMES21.xml", "C1A7340A0915BCB4FB242B6F3165F5A3E7DB18C5", "D96D6066CF854A54B8E6A62DD11041A9", "Depart" })]
		public void TestTransportLayerHandlerHandleBusinessResponseForWithBody(object[] testParams)
        {
            string inputFile = testParams[0].ToString(), outputFile = testParams[1].ToString(), xHubSignature = testParams[2].ToString(), subscriptionValue = testParams[3].ToString(), referenceType = testParams[4].ToString();

            var config = new TestConfiguration
            {
                ConnectionState = ConnectionState.Closed,
                SenderID = "GBCustomsTest-CTCGB",
                SubscriptionType = "GBCTID",
                SubscriptionValue = subscriptionValue,
                ExpectedRecipients = new[] { "recipient" },
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
                ReferenceType = referenceType,
				ClientPermittedRecipient = true,
			};

            handler.SetUpMock(config, new Dictionary<string, string>{ {"x-hub-signature", xHubSignature } });

			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustomsTest-CTCGB"), Arg<string>.Is.Equal("GBCustomsTest"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
			var message = x.Arguments[4] as eHubGatewayMessage;
			var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
			Assert.AreEqual($"recipient", message.ClientID);
			Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
			Assert.AreEqual(GetMessage(outputFile), message.MessageStream.DecodeAndDecompress().ReadToEnd());
            });

            var response = handler.ExecuteProcessTest();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            config.VerifyAll();
        }

        [Test]
        [TestCase(new object[] { "CTCGB_TransportLayerResponse_NoBody_Arrival.json", "GBCustoms_NoBody_Arrival.xml", "FEF109871363F09924D469673551E84CEB83349B", "CTCGB_SubscribedMessage_Arrival.xml", "775", "Arrive" })]
        [TestCase(new object[] { "CTCGB_TransportLayerResponse_NoBody_Departure.json", "GBCustoms_NoBody_Departure.xml", "343F20B9888E128F4249C96DB2AADA7F028FA194", "CTCGB_SubscribedMessage_Departure.xml", "123", "Depart" })]
        public void TestTransportLayerHandlerHandleBusinessResponseForNoBody(object[] testParams)
        {
            string inputFile = testParams[0].ToString(), outputFile = testParams[1].ToString(), xHubSignature = testParams[2].ToString(), subscribedMessage = testParams[3].ToString(), subscriptionValue = testParams[4].ToString(), referenceType = testParams[5].ToString();
            var config = new TestConfiguration
            {
                ConnectionState = ConnectionState.Closed,
                SenderID = "GBCustomsTest-CTCGB",
                SubscriptionType = "GBCTID",
                SubscriptionValue = subscriptionValue,
                ExpectedRecipients = new[] { "GBCustomsTest-CTCGB" },
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
                ReferenceType = referenceType,
				ClientPermittedRecipient = true,
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
        [TestCase("CTCGB_TransportLayerResponse_WithBody_Arrival.json", "6C29EFEE5710AA1C667FDD2D1B17A19452E7E1F9")]
        public void TestTransportLayerHandlerValidateIdAndGetRecipients_WhenNoBoxIdIsFound_ShouldThrowHttpException(string inputFile, string xHubSignature)
        {
            var config = new TestConfiguration
            {
                ConnectionState = ConnectionState.Closed,
                SenderID = "GBCustomsTest-CTCGB",
                SubscriptionType = "GBCTID",
                SubscriptionValue = "50dca3fc-c37c-4f03-b719-63571333624c",
                ExpectedRecipients = null,
                ClientRegistrationType = RegistrationType,
                ExpectedClientRegistrationForCorrelation = CreateInvalidClientRegistrations(),
                RequestContent = GetMessage(inputFile),
                MessageDoc = null,
                MessageBodyDoc = null,
                ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                UseMockForConnection = true,
                UseMockForClientRegistrationAccessor = false,
                UseMockForClientRegistrationAccessorForCorrelation = true,
                UseMockForSubscriptionAccessor = false,
                UseMockForLogger = true,
                IncludeMessageHeaders = true,
                ReferenceType = "Arrive"
            };

            handler.SetUpMock(config, new Dictionary<string, string> { { "x-hub-signature", xHubSignature } });
            var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

            Assert.AreEqual((int)HttpStatusCode.NotImplemented, exception.GetHttpCode());
            Assert.AreEqual("Provider: CTCGB. No Message ID found from the received notification.", exception.Message);
            config.VerifyAll();
        }

        [Test]
        [TestCase("CTCGB_TransportLayerResponse_WithBody_Arrival.json", "6C29EFEE5710AA1C667FDD2D1B17A19452E7E1F9")]
        public void TestTransportLayerHandlerValidateIdAndGetRecipients_WhenNoSubscriberIsFound_ShouldThrowHttpException(string inputFile, string xHubSignature)
        {
            var config = new TestConfiguration
            {
                ConnectionState = ConnectionState.Closed,
                SenderID = "GBCustomsTest-CTCGB",
                SubscriptionType = "GBCTID",
                SubscriptionValue = "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e",
                ExpectedRecipients = null,
                ClientRegistrationType = RegistrationType,
                ExpectedClientRegistrationForCorrelation = CreateClientRegistrations(),
                RequestContent = GetMessage(inputFile),
                MessageDoc = null,
                MessageBodyDoc = null,
                ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                UseMockForConnection = true,
                UseMockForClientRegistrationAccessor = false,
                UseMockForClientRegistrationAccessorForCorrelation = true,
                UseMockForSubscriptionAccessor = true,
                UseMockForInboxAccessor = true,
                UseMockForLogger = true,
                IncludeMessageHeaders = true,
                ReferenceType = "Arrive"
            };

            handler.SetUpMock(config, new Dictionary<string, string> { { "x-hub-signature", xHubSignature } });
            handler.Expect(x => x.GetRecipientsWaitForRetry()).Repeat.Times(3);
            var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

            Assert.AreEqual(465, exception.GetHttpCode());
            Assert.AreEqual("No subscriber found for Message ID: [775]", exception.Message);

            Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
            Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);
        }

        [Test]
        [TestCase("CTCGB_TransportLayerResponse_WithBody_Arrival.json", "Invalid_signature")]
        public void TestTransportLayerHandlerAuthorisation_ShouldThrowHttpException(string inputFile, string xHubSignature)
        {
            var config = new TestConfiguration
            {
                ConnectionState = ConnectionState.Closed,
                SenderID = "GBCustomsTest-CTCGB",
                SubscriptionType = "GBCTID",
                SubscriptionValue = "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e",
                ExpectedRecipients = null,
                RequestContent = GetMessage(inputFile),
                MessageDoc = null,
                MessageBodyDoc = null,
                ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                UseMockForConnection = true,
                UseMockForClientRegistrationAccessor = false,
                UseMockForSubscriptionAccessor = true,
                UseMockForInboxAccessor = true,
                UseMockForLogger = true,
                IncludeMessageHeaders = true,
                ReferenceType = "Arrive"
            };

            handler.SetUpMock(config, new Dictionary<string, string> { { "x-hub-signature", xHubSignature } });
            var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

            Assert.AreEqual(464, exception.GetHttpCode());
            Assert.IsTrue(exception.Message.StartsWith("Could not verify the push request for the test, uat or production secret."));
        }

        [SetUp]
        public void SetupTestHandler()
        {
            handler = MockRepository.GeneratePartialMock<TransportLayerHandlerForTest>(ProviderType.CTCGB);
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
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-ArriveSubscribed" },
                    { "CX_Code", "JSONBody: message.arrivalId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "SubscribedArrival" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-DepartSubscribed" },
                    { "CX_Code", "JSONBody: message.departureId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "SubscribedDeparture" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-MessageUri" },
                    { "CX_Code", "JSONBody: message.messageUri" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "MessageUri" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-DepartureId" },
                    { "CX_Code", "JSONBody: message.departureId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "ResponseDeparture" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-Body" },
                    { "CX_Code", "JSONBody: message.body" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "ResponseBody" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-ArrivalId" },
                    { "CX_Code", "JSONBody: message.arrivalId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "ResponseArrival" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-RequestId" },
                    { "CX_Code", "JSONBody: message.requestId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "RequestId" }
                },
            };
        }

        List<Dictionary<string, object>> CreateInvalidClientRegistrations()
        {
            return new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-MessageUri" },
                    { "CX_Code", "JSONBody: message.messageUri" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "MessageUri" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-DepartureId" },
                    { "CX_Code", "JSONBody: message.departureId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "ResponseDeparture" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-Body" },
                    { "CX_Code", "JSONBody: message.body" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "ResponseBody" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-ArrivalId" },
                    { "CX_Code", "JSONBody: message.arrivalId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "ResponseArrival" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" },
                    { "CX_Qualifier", "Notification-RequestId" },
                    { "CX_Code", "JSONBody: message.requestId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "RequestId" }
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
                    CC_ID = "GBCustomsTest-CTCGB"
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
                    SV_Value = "775",
                    SV_Reference = subscribedMessage
                },
                new eHubSubscriptionValue
                {
                    SV_PK = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    SV_CC_Sender = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
                    SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    SV_Value = "123",
                    SV_Reference = subscribedMessage
                },
            });

            return mockContext;
        }

        public override string Provider => "CTCGB";
    }
}
