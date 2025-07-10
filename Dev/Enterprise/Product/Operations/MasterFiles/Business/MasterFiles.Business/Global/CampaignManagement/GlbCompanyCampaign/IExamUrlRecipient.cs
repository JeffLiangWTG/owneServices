namespace Enterprise.MasterFiles.Business
{
	public interface IExamUrlRecipient : IGlbCompanyCampaignItemRecipient
	{
		string TablePrefix { get; }

		string Language { get; }

		bool HasNonPermittedDuplicateEmail { get; }
	}
}
