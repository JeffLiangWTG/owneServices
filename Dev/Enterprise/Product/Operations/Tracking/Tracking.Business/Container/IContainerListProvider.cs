using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public interface IContainerListProvider
	{
		RefContainerCollection Container_List { get; }
	}
}
