using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWorkOrderDocketFetchStrategyTest : WhsPickableDocketFetchStrategyTest<WhsWorkOrder>
	{
		protected override WhsWorkOrder CreateDocketAndLine(OrgHeader org, WhsWarehouse warehouse, string docketId, OrgSupplierPart part, decimal numberOfUnits)
		{
			return Helper.CreateWhsWorkOrderWithLine(org, warehouse, docketId, part, numberOfUnits);
		}
	}
}
