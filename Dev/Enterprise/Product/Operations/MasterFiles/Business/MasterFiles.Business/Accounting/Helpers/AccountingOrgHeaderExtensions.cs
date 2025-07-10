using CargoWise.Types;
using Enterprise.Integration.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Accounting
{
	public static class AccountingOrgHeaderExtensions
	{
		public static bool IsExporterExemptCreditor(this OrgHeader org, string countryCode) => org.IsExporterExempt(countryCode, CostSell.Cost);
		public static bool IsExporterExemptDebtor(this OrgHeader org, string countryCode) => org.IsExporterExempt(countryCode, CostSell.Revenue);

		public static bool IsExporterExempt(this OrgHeader org, string countryCode, CostSell costOrSell)
		{
			if (org != null)
			{
				foreach (JobRequiredDocument jobDocument in org.RequiredDocuments)
				{
					var isExempt =
						costOrSell == CostSell.Revenue && jobDocument.EQ_DocUsage == JobRequiredDocument.DocUsage.Debtor ||
						costOrSell == CostSell.Cost && jobDocument.EQ_DocUsage == JobRequiredDocument.DocUsage.Creditor;

					isExempt = isExempt && jobDocument.EQ_DocType == RefDocTypes.VATExporterExemption;
					isExempt = isExempt && jobDocument.EQ_RN_NKRelatedCountry == countryCode;
					isExempt = isExempt && jobDocument.EQ_DateReceived <= ZDateTimeOffset.Now && ZDateTime.Now <= jobDocument.EQ_ValidToDate;

					if (isExempt)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
