using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickableDocketInvoicingSupporter))]
	public class WhsPickableDocketInvoicingSupporterTest : WhsDocketInvoicingSupporterTest
	{
		#region ExpectedConsumerType

		protected override JobInvoicingConsumerType ExpectedConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseOutwards; }
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ActualChargeableUnit

		public override void TestIJobInvoicingPlugIn_ActualChargeableUnit()
		{
			var pickableDocket = (WhsDocket)GetNewBusinessObject();
			var jobInvPlugIn = GetNewSupporter(pickableDocket);
			AssertEquals(ZString.Empty, jobInvPlugIn.ActualChargeableUnit);
		}

		#endregion

		#region TestGetReasonNotToAllowAutoRate

		public void TestGetReasonNotToAllowAutoRate_PickingDocket()
		{
			TestGetReasonNotToAllowAutoRateCore(DocketStatus.Codes.Picking, false);
		}

		#endregion

		protected override bool IsIJobInvoicingPlugIn_DefaultDebtorOverriden
		{
			get { return true; }
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsOrder>();
		}

		protected override WhsDocketInvoicingSupporter GetNewSupporter(WhsDocket parent)
		{
			return new WhsPickableDocketInvoicingSupporter(parent);
		}
	}
}
