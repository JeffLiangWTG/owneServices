using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderLineFetchStrategyTest : WhsDocketLineFetchStrategyTest<WhsOrder, WhsOrderLine>
	{
		protected override void AdditionalExpectedDbHitsForFetchForView(Dictionary<string, int> expectedDbHits)
		{
			base.AdditionalExpectedDbHitsForFetchForView(expectedDbHits);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
		}

		protected override WhsOrder CreateDocket(OrgHeader client, WhsWarehouse warehouse, string reference)
		{
			return Helper.CreateWhsOrder(client, warehouse, reference);
		}

		protected override void CreateDocketLine(WhsOrder order, OrgSupplierPart part, WhsLocation location, decimal quantity)
		{
			Helper.CreateWhsOrderLine(order, part, quantity, "", "", "");
		}
	}
}
