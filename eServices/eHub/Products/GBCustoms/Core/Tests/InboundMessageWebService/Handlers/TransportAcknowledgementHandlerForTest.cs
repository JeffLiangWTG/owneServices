using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers
{
	public class TransportAcknowledgementHandlerForTest : TransportAcknowledgementHandler
	{
		public TransportAcknowledgementHandlerForTest(ProviderType provider) : base(provider)
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
			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			SubscriptionAccessorFactory = () => subscriptionAccessor;
			subscriptionAccessor.Expect(x => x.SelectSubscribedClients(config.Transaction, config.SenderID, config.SubscriptionType, config.SubscriptionValue, config.ReferenceType)).Repeat.Once().Return(config.ExpectedRecipients);
			config.SubscriptionAccessor = subscriptionAccessor;
		}

		void MockClientRegistrationAccessor()
		{
			var sqlReader = MockRepository.GenerateStrictMock<SqlDataReader>();
			sqlReader.Expect(_ => _.Read()).Repeat.Any();
			sqlReader.Expect(_ => _["CX_Code"]).Repeat.Once().Return(config.Header);
			var clientRegistrationAccessor = MockRepository.GenerateMock<IClientRegistrationAccessor>();
			clientRegistrationAccessor.Expect(accessor => accessor.ReadRegistrations(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal(config.SenderID), Arg<string>.Is.Equal(config.ClientRegistrationType)))
				.Repeat.Once().Return(new List<SqlDataReader> { sqlReader });
			ClientRegistrationAccessorFactory = () => clientRegistrationAccessor;
			config.ClientRegistrationAccessor = clientRegistrationAccessor;
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
			config.MessageContentLogger = logger;
		}

		void MockConnection()
		{
			var connection = MockRepository.GenerateMock<SqlConnection>();
			GetConnection = key => connection;
			var transaction = MockRepository.GenerateMock<SqlTransaction>();
			if (config.ConnectionState == ConnectionState.Closed)
			{
				connection.Expect(x => x.BeginTransaction()).Return(transaction);
				transaction.Expect(x => x.Connection).Return(connection);
			}

			transaction.Expect(x => x.Commit());
			transaction.Expect(x => x.Connection).Return(connection);
			connection.Expect(conn => conn.State).Return(config.ConnectionState);
			connection.Expect(x => x.Open());
			config.Transaction = transaction;
			config.eHubTransactionsConnection = connection;
		}

		void MockPartyAccessor()
		{
			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();

			partyAccessor.Expect(accessor => accessor.IsPermitInboxRecipient(Arg<string>.Is.Anything, Arg<bool>.Is.Anything))
				.Repeat.Any().Return(config.ClientPermittedRecipient);

			PartyAccessorFactory = () => partyAccessor;
			config.PartyAccessor = partyAccessor;
		}

		protected internal override string GetSubscription(SqlTransaction transaction, string recipient, string value)
		{
			Assert.AreEqual(config.ExpectedRecipients[0], recipient);
			Assert.AreEqual(config.SubscriptionValue, value);
			return config.SubscribedMessage;
		}

		public HttpResponseMessage ExecuteProcessTest()
		{
			return base.Process(GetRequest(), config.Logger, config.ServiceID, config.MessageDoc, config.MessageBodyDoc);
		}

		private HandlerTestBase.TestConfiguration config;
	}
}
