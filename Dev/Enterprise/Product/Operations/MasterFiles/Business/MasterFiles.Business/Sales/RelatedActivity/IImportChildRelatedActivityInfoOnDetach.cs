namespace Enterprise.MasterFiles.Business
{
	public interface IImportChildRelatedActivityInfoOnDetach
	{
		bool ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory);
	}
}
