using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface ITriggerActionRootProvider
	{
		IBusiness[] GetRoots(IProcessTaskNotification action, BusinessObject parent, IStmALog @event);
	}
}
