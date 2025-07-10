using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWorkOrderLineFetchStrategyTest : WhsDocketLineFetchStrategyTest<WhsWorkOrder, WhsWorkOrderLine>
	{
		protected override void AdditionalExpectedDbHitsForFetchForView(Dictionary<string, int> expectedDbHits)
		{
			base.AdditionalExpectedDbHitsForFetchForView(expectedDbHits);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
		}

		protected override WhsWorkOrder CreateDocket(OrgHeader client, WhsWarehouse warehouse, string reference)
		{
			return Helper.CreateWhsWorkOrder(client, warehouse, reference);
		}

		protected override void CreateDocketLine(WhsWorkOrder docket, OrgSupplierPart part, WhsLocation location, decimal quantity)
		{
			Helper.CreateWhsWorkOrderLine(docket, part, quantity);
		}
	}
}
