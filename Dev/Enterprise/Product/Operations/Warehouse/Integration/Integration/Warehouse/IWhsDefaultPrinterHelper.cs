using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration.Warehouse
{
	public interface IWhsDefaultPrinterHelper
	{
		void CheckForStmDefaultPrinterRecords(ZStringBuilder builder, ZGuid printQueuePk, BusinessObjectFactory factory);
	}
}
