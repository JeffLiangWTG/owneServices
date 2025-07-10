using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectInvoicingSupporter : JobInvoicingSupporter
	{
		public ProjectInvoicingSupporter(Project parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly Project Parent;

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Project; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.ProjectAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.ProjectJobInvoicing;
		}

		public override OrgHeader Consignee
		{
			get { return Parent.ClientOrganisation; }
		}

		public override OrgHeader Consignor
		{
			get { return Parent.ClientOrganisation; }
		}
	}
}
