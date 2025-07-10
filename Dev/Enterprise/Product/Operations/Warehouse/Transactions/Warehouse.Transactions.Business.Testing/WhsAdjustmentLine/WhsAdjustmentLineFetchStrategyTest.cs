using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAdjustmentLineFetchStrategyTest : WhsDocketLineFetchStrategyTest<WhsAdjustment, WhsAdjustmentLine>
	{
		protected override void AdditionalExpectedDbHitsForFetchForView(Dictionary<string, int> expectedDbHits)
		{
			base.AdditionalExpectedDbHitsForFetchForView(expectedDbHits);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
		}

		protected override WhsAdjustment CreateDocket(OrgHeader client, WhsWarehouse warehouse, string reference)
		{
			return Helper.CreateWhsAdjustment(client, warehouse, reference);
		}

		protected override void CreateDocketLine(WhsAdjustment adjustment, OrgSupplierPart part, WhsLocation location, decimal quantity)
		{
			Helper.CreateWhsAdjustmentLine(adjustment, part, quantity, location);
		}
	}
}
