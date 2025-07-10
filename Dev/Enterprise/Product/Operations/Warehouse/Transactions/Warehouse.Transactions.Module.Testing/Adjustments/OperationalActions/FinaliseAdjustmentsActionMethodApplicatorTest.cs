using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(FinaliseAdjustmentsActionMethodApplicator))]
	public class FinaliseAdjustmentsActionMethodApplicatorTest : FinaliseDocketActionMethodApplicatorTest<WhsAdjustment, FinaliseAdjustmentsActionMethodApplicator>
	{
		protected override WhsAdjustment GetNewDocket(WhsTestHelperFunctions helper, OrgHeader client, WhsWarehouse warehouse, string reference = "1")
		{
			return helper.CreateWhsAdjustment(client, warehouse, reference);
		}

		protected override WhsDocketLine GetNewDocketLine(WhsTestHelperFunctions helper, WhsAdjustment docket, OrgSupplierPart part, ZDecimal quantity)
		{
			return helper.CreateWhsAdjustmentLine(docket, part, quantity, docket.Warehouse.DefaultLocation);
		}
	}
}
