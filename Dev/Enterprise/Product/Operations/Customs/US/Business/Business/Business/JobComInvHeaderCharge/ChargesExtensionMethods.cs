using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	public static class ChargesExtensionMethods
	{
		public static List<ApportionChargeKey> GetChargesOverriddenByLines(this InvoiceChargeCollection charges)
		{
			return GetChargesOverriddenByLines(charges, (JobComInvoiceHeader)charges.Parent);
		}

		public static List<ApportionChargeKey> GetChargesOverriddenByLines(this InvoiceApportionChargeCollection charges)
		{
			return GetChargesOverriddenByLines(charges, (JobComInvoiceHeader)charges.Parent);
		}

		static List<ApportionChargeKey> GetChargesOverriddenByLines(IEnumerable<JobComInvCharge> charges, JobComInvoiceHeader invoice)
		{
			List<ApportionChargeKey> result = new List<ApportionChargeKey>();

			foreach (JobComInvCharge charge in charges)
			{
				if (charge.J7_Amount > 0 && charge.Currency != null || charge.J7_Percentage > 0m)
				{
					if (IsOverriddenByInvoiceLine(invoice, charge))
					{
						result.Add(charge.ApportionChargeKey);
					}
				}
			}

			return result;
		}

		static bool IsOverriddenByInvoiceLine(JobComInvoiceHeader invoiceHeader, JobComInvCharge charge)
		{
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				foreach (Customs.Business.BaseJobComInvHeaderCharge overriddenLineCharge in invoiceLine.Charges)
				{
					if (!charge.ApportionChargeKey.IsFullApportionment && overriddenLineCharge.ApportionChargeKey.Equals(charge.ApportionChargeKey))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
