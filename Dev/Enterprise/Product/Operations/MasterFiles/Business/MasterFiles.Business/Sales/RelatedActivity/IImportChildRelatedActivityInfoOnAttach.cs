namespace Enterprise.MasterFiles.Business
{
	public interface IImportChildRelatedActivityInfoOnAttach
	{
		bool ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory);
	}
}
