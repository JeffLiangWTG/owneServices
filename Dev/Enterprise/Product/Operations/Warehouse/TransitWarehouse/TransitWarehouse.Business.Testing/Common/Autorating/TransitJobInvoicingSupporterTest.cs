using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class TransitJobInvoicingSupporterTest<T> : JobInvoicingSupporterTest
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		IJobInvoicingPlugIn,
		ITransitJobInvoicingPlugIn
	{
		public void TestConsumerType() => AssertEquals(ExpectedConsumerType, GetInvoicingSupporter(BusinessObject).ConsumerType);

		public void TestAuditSecurity() => AssertEquals(ExpectedAuditSecurityCheckpoint, GetInvoicingSupporter(BusinessObject).AuditSecurity);

		public void TestJobInvoicingSecurity_Consignment() => AssertEquals(ExpectedJobInvoicingSecurityCheckpoint, GetInvoicingSupporter(BusinessObject).JobInvoicingSecurity);

		#region TestIncludeInConsolCosting

		public void TestIncludeInConsolCosting()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(ExpectedIncludeInConsolCosting, invoicingSupporter.IncludeInConsolCosting(true));
			AssertEquals(ExpectedIncludeInConsolCosting, invoicingSupporter.IncludeInConsolCosting(false));
		}

		#endregion

		#region Implementation

		protected abstract IJobInvoicingSupporter GetInvoicingSupporter(T businessObject);

		protected override IJobInvoicingPlugIn GetNewBusinessObject() => Factory.NewWithValidTestData<T>();

		protected T BusinessObject => businessObject ?? (businessObject = (T)GetNewBusinessObject());
		T businessObject;

		protected abstract JobInvoicingConsumerType ExpectedConsumerType { get; }

		protected abstract SecurityCheckpoint ExpectedAuditSecurityCheckpoint { get; }

		protected abstract SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint { get; }

		protected virtual bool ExpectedIncludeInConsolCosting => false;

		protected WhsWarehouse Warehouse => warehouse ?? (warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A"));
		WhsWarehouse warehouse;

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
