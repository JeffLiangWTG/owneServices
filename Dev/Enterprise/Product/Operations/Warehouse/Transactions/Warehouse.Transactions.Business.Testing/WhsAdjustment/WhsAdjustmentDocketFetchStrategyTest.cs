using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAdjustmentDocketFetchStrategyTest : WhsDocketFetchStrategyTest<WhsAdjustment>
	{
		protected override WhsAdjustment CreateDocketAndLine(OrgHeader org, WhsWarehouse warehouse, string docketId, OrgSupplierPart part, decimal numberOfUnits)
		{
			var adjustment = Helper.CreateWhsAdjustment(org, warehouse, docketId);
			Helper.CreateWhsAdjustmentLine(adjustment, part, numberOfUnits, warehouse.DefaultLocation);
			return adjustment;
		}
	}
}
