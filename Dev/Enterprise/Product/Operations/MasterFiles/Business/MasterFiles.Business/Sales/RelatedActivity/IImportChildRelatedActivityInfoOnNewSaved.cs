namespace Enterprise.MasterFiles.Business
{
	public interface IImportChildRelatedActivityInfoOnNewSaved
	{
		bool ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory);
	}
}
