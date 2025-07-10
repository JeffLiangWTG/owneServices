using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestInvoicingSupporter : JobInvoicingSupporter
	{
		public WorkRequestInvoicingSupporter(WorkRequest parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly WorkRequest Parent;

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.WorkRequest;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.ProjectAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.ProjectJobInvoicing;
		}

		public override ZGuid OverriddenDepartmentPK => Parent.WKR_GE_Department;
	}
}
