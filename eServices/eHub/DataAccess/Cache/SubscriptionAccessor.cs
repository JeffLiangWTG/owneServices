using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Message.Interop;
using System.Data.SqlClient;
using System.Xml.XPath;

namespace CargoWise.eHub.DataAccess.Cache
{
	[Serializable]
	class SubscriptionAccessor : ISubscriptionAccessor
	{
		private const string SUBSCRIPTION_ACCESSOR_CACHE_KEY = "CargoWise.eHub.DataAccess.Cache.SubscriptionAccessor, Version=3.0.0.0";

		#region ISubscriptionAccessor Members

		public bool IsAutoSubscriptionRequired(IBaseMessage message)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			return subsAccessor.IsAutoSubscriptionRequired(message);
		}

		public void InsertSubscribedClients(SqlTransaction transaction, Guid subscriptionTypePK, string senderId, string[] recipientIds, string value, string reference, string referenceType)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			subsAccessor.InsertSubscribedClients(transaction, subscriptionTypePK, senderId, recipientIds, value, reference, referenceType);
		}

		public void InsertSubscribedClients(SqlTransaction transaction, string subscriptionTypeID, string senderId, string[] recipientIds, string value, string reference, string referenceType)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			subsAccessor.InsertSubscribedClients(transaction, subscriptionTypeID, senderId, recipientIds, value, reference, referenceType);
		}

		public void InsertAutoSubscriptionsForSender(IBaseMessage message)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			subsAccessor.InsertAutoSubscriptionsForSender(message);
		}

		public string[] SelectSubscribedClients(IBaseMessage message)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			return subsAccessor.SelectSubscribedClients(message);
		}

		public SubscriptionInfo[] SelectSubscriptions(IBaseMessage message)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			return subsAccessor.SelectSubscriptions(message);
		}

		public SubscriptionInfo[] SelectSubscriptions(string subscriptionType, string[] providerIDs, string value = null, string reference = null, string referenceType = null)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			return subsAccessor.SelectSubscriptions(subscriptionType, providerIDs, value, reference, referenceType);
		}

		public string[] SelectUSCustomsClientsForFilerCode(string filerCode, bool prod)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			return subsAccessor.SelectUSCustomsClientsForFilerCode(filerCode, prod);
		}

		public XPathNodeIterator SelectSubscriptions(string subscriptionType, string senderId, string value)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			return subsAccessor.SelectSubscriptions(subscriptionType, senderId, value);
		}

		public XPathNodeIterator SelectSubscriptionsByValue(string subscriptionType, string value, string referenceType = null)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			return subsAccessor.SelectSubscriptionsByValue(subscriptionType, value, referenceType);
		}

		public string[] SelectSubscribedClients(SqlTransaction transaction, string senderId, string subscriptionType, string value)
		{
			ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
			return subsAccessor.SelectSubscribedClients(transaction, senderId, subscriptionType, value);
		}

		public string[] SelectSubscribedClients(SqlTransaction transaction, string senderId, string subscriptionType, string value, string referenceType)
        {
            ISubscriptionAccessor subsAccessor = DataAccessFactories.NewSubscriptionAccessorInstance(SUBSCRIPTION_ACCESSOR_CACHE_KEY);
            return subsAccessor.SelectSubscribedClients(transaction, senderId, subscriptionType, value, referenceType);
        }

		#endregion
    }
}
