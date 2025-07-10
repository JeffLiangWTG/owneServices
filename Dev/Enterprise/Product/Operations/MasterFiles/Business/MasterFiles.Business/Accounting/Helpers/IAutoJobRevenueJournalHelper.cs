namespace Enterprise.MasterFiles.Business
{
	public interface IAutoJobRevenueJournalHelper
	{
		bool IsExcludedFromAutoJRJ(GlbBranch branch, OrgHeader org, string countryCode);
		bool IsAutoJRJEnabled { get; }
	}
}
