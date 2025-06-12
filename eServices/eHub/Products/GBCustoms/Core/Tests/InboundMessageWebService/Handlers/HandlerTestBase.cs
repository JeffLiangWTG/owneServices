using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Integration;
using Common.Logging;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers
{
	public class HandlerTestBase
	{
		public class TestConfiguration
		{
			public bool UseMockForInboxAccessor { get; set; }
			public bool UseMockForSubscriptionAccessor { get; set; }
			public bool UseMockForClientAccessor { get; set; }
			public bool UseMockForClientRegistrationAccessor { get; set; }
			public bool UseMockForClientRegistrationAccessorForCorrelation { get; set; }
			public bool UseMockForLogger { get; set; }
			public bool UseMockForMessageContentLogger { get; set; }
			public bool UseMockForConnection { get; set; }
			public bool SchemaValidation { get; set; }
			public bool IncludeMessageHeaders { get; set; }
			public bool GmrIdSubscriptionExists { get; set; }

			public IInboxAccessor InboxAccessor { get; set; }
			public ISubscriptionAccessor SubscriptionAccessor { get; set; }
			public IClientRegistrationAccessor ClientRegistrationAccessor { get; set; }
			public IPartyAccessor PartyAccessor { get; set; }
			public SqlConnection eHubTransactionsConnection { get; set; }
			public SqlTransaction Transaction { get; set; }
			public ConnectionState ConnectionState { get; set; }
			public ILog Logger { get; set; }
			public ILog MessageContentLogger { get; set; }
			public string Header { get; set; }
			public string RequestContent { get; set; }
			public string SenderID { get; set; }
			public string ClientRegistrationType { get; set; }
			public string SubscriptionType { get; set; }
			public string SubscriptionValue { get; set; }
			public string SubscriptionValueGmrId { get; set; }
			public string SubscriptionTypeCSPID { get; set; }
			public string SubscriptionValueCSPID { get; set; }
			public string SubscriptionTypeConversationID { get; set; }
			public string SubscriptionValueConversationID { get; set; }
			public string[] SubscriptionProviderIDList { get; set; }
			public string[] ExpectedRecipients { get; set; }
			public string[] ExpectedRecipientsGmrId { get; set; }
			public SqlDataReader[] ExpectedClientRegistration { get; set; }
			public IEnumerable<Dictionary<string, object>> ExpectedClientRegistrationForCorrelation { get; set; }
			public XDocument MessageDoc { get; set; }
			public XDocument MessageBodyDoc { get; set; }
			public Guid ServiceID { get; set; }
			public string SubscribedMessage { get; set; }
			public string PushSecretTest { get; set; }
			public string PushSecretProd { get; set; }
			public string ReferenceType { get; set; }
			public bool ClientExist { get; set; }
			public bool ClientPermittedRecipient { get; set; }

			public void VerifyAll()
			{
				if (UseMockForConnection) eHubTransactionsConnection.VerifyAllExpectations();
				if (UseMockForInboxAccessor) InboxAccessor.VerifyAllExpectations();
				if (UseMockForSubscriptionAccessor) SubscriptionAccessor.VerifyAllExpectations();
				if (UseMockForClientRegistrationAccessor || UseMockForClientRegistrationAccessorForCorrelation) ClientRegistrationAccessor.VerifyAllExpectations();
				if (UseMockForClientAccessor) PartyAccessor.VerifyAllExpectations();
				if (UseMockForLogger) Logger.VerifyAllExpectations();
				if (UseMockForMessageContentLogger) MessageContentLogger.VerifyAllExpectations();
			}
		}

		protected string GetMessage(string source)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.TestFiles.{Provider}.{source}").ReadToEnd();
		}

		public virtual string Provider { get; }
	}
}
