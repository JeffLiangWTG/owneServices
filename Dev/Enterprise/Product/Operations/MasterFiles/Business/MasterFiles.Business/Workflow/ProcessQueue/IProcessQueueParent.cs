using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IProcessQueueParent : IBusiness
	{
		ActiveProcessQueueCollection ActiveProcessQueueForBinding { get; }

		ProcessQueue CurrentQueue { get; }
		string TablePrefix { get; }
		ZGuid PK { get; }
	}
}
