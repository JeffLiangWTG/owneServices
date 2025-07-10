namespace Enterprise.MasterData.Common
{
	public interface IDirtyRecordFinder
	{
		bool IsOrgDirtyForDeduplication();
		string GetDirtyReason();
	}
}
