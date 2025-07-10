namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyCampaignSubscriptionForOrganisationCollection : IGlbCompanyCampaignSubscriptionCollection
	{
		new IGlbCompanyCampaignSubscription this[int index] { get; }
	}
}