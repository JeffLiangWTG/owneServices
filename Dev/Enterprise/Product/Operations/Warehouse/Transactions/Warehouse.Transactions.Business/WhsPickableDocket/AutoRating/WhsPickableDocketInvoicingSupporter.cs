using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickableDocketInvoicingSupporter : WhsDocketInvoicingSupporter
	{
		public WhsPickableDocketInvoicingSupporter(WhsDocket parent)
			: base(parent)
		{
		}

		#region ConsumerType

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseOutwards; }
		}

		#endregion

		#region AuditBillingSecurity

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return auditBillingSecurity;
		}

		public void SetAuditBillingSecurity(SecurityCheckpoint security)
		{
			auditBillingSecurity = security;
		}

		SecurityCheckpoint auditBillingSecurity;

		#endregion
	}
}
