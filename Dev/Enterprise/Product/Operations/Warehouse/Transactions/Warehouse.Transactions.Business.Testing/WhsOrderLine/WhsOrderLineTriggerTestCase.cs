using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderLineTriggerTestCase : WhsPickableDocketLineTriggerTestCase
	{
		protected override WhsDocketLine GetNewDocketLineForNewNonFinalisedDocket(TestDataSimpleEnvironment data)
		{
			var docket = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			return Helper.CreateWhsOrderLine(docket, data.Part1, 1);
		}

		protected override WhsDocketLine GetNewDocketLineForNewFinalisedDocket(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, ZDecimal qty)
		{
			var docket = Helper.CreateWhsOrder(client, whs);
			var docketLine = Helper.CreateWhsOrderLine(docket, product, 1);
			docketLine.WE_TransactionQuantity = qty;
			docket.FinaliseDocketWithoutUserConfirmation();
			return docketLine;
		}

		protected override WhsDocketLine GetNewDocketLineForDocket(WhsDocket docket, OrgSupplierPart product)
		{
			return Helper.CreateWhsOrderLine((WhsOrder)docket, product, 1);
		}
	}
}
