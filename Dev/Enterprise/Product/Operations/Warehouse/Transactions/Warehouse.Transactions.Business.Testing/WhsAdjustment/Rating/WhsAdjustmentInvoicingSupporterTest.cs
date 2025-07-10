using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentInvoicingSupporter))]
	public class WhsAdjustmentInvoicingSupporterTest : WhsDocketInvoicingSupporterTest
	{
		#region ExpectedConsumerType

		protected override JobInvoicingConsumerType ExpectedConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseStorage; }
		}

		#endregion

		#region TestAuditSecurity

		public void TestAuditSecurity()
		{
			var supporter = GetNewSupporter(Docket);
			AssertEquals("AuditSecurity must be WhsInvoicingAuditBilling", Env.Security.WhsInvoicingAuditBilling, supporter.AuditSecurity);
		}

		#endregion

		protected override WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsAdjustment>();
		}

		protected override WhsDocketInvoicingSupporter GetNewSupporter(WhsDocket parent)
		{
			return new WhsAdjustmentInvoicingSupporter((WhsAdjustment)parent);
		}
	}
}
