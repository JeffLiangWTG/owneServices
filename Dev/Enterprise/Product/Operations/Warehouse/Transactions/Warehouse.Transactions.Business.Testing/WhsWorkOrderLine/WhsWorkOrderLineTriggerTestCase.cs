using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsWorkOrderLineTriggerTestCase : WhsPickableDocketLineTriggerTestCase
	{
		protected override WhsDocketLine GetNewDocketLineForNewNonFinalisedDocket(TestDataSimpleEnvironment data)
		{
			var docket = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			return GetNewDocketLineForDocket(docket, data.Part1);
		}

		protected override WhsDocketLine GetNewDocketLineForNewFinalisedDocket(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, ZDecimal qty)
		{
			var docket = Helper.CreateWhsWorkOrder(client, whs);
			var docketLine = Helper.CreateWhsWorkOrderLine(docket, product, qty);
			docket.FinaliseDocketWithoutUserConfirmation();
			return docketLine;
		}

		protected override WhsDocketLine GetNewDocketLineForDocket(WhsDocket docket, OrgSupplierPart product)
		{
			return Helper.CreateWhsWorkOrderLine((WhsWorkOrder)docket, product, 1);
		}

		protected override void AdditionalTestDataSetupForDocket(TestDataSimpleEnvironment data)
		{
			Helper.CreateProductBOM(data.Part1, data.Part2);
		}
	}
}
