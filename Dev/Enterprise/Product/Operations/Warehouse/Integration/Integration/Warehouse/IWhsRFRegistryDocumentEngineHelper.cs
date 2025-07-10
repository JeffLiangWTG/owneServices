using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration.Warehouse
{
	public interface IWhsRFRegistryDocumentEngineHelper
	{
		void CheckForWhsRFRegistryRecords(ZStringBuilder builder, ZGuid printQueuePk, BusinessObjectFactory factory);
	}
}
