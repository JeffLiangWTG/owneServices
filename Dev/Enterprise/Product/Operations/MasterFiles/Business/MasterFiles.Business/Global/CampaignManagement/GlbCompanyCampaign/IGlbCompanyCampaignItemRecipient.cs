namespace Enterprise.MasterFiles.Business
{
	public interface IGlbCompanyCampaignItemRecipient : IContactable
	{
		OrgHeader Organisation { get; }

		string Salutation { get; }
		string Title { get; }

		string Phone { get; }
		string Fax { get; }

		string RelatedDocName { get; }
	}
}
