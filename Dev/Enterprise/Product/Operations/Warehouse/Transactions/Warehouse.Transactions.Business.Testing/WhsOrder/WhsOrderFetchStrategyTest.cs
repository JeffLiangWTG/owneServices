using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderFetchStrategyTest : WhsPickableDocketFetchStrategyTest<WhsOrder>
	{
		protected override WhsDocketFetchStrategy GetNewFetchStrategy()
		{
			return new WhsOrderFetchStrategy(Factory.New<WhsOrder>());
		}

		protected override WhsOrder CreateDocketAndLine(OrgHeader org, WhsWarehouse warehouse, string docketId, OrgSupplierPart part, decimal numberOfUnits)
		{
			return Helper.CreateWhsOrderWithOrderLine(org, warehouse, docketId, part, numberOfUnits);
		}
	}
}
