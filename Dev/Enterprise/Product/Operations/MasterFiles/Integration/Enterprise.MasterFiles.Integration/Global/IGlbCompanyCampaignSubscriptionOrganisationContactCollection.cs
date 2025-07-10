namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyCampaignSubscriptionForOrganisationContactCollection : IGlbCompanyCampaignSubscriptionCollection
	{
		new IGlbCompanyCampaignSubscription this[int index] { get; }
		void RejectAllChanges();
	}
}