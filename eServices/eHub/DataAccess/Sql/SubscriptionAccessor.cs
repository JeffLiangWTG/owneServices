using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.XPath;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.DataAccess.Sql
{
	public class SubscriptionAccessor : ISubscriptionAccessor
	{
		public SubscriptionAccessor() : this(new eServices.eHubDataAccess.Sql.SubscriptionAccessor()) { }

		public SubscriptionAccessor(eServices.eHubDataAccess.Integration.ISubscriptionAccessor subscriptionAccessor)
		{
			this.subscriptionAccessor = subscriptionAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.ISubscriptionAccessor subscriptionAccessor;

		public bool IsAutoSubscriptionRequired(IBaseMessage message)
			=> subscriptionAccessor.IsAutoSubscriptionRequired(key => BizTalkContextAccessor(message, key));

		public void InsertSubscribedClients(SqlTransaction transaction, Guid subscriptionTypePK, string senderId, string[] recipientIds, string value, string reference, string referenceType)
			=> subscriptionAccessor.InsertSubscribedClients(transaction, subscriptionTypePK, senderId, recipientIds, value, reference, referenceType);

		public void InsertSubscribedClients(SqlTransaction transaction, string subscriptionTypeID, string senderId, string[] recipientIds, string value, string reference, string referenceType)
			=> subscriptionAccessor.InsertSubscribedClients(transaction, subscriptionTypeID, senderId, recipientIds, value, reference, referenceType);

		public void InsertAutoSubscriptionsForSender(IBaseMessage message)
			=> subscriptionAccessor.InsertAutoSubscriptionsForSender(key => BizTalkContextAccessor(message, key), message.BodyPart.Data);

		public string[] SelectSubscribedClients(IBaseMessage message)
			=> subscriptionAccessor.SelectSubscribedClients(key => BizTalkContextAccessor(message, key), message.BodyPart.Data);

		public XPathNodeIterator SelectSubscriptions(string subscriptionType, string senderId, string value)
			=> subscriptionAccessor.SelectSubscriptions(subscriptionType, senderId, value);

		public XPathNodeIterator SelectSubscriptionsByValue(string subscriptionType, string value, string referenceType = null)
			=> subscriptionAccessor.SelectSubscriptionsByValue(subscriptionType, value, referenceType);

		public SubscriptionInfo[] SelectSubscriptions(IBaseMessage message)
			=> subscriptionAccessor.SelectSubscriptions(key => BizTalkContextAccessor(message, key), message.BodyPart.Data)
				.Select(s => (SubscriptionInfo)s).ToArray();

		public string[] SelectSubscribedClients(SqlTransaction transaction, string senderId, string subscriptionType, string value)
			=> subscriptionAccessor.SelectSubscribedClients(transaction, senderId, subscriptionType, value);

		public string[] SelectSubscribedClients(SqlTransaction transaction, string senderId, string subscriptionType, string value, string referenceType)
			=> subscriptionAccessor.SelectSubscribedClients(transaction, senderId, subscriptionType, value, referenceType);

		public string[] SelectUSCustomsClientsForFilerCode(string filerCode, bool prod)
			=> subscriptionAccessor.SelectUSCustomsClientsForFilerCode(filerCode, prod);

		public SubscriptionInfo[] SelectSubscriptions(string subscriptionType, string[] providerIDs, string value = null, string reference = null, string referenceType = null)
			=> subscriptionAccessor.SelectSubscriptions(subscriptionType, providerIDs, value, reference, referenceType)
				.Select(s => (SubscriptionInfo)s).ToArray();

		private static object BizTalkContextAccessor(IBaseMessage message, string key)
		{
			var uri = new Uri(key);
			string name = uri.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);
			string ns = uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.Unescaped);
			return message.Context.Read(name, ns);
		}
	}
}
