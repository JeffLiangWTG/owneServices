using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDJobInvoicingSupporter<T> : JobInvoicingSupporter
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ICYDJobInvoicingSupporter
	{
		public CYDJobInvoicingSupporter(T parent) : base(parent)
		{
			this.parent = parent;
		}

		public override OrgHeader OverriddenDefaultLocalClient => parent.DefaultClient;

		readonly T parent;

		public override JobInvoicingConsumerType ConsumerType => parent.ConsumerType;

		#region Security

		protected override SecurityCheckpoint GetAuditSecurityCore() => parent.AuditSecurity;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore() => parent.JobInvoicingSecurity;

		#endregion
	}
}
