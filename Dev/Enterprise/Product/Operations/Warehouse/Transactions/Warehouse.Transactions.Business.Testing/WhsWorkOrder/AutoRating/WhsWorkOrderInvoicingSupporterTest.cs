using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderInvoicingSupporter))]
	public class WhsWorkOrderInvoicingSupporterTest : WhsPickableDocketInvoicingSupporterTest
	{
		#region ExpectedConsumerType

		protected override JobInvoicingConsumerType ExpectedConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseOutwards; }
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsWorkOrder>();
		}

		protected override WhsDocketInvoicingSupporter GetNewSupporter(WhsDocket parent)
		{
			return new WhsWorkOrderInvoicingSupporter((WhsWorkOrder)parent);
		}

		#endregion
	}
}
