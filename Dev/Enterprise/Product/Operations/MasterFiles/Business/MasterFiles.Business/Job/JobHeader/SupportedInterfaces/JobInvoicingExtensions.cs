using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class JobInvoicingExtensions
	{
		public static ZGuid GetDebtorPK(this JobHeader job, JobCharge charge)
		{
			Argument.NotNull(job, nameof(job));

			var chargeCode = charge?.ChargeCode;
			var relatedJobNumber = charge?.JR_Calc_RelatedJobNumber ?? ZString.Empty;

			return job.GetDebtorPK(chargeCode, relatedJobNumber);
		}

		public static OrgHeader GetDefaultDebtor(this IJobInvoicingSupporter invoicingSupporter, JobCharge charge)
		{
			Argument.NotNull(invoicingSupporter, nameof(invoicingSupporter));

			var chargeCode = charge?.ChargeCode;
			var job = charge?.Job;
			var relatetJobNumber = charge?.JR_Calc_RelatedJobNumber ?? ZString.Empty;

			return invoicingSupporter.GetDefaultDebtor(chargeCode, job, relatetJobNumber);
		}
	}
}
