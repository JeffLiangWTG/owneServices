namespace Enterprise.MasterFiles.Business
{
	public interface IRefreshProcessTaskCollection
	{
		void RefreshProcessTaskCollection(bool reLoadExistingRows);
	}
}
