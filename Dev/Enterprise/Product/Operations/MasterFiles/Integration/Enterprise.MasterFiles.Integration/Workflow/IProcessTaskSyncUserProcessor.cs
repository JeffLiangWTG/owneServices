using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IProcessTaskSyncUserProcessor
	{
		void SyncUser(BusinessObjectFactory factory, IProcessTask task);
	}
}
