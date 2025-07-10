using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAJobComInvoiceLineDeepCloneStrategy : JobComInvoiceLineDeepCloneStrategy
	{
		public ZAJobComInvoiceLineDeepCloneStrategy(JobComInvoiceLine invoiceLineToClone, CloneType cloneType, JobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobComInvoiceLine)base.CloneInternal(args);
			var lineToClone = (JobComInvoiceLine)invoiceLineToClone;
			foreach (CusLineTariffDetail detail in lineToClone.CusLineTariffDetails)
			{
				var detailCloned = (CusLineTariffDetail)detail.Clone(args);
				result.CusLineTariffDetails.Add(detailCloned);
			}
			return result;
		}

		protected override void SetEntryInstructionForJobComInvoiceLine(BaseJobComInvoiceLine clonedInvoiceLine)
		{
			if (!invoiceLineToClone.JI_CEI.IsEmpty && pkPairsDictionaryCollection != null)
			{
				if (pkPairsDictionaryCollection.TryGetValue(JobDeclarationDeepCloneStrategy.CusEntryInstructionPKPairsKey, out var pkPairs) && pkPairs.TryGetValue(invoiceLineToClone.JI_CEI, out var ceiPK))
				{
					using (((JobComInvoiceLine)clonedInvoiceLine).SuspendCusLineTariffDetailDefaulting())
					{
						clonedInvoiceLine.JI_CEI = ceiPK;
					}
				}
			}
		}
	}
}
