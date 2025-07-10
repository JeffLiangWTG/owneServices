using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceHeaderDeepCloneStrategy : JobComInvoiceHeaderDeepCopyStrategy
	{
		public JobComInvoiceHeaderDeepCloneStrategy(BaseJobComInvoiceHeader invoice, CloneType cloneType)
			: base(invoice, cloneType)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceHeaderDeepCloneStrategy(BaseJobComInvoiceHeader invoice, CloneType cloneType, BaseJobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoice, cloneType, clonedDeclaration, pkPairsDictionaryCollection)
		{
		}

		protected override Customs.Business.JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobComInvoiceHeader)base.CloneInternal(args);
			if (bizObjToClone is JobComInvoiceHeader sourceInvoiceHeader)
			{
				using (result.GetValidationSuspender())
				using (result.SuspendSettingHasChanges())
				{
					result.TW_MarksAndNumbers = sourceInvoiceHeader.TW_MarksAndNumbers;
					DeepCopyCusPackingList(sourceInvoiceHeader, result);
				}
			}
			return result;
		}

		void DeepCopyCusPackingList(JobComInvoiceHeader invoiceHeaderToClone, JobComInvoiceHeader clonedResult)
		{
			var packingListToClone = invoiceHeaderToClone.LoadCusPackingList(invoiceHeaderToClone.Factory);
			if (packingListToClone != null)
			{
				var cusPackingListCloneStrategy = new CusPackingListDeepCloneStrategy(packingListToClone, cloneType, clonedResult, alternativeFactoryToInstantiateCloneIn);
				cusPackingListCloneStrategy.Clone();
			}
		}
	}
}
