using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.BizTalk.Message.Interop;
using System.Data.SqlClient;
using System.Xml.XPath;

namespace CargoWise.eHub.DataAccess.Integration
{
	public interface ISubscriptionAccessor
	{
		bool IsAutoSubscriptionRequired(IBaseMessage message);
		void InsertSubscribedClients(SqlTransaction transaction, Guid subscriptionType, string senderId, string[] recipientIds, string value, string reference, string referenceType);
		void InsertSubscribedClients(SqlTransaction transaction, string subscriptionTypeID, string senderId, string[] recipientIds, string value, string reference, string referenceType);
		void InsertAutoSubscriptionsForSender(IBaseMessage message);
		string[] SelectSubscribedClients(IBaseMessage message);
		SubscriptionInfo[] SelectSubscriptions(IBaseMessage message);
		SubscriptionInfo[] SelectSubscriptions(string subscriptionType, string[] providerIDs, string value = null, string reference = null, string referenceType = null);
		string[] SelectSubscribedClients(SqlTransaction transaction, string senderId, string subscriptionType, string value);
		string[] SelectSubscribedClients(SqlTransaction transaction, string senderId, string subscriptionType, string value, string referenceType);
		string[] SelectUSCustomsClientsForFilerCode(string filerCode, bool prod);
		XPathNodeIterator SelectSubscriptions(string subscriptionType, string senderId, string value);
		XPathNodeIterator SelectSubscriptionsByValue(string subscriptionType, string value, string referenceType = null);
	}
}
