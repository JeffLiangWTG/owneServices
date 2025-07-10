using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceHeaderCloneStrategy : Customs.Business.JobComInvoiceHeaderDeepCopyStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceHeaderCloneStrategy(JobComInvoiceHeader invoiceToClone, Customs.Business.CloneType cloneType, JobDeclaration clonedDec, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceToClone, cloneType, clonedDec, pkPairsDictionaryCollection)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			JobComInvoiceHeader result = (JobComInvoiceHeader)base.CloneInternal(args);

			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.JZ_ValuationDateOverride = ZDateTime.Empty;
				result.SetExchangeRateIfNotUserEntered();
			}

			return result;
		}
	}
}
