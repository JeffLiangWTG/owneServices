using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsAdjustmentInvoicingSupporter : WhsDocketInvoicingSupporter
	{
		public WhsAdjustmentInvoicingSupporter(WhsAdjustment parent) : base(parent)
		{
		}

		#region ConsumerType

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseStorage; }
		}

		#endregion

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.WhsInvoicingAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.WhsInvoicingJobInvoicing;
		}
	}
}
