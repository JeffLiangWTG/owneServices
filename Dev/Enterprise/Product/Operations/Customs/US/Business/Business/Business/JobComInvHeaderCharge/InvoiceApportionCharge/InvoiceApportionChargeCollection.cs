using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceApportionChargeCollection : JobComInvApportionedChargeCollection<InvoiceApportionCharge>
	{
		public InvoiceApportionChargeCollection(JobComInvoiceHeader invoice)
			: base(invoice)
		{
		}

		protected JobComInvoiceHeader Invoice => (JobComInvoiceHeader)Parent;

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
