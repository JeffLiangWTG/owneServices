using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration.Warehouse
{
	public interface IWhsPickOnSalesOrderDetector
	{
		bool IsComponentUsedToBuiltKitOnSalesOrder(BusinessObjectFactory factory, ZGuid productPK);
		bool IsKitBuiltOnSalesOrder(BusinessObjectFactory factory, ZGuid productPK);
	}
}
