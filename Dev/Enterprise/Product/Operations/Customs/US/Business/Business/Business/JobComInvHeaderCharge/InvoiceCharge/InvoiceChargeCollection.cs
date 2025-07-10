using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceChargeCollection : JobComInvChargeCollection<InvoiceCharge>
	{
		public InvoiceChargeCollection(JobComInvoiceHeader invoice)
			: base(invoice)
		{
		}

		public void RefreshChargesNotOverriddenLines()
		{
			chargesOverriddenByLines = null;
		}

		public bool IsOverriddenByLines(JobComInvCharge charge)
		{
			if (chargesOverriddenByLines == null)
			{
				chargesOverriddenByLines = this.GetChargesOverriddenByLines();
			}

			return chargesOverriddenByLines.Contains(charge.ApportionChargeKey);
		}
		List<ApportionChargeKey> chargesOverriddenByLines;
	}
}
