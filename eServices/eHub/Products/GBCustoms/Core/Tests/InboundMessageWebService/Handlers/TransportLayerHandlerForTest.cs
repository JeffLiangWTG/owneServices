using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using Common.Logging;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers
{
	public class TransportLayerHandlerForTest : TransportLayerHandler
	{
		public TransportLayerHandlerForTest(ProviderType provider) : base(provider)
		{
		}

		public void SetUpMock(HandlerTestBase.TestConfiguration config, Dictionary<string, string> messageHeaderConfiguration)
		{
			this.config = config;
			MockPartyAccessor();
			if (config.UseMockForConnection) MockConnection();
			if (config.UseMockForInboxAccessor) MockInboxAccessor();
			if (config.UseMockForSubscriptionAccessor) MockSubscriptionAccessor();
			if (config.UseMockForClientRegistrationAccessor) MockClientRegistrationAccessor();
			if (config.UseMockForClientRegistrationAccessorForCorrelation) MockClientRegistrationAccessorForCorrelation();
			if (config.UseMockForLogger) MockLogger(config.Logger);
			if (config.UseMockForMessageContentLogger) MockMessageContentLogger(config.MessageContentLogger);
			if (config.IncludeMessageHeaders) MockMessageHeaders(messageHeaderConfiguration);
			if (config.UseMockForClientAccessor) MockClientAccessor();
		}

		HttpRequestMessage GetRequest(bool includeHeaders = true)
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "");
			request.Content = new StringContent(config.RequestContent);
			request.Content.Headers.Remove("content-type");
			request.Content.Headers.Add("content-type", "application/vnd.api+json");
			if (includeHeaders)
			{
				request.Headers.Add("Notification-Box-Id", "50dca3fc-c37c-4f03-b719-63571333624c");
				request.Headers.Add("Notification-Message-Id", "665544332211");
			}
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

			if (config.ExpectedRecipients == null)
			{
				if (!string.IsNullOrEmpty(config.SubscriptionValueGmrId))
				{
					subscriptionAccessor.Expect(x => x.InsertSubscribedClients(config.Transaction, config.SubscriptionType, config.SenderID, config.ExpectedRecipientsGmrId, config.SubscriptionValueGmrId, "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e", "GMR-ID")).Repeat.Once();
				
					if (config.GmrIdSubscriptionExists)
					{
						subscriptionAccessor.Expect(x => x.SelectSubscribedClients(config.Transaction, config.SenderID, config.SubscriptionType, config.SubscriptionValueGmrId, config.ReferenceType)).Repeat.Once().Return(config.ExpectedRecipientsGmrId);
					}
					else
					{
						subscriptionAccessor.Expect(x => x.SelectSubscribedClients(config.Transaction, config.SenderID, config.SubscriptionType, config.SubscriptionValueGmrId, config.ReferenceType)).Repeat.Once().Return(new string[0]);
					}
				}
			}

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
				sqlReader.Expect(_ => _["CX_Qualifier"]).Repeat.Any().Return("");
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
			clientAccessor.Expect(accessor => accessor.IsPermitInboxRecipient(Arg<string>.Is.Anything, Arg<bool>.Is.Anything))
				.Repeat.Any().Return(config.ClientPermittedRecipient);
			PartyAccessorFactory = () => clientAccessor;
			config.PartyAccessor = clientAccessor;
		}

		void MockClientRegistrationAccessorForCorrelation()
		{
			var clientRegistrations = new List<Dictionary<string, object>>();
			clientRegistrations.AddRange(config.ExpectedClientRegistrationForCorrelation);

			var clientRegistrationAccessor = MockRepository.GenerateMock<IClientRegistrationAccessor>();
			clientRegistrationAccessor.Expect(accessor => accessor.ReadRegistrations(
				Arg<string>.Is.Equal(config.SenderID),
				Arg<string>.Is.Equal(config.ClientRegistrationType),
				Arg<bool>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<int?>.Is.Anything,
				Arg<int?>.Is.Anything
				)).Repeat.Any().Return(clientRegistrations);

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

			config.MessageContentLogger = mockLogger;
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

		void MockMessageHeaders(Dictionary<string, string> messageHeaderConfiguration)
		{
			foreach (var setting in messageHeaderConfiguration)
			{
				this.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
		}

		void MockPartyAccessor()
		{
			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();

			partyAccessor.Expect(accessor => accessor.IsPermitInboxRecipient(Arg<string>.Is.Anything, Arg<bool>.Is.Anything))
				.Repeat.Any().Return(config.ClientPermittedRecipient);

			PartyAccessorFactory = () => partyAccessor;
			config.PartyAccessor = partyAccessor;
		}

		public HttpResponseMessage ExecuteProcessTest()
		{
			return base.Process(GetRequest(), config.Logger, config.ServiceID, config.MessageDoc, config.MessageBodyDoc, true, config.RequestContent);
		}

		public HttpResponseMessage ExecuteProcessTestWithNoHeaders()
		{
			return base.Process(GetRequest(false), config.Logger, config.ServiceID, config.MessageDoc, config.MessageBodyDoc, true, config.RequestContent);
		}

		private HandlerTestBase.TestConfiguration config;
	}
}
