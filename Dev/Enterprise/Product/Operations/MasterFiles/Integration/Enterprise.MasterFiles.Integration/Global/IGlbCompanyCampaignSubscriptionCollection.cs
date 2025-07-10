using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyCampaignSubscriptionCollection : IActiveBusinessObjectCollection, IEnumerable<IGlbCompanyCampaignSubscription>
	{
		new IGlbCompanyCampaignSubscription this[int index] { get; }
		new IGlbCompanyCampaignSubscription AddNew();
		void ValidateAllMembers();
	}
}