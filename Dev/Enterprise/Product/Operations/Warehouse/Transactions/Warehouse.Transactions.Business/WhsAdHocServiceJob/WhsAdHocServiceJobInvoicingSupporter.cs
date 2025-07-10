using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdHocServiceJobInvoicingSupporter : JobInvoicingSupporter
	{
		public WhsAdHocServiceJobInvoicingSupporter(WhsAdHocServiceJob adHocServiceJob)
			: base(adHocServiceJob)
		{
			this.Parent = adHocServiceJob;
		}

		protected readonly WhsAdHocServiceJob Parent;

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseAdHocServiceJob; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.WhsAdHocServiceJobAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.WhsAdHocServiceJobJobInvoicing;
		}

		public override OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber)
		{
			//Todo: should not default to job.LocalCharges ideally. So that we could abolish the job.LocalCharges and job.Agent eventually
			return Parent?.Client?.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, ZString.Empty, TransportMode, ContainerMode) ?? job?.LocalCharges;
		}

		public override GlbBranch OperationsBranch
		{
			get { return RegistryHelper.GetBillingOperationsBranch(Parent.Warehouse, Parent.Client); }
		}
	}
}
