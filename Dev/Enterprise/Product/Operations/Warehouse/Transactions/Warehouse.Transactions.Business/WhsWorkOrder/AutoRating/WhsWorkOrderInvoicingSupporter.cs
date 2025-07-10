using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region Invoicing Supporter

	public class WhsWorkOrderInvoicingSupporter : WhsPickableDocketInvoicingSupporter
	{
		public WhsWorkOrderInvoicingSupporter(WhsWorkOrder parent)
			: base(parent)
		{
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.WhsWorkOrderJobInvoicing;
		}
	}

	#endregion
}
