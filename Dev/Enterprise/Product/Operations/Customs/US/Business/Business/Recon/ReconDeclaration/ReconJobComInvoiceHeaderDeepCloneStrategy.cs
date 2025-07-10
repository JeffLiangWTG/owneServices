using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconJobComInvoiceHeaderDeepCloneStrategy : JobComInvoiceHeaderDeepCopyStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ReconJobComInvoiceHeaderDeepCloneStrategy(JobComInvoiceHeader invoice, CloneType cloneType, JobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoice, cloneType, clonedDeclaration, pkPairsDictionaryCollection)
		{
		}

		protected override JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new ReconJobComInvoiceLineDeepCloneStrategy((JobComInvoiceLine)invoiceLineToClone, cloneType, (JobComInvoiceHeader)clonedInvoice, pkPairsDictionaryCollection);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			JobComInvoiceHeader result = (JobComInvoiceHeader)base.CloneInternal(args);

			var pkPairsCusEntryHeaders = new Dictionary<ZGuid, ZGuid>();
			if (pkPairsDictionaryCollection.TryGetValue("cusEntryHeaderExistedPKPairs", out pkPairsCusEntryHeaders))
			{
				ZGuid newEntryPCIHeaderPK;
				if (pkPairsCusEntryHeaders.TryGetValue(((JobComInvoiceHeader)InvoiceToClone).US_CH_ReconEntry, out newEntryPCIHeaderPK))
				{
					result.US_CH_ReconEntry = newEntryPCIHeaderPK;
					result.GetAddInfo().UpdateRelatedPropertyInfo();
				}
			}

			result.InvoiceLines.OfType<JobComInvoiceLine>().ForEach(x => x.ResetValuesForRecon());
			return result;
		}
	}
}
