using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Xml;

using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using Common.Logging;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers
{
	public class BusinessNotificationHandlerForTest : BusinessNotificationHandler
	{
		public BusinessNotificationHandlerForTest(ProviderType provider) : base(provider)
		{
		}

		public void SetUpMock(HandlerTestBase.TestConfiguration config)
		{
			this.config = config;
			MockPartyAccessor();
			if (config.UseMockForConnection) MockConnection();
			if (config.UseMockForInboxAccessor) MockInboxAccessor();
			if (config.UseMockForSubscriptionAccessor) MockSubscriptionAccessor();
			if (config.UseMockForClientRegistrationAccessor) MockClientRegistrationAccessor();
			if (config.UseMockForLogger) MockLogger(config.Logger);
			if (config.UseMockForMessageContentLogger) MockMessageContentLogger(config.MessageContentLogger);
			if (config.UseMockForClientAccessor) MockClientAccessor();
		}

		HttpRequestMessage GetRequest()
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "");
			request.Content = new StringContent(config.RequestContent);
			return request;
		}

		void MockInboxAccessor()
		{
			var inboxAccessor = MockRepository.GenerateMock<IInboxAccessor>();
			InboxAccessorFactory = () => inboxAccessor;
			config.InboxAccessor = inboxAccessor;
		}

		void MockSubscriptionAccessor()
		{
			SubscriptionInfo[] subscriptions = new SubscriptionInfo[]
			{
				new SubscriptionInfo()
				{
					Type = config.SubscriptionType,
					Value = "PreIrrelevant",
					Reference = "PreIrrelevant"
				},
				new SubscriptionInfo()
                {
					Type = config.SubscriptionType,
					Value = "ValueDoesNotMatter",
					Reference = "<eHubMessageTrackingId>eHubTrackingID</eHubMessageTrackingId>"
				},
				new SubscriptionInfo()
				{
					Type = config.SubscriptionType,
					Value = "PostIrrelevant",
					Reference = "PostIrrelevant"
				}
			};
			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			SubscriptionAccessorFactory = () => subscriptionAccessor;

			if (config.ClientExist)
			{
				subscriptionAccessor.Expect(x => x.SelectSubscribedClients(config.Transaction, config.SenderID, config.SubscriptionType, config.SubscriptionValue, config.ReferenceType)).Repeat.Once().Return(null);
			}
			else
			{
				subscriptionAccessor.Expect(x => x.SelectSubscribedClients(config.Transaction, config.SenderID, config.SubscriptionType, config.SubscriptionValue, config.ReferenceType)).Repeat.Once().Return(config.ExpectedRecipients);
			}
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(config.SubscriptionType, config.SubscriptionProviderIDList, "ConversationID")).Return(subscriptions);
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(config.SubscriptionType, config.SubscriptionProviderIDList, "Sani_ST2_7177105")).Return(subscriptions);
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(config.SubscriptionType, config.SubscriptionProviderIDList, config.SubscriptionValue)).Return(subscriptions);
			config.SubscriptionAccessor = subscriptionAccessor;
		}

		void MockClientRegistrationAccessor()
		{
			List<SqlDataReader> clientRegistrations = new List<SqlDataReader>();
			if (config.ExpectedClientRegistration == null)
			{
				var sqlReader = MockRepository.GenerateStrictMock<SqlDataReader>();
				sqlReader.Expect(_ => _.Read()).Repeat.Any();
				sqlReader.Expect(_ => _["CX_Code"]).Repeat.Any().Return(config.Header ?? string.Empty);
				sqlReader.Expect(_ => _["CX_Qualifier"]).Repeat.Any().Return(string.Empty);
				clientRegistrations.Add(sqlReader);
			}
			else
			{
				clientRegistrations.AddRange(config.ExpectedClientRegistration);
			}

			var clientRegistrationAccessor = MockRepository.GenerateMock<IClientRegistrationAccessor>();
			clientRegistrationAccessor.Expect(accessor => accessor.ReadRegistrations(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal(config.SenderID), Arg<string>.Is.Equal(config.ClientRegistrationType)))
				.Repeat.Once().Return(clientRegistrations);
			ClientRegistrationAccessorFactory = () => clientRegistrationAccessor;
			config.ClientRegistrationAccessor = clientRegistrationAccessor;
		}

		void MockClientAccessor()
		{
			List<SqlDataReader> clients = new List<SqlDataReader>();
			var sqlReader = MockRepository.GenerateStrictMock<SqlDataReader>();
			sqlReader.Expect(_ => _.Read()).Repeat.Any();

			var clientAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			clientAccessor.Expect(accessor => accessor.ClientExists(Arg<string>.Is.Anything))
				.Repeat.Once().Return(config.ClientExist);
			clientAccessor.Expect(accessor => accessor.IsPermitInboxRecipient(Arg<string>.Is.Anything, Arg<bool>.Is.Anything))
				.Repeat.Any().Return(config.ClientPermittedRecipient);
			PartyAccessorFactory = () => clientAccessor;
			config.PartyAccessor = clientAccessor;
		}

		void MockPartyAccessor()
		{
			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();

			partyAccessor.Expect(accessor => accessor.IsPermitInboxRecipient(Arg<string>.Is.Anything, Arg<bool>.Is.Anything))
				.Repeat.Any().Return(config.ClientPermittedRecipient);

			PartyAccessorFactory = () => partyAccessor;
			config.PartyAccessor = partyAccessor;
		}

		void MockLogger(ILog mockLogger = null)
		{
			if (mockLogger == null)
			{
				mockLogger = MockRepository.GenerateMock<ILog>();
				mockLogger.Stub(x => x.IsDebugEnabled).Return(true);
				mockLogger.Stub(x => x.IsErrorEnabled).Return(true);
				mockLogger.Stub(x => x.IsInfoEnabled).Return(true);
			}

			logger = mockLogger;
			config.Logger = logger;
		}

		void MockMessageContentLogger(ILog mockLogger = null)
		{
			if (mockLogger == null)
			{
				mockLogger = MockRepository.GenerateMock<ILog>();
				mockLogger.Stub(x => x.IsDebugEnabled).Return(true);
				mockLogger.Stub(x => x.IsErrorEnabled).Return(true);
				mockLogger.Stub(x => x.IsInfoEnabled).Return(true);
			}
			
			config.MessageContentLogger = mockLogger;
		}

		void MockConnection()
		{
			var connection = MockRepository.GenerateMock<SqlConnection>();
			GetConnection = key => connection;
			var transaction = MockRepository.GenerateMock<SqlTransaction>();
			if (config.ConnectionState == ConnectionState.Closed)
			{
				connection.Expect(x => x.Open());
				connection.Expect(x => x.BeginTransaction()).Return(transaction);
				transaction.Expect(x => x.Connection).Return(connection);
			}
			else if (config.ConnectionState == ConnectionState.Broken)
			{
				connection.Expect(x => x.Open()).Throw(new Exception("DataAccess exception")).Repeat.Once();
			}

			connection.Expect(conn => conn.State).Return(config.ConnectionState);
			transaction.Expect(x => x.Commit());
			transaction.Expect(x => x.Connection).Return(connection);
			config.Transaction = transaction;
			config.eHubTransactionsConnection = connection;
		}

		public HttpResponseMessage ExecuteProcessTest()
		{
			return base.Process(GetRequest(), config.Logger, config.ServiceID, config.MessageDoc, config.MessageBodyDoc);
		}

		private HandlerTestBase.TestConfiguration config;
	}
}
