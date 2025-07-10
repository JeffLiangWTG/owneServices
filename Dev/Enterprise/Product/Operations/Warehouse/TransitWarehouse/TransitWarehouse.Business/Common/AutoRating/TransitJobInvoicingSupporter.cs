using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitJobInvoicingSupporter<T> : JobInvoicingSupporter
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitJobInvoicingPlugIn
	{
		public TransitJobInvoicingSupporter(T businessObject) : base(businessObject)
		{
			this.businessObject = businessObject;
		}

		readonly T businessObject;

		public override JobInvoicingConsumerType ConsumerType => businessObject.ConsumerType;

		#region Security

		protected override SecurityCheckpoint GetAuditSecurityCore() => businessObject.AuditSecurity;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore() => businessObject.JobInvoicingSecurity;

		#endregion
	}
}
