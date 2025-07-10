using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemInvoicingSupporter : JobInvoicingSupporter
	{
		public WorkItemInvoicingSupporter(WorkItemCommon workItem)
			: base(workItem)
		{
			this.Parent = workItem;
		}

		protected readonly WorkItemCommon Parent;

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.WorkItem; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.WorkItemAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.WorkItemJobInvoicing;
		}
	}
}
