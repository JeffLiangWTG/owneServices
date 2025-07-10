using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Invoicing.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPeriodicInvoicingInvoicingSupporter(PeriodicInvoicing parent) : PeriodicInvoicingInvoicingSupporter(parent)
	{
		public override OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber)
		{
			return Parent.Client;
		}

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.CYDPeriodicInvoicing;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.CYDPeriodicInvoicingAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.CYDPeriodicInvoicingJobInvoicing;
		}
	}
}
