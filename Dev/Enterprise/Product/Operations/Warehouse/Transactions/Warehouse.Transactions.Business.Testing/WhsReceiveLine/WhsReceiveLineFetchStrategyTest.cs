using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveLineFetchStrategyTest : WhsDocketLineFetchStrategyTest<WhsReceive, WhsReceiveLine>
	{
		protected override void AdditionalExpectedDbHitsForFetchForView(Dictionary<string, int> expectedDbHits)
		{
			base.AdditionalExpectedDbHitsForFetchForView(expectedDbHits);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
		}

		protected override WhsReceive CreateDocket(OrgHeader client, WhsWarehouse warehouse, string reference)
		{
			return Helper.CreateWhsReceive(client, warehouse, reference);
		}

		protected override void CreateDocketLine(WhsReceive receive, OrgSupplierPart part, WhsLocation location, decimal quantity)
		{
			Helper.CreateWhsReceiveInventoryLine(receive, part, quantity);
		}
	}
}
