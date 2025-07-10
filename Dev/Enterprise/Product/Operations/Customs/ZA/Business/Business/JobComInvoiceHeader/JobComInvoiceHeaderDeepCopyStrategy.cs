using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class JobComInvoiceHeaderDeepCopyStrategy : Customs.Business.JobComInvoiceHeaderDeepCopyStrategy
	{
		public JobComInvoiceHeaderDeepCopyStrategy(JobComInvoiceHeader invoice, CloneType cloneType, JobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoice, cloneType, clonedDeclaration, pkPairsDictionaryCollection)
		{
		}

		protected override JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new ZAJobComInvoiceLineDeepCloneStrategy((JobComInvoiceLine)invoiceLineToClone, cloneType, (JobComInvoiceHeader)clonedInvoice, pkPairsDictionaryCollection);
		}
	}
}
