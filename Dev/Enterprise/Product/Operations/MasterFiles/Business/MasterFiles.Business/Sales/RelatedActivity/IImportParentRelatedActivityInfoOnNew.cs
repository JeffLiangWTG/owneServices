namespace Enterprise.MasterFiles.Business
{
	public interface IImportParentRelatedActivityInfoOnNew
	{
		bool ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory);
	}
}