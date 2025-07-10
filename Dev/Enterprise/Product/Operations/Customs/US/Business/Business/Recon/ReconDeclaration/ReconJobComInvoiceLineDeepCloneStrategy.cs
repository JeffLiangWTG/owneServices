using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class ReconJobComInvoiceLineDeepCloneStrategy : Customs.Business.JobComInvoiceLineDeepCloneStrategy
	{
		public ReconJobComInvoiceLineDeepCloneStrategy(JobComInvoiceLine invoiceLineToClone, Customs.Business.CloneType cloneType, JobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedArgs = new BusinessObjectCloneArgs(
				args.AlternativeFactoryToInstantiateCloneIn,
				args.GetExcludedColumns(),
				args.TypeToCloneAs,
				args.PerformRowCopyWithoutTriggeringValidationAndSetter,
				args.CopyDecider);

			clonedArgs.AddExcludedColumns(new string[] {
				JobComInvoiceLine.Schema.US_R_OrigHMFAmount,
				JobComInvoiceLine.Schema.US_R_OrigOverrideSupDuty,
				JobComInvoiceLine.Schema.US_R_OrigCV,
				JobComInvoiceLine.Schema.US_R_OrigDuty,
				JobComInvoiceLine.Schema.US_R_OrigFirstQty,
				JobComInvoiceLine.Schema.US_R_OrigFirstUQ,
				JobComInvoiceLine.Schema.US_R_OrigOverrideDuty,
				JobComInvoiceLine.Schema.US_R_OrigRateType,
				JobComInvoiceLine.Schema.US_R_OrigSPI,
				JobComInvoiceLine.Schema.US_R_OrigSecondQty,
				JobComInvoiceLine.Schema.US_R_OrigSecondUQ,
				JobComInvoiceLine.Schema.US_R_OrigTariff,
				JobComInvoiceLine.Schema.US_R_OrigSupQty1,
				JobComInvoiceLine.Schema.US_R_OrigSupUQ1,
				JobComInvoiceLine.Schema.US_R_OrigSupQty2,
				JobComInvoiceLine.Schema.US_R_OrigSupUQ2,
				JobComInvoiceLine.Schema.US_R_OrigSupQty3,
				JobComInvoiceLine.Schema.US_R_OrigSupUQ3,
				JobComInvoiceLine.Schema.US_R_OrigSupDuty
			});

			var result = (JobComInvoiceLine)base.CloneInternal(args);
			var invline = (JobComInvoiceLine)invoiceLineToClone;

			using (result.GetValidationSuspender())
			{
				foreach (FeeCusCodeData cusCode in invline.FeeCusCodes)
				{
					var newFee = (FeeCusCodeData)cusCode.Clone();
					result.FeeCusCodes.Add(newFee);

					var newCharge = result.ReconOriginalCharges.AddNew(newFee.CY_Code, newFee.CY_Data);
					newCharge.CY_IsOverridden = newFee.CY_IsOverridden;
				}
			}
			return result;
		}
	}
}
