using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobComInvoiceHeaderDeepCopyStrategy : EU.Business.Declaration.JobComInvoiceHeaderDeepCopyStrategy
{
	public JobComInvoiceHeaderDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, CloneType cloneType, BaseJobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
		: base(invoiceToClone, cloneType, clonedDeclaration, pkPairsDictionaryCollection)
	{
	}

	new JobComInvoiceHeader InvoiceToClone => (JobComInvoiceHeader)base.InvoiceToClone;

	protected override Customs.Business.JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
	{
		return new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection);
	}

	protected override void CloneExtraInvoiceDataInSpecificCountry(BusinessObjectCloneArgs args, BaseJobComInvoiceHeader clonedInvoice)
	{
		if (clonedDeclaration.IsImport)
		{
			var clonedInvoicePL = (JobComInvoiceHeader)clonedInvoice;
			clonedInvoicePL.TranCircumstanceCode1 = InvoiceToClone.TranCircumstanceCode1;

			foreach (TranCircumstance tranCircumstance in InvoiceToClone.AdditionalTranCircumstanceCodes)
			{
				clonedInvoicePL.AdditionalTranCircumstanceCodes.Add(tranCircumstance.Clone(args));
			}
		}
	}
}
