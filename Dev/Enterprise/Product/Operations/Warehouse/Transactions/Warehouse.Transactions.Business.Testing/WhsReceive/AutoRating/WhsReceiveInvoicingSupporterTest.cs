using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveInvoicingSupporter))]
	public class WhsReceiveInvoicingSupporterTest : WhsDocketInvoicingSupporterTest
	{
		#region TestCreateAccountingJobOnSavingOfOperationsJob

		public void TestCreateAccountingJobOnSavingOfOperationsJob_CreatedFromPickByBOM()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			AssertEquals(true, receive.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);

			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();
			AssertEquals(false, receive.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		#endregion

		#region TestGetReasonNotToAllowAutoRate_CreatedFromPickByBOM

		public void TestGetReasonNotToAllowAutoRate_CreatedFromPickByBOM()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var supporter = GetNewSupporter(receive);
			AssertNull($"{receive.HumanReadableName} should validate with no errors.", supporter.GetReasonNotToAllowAutoRate());

			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();
			AssertEquals($"Receive {receive.HumanReadableName} is created from Pick Order and cannot be Auto Rated.", supporter.GetReasonNotToAllowAutoRate());
		}

		#endregion

		#region ExpectedConsumerType

		protected override JobInvoicingConsumerType ExpectedConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseInwards; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		protected override bool IsIJobInvoicingPlugIn_DefaultDebtorOverriden
		{
			get { return true; }
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsReceive>();
		}

		protected override WhsDocketInvoicingSupporter GetNewSupporter(WhsDocket parent)
		{
			return new WhsReceiveInvoicingSupporter((WhsReceive)parent);
		}

		#endregion
	}
}
