using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderInvoicingSupporter : JobInvoicingSupporter
	{
		public WhsVASOrderInvoicingSupporter(WhsVASOrder vasOrder)
			: base(vasOrder)
		{
			Parent = vasOrder;
		}

		protected readonly WhsVASOrder Parent;

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.WarehouseVASOrder;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore() => Env.Security.WhsVASOrderJobInvoicing;
	}
}
