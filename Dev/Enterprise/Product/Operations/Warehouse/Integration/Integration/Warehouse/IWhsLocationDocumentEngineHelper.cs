using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration.Warehouse
{
	public interface IWhsLocationDocumentEngineHelper
	{
		void CheckForWhsLocationRecords(ZStringBuilder builder, ZGuid printQueuePk, BusinessObjectFactory factory);
	}
}
