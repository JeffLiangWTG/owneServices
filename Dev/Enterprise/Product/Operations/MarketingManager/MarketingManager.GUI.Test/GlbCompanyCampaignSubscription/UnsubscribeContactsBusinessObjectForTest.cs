using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class UnsubscribeContactsBusinessObjectForTest : UnsubscribeContactsBusinessObject
	{
		public UnsubscribeContactsBusinessObjectForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UnsubscribeContactsBusinessObjectForTest(GlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		public override SubscriptionListNodeCollection SubscriptionsList
		{
			get
			{
				if (subscriptionsList == null)
				{
					subscriptionsList = new SubscriptionListNodeCollection(false);
					subscriptionsList.OnNewBusinessObjectAdded += new EventHandler(OnNewSubscriptionAdded);
					RegisterEditableChildObject(subscriptionsList);
				}
				return subscriptionsList;
			}
		}

		public void OnNewSubscriptionAdded_Exposed(object sender, EventArgs e)
		{
			base.OnNewSubscriptionAdded(sender, e);
		}
	}
}
