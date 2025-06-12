using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.DataAccess.Integration
{
	public struct SubscriptionInfo
	{
		public Guid ID;
		public string Type;
		public string Provider;
		public string Subscriber;
		public string Value;
		public string Reference;
		public string ReferenceType;
		public DateTime? Subscribed;
		public DateTime? Expiry;

		public static implicit operator SubscriptionInfo(eServices.eHubDataAccess.Integration.SubscriptionInfo subscriptionInfo)
			=> new SubscriptionInfo
			{
				ID = subscriptionInfo.ID,
				Type = subscriptionInfo.Type,
				Provider = subscriptionInfo.Provider,
				Subscriber = subscriptionInfo.Subscriber,
				Value = subscriptionInfo.Value,
				Reference = subscriptionInfo.Reference,
				ReferenceType = subscriptionInfo.ReferenceType,
				Subscribed = subscriptionInfo.Subscribed,
				Expiry = subscriptionInfo.Expiry
			};
	}
}
