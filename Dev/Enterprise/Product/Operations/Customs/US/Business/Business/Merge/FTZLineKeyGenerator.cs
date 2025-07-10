using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	class FTZLineKeyGenerator : ILineKeyGenerator
	{
		public void AddKey(MergeKey result, JobComInvoiceLine invoiceLine)
		{
			result.Add(invoiceLine.JI_JZ);

			//merge order of invoice lines depend on this existence
			var ftzBill = invoiceLine.InvoiceHeader.FTZBill;

			result.Add(ftzBill != null ? ftzBill.CU_BillUniqueCode : ZString.Empty);

			if (invoiceLine != null &&
				invoiceLine.Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMerge &&
				invoiceLine.Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription)
			{
				ZGuid doNotMergeKey = ImportEntryCreationStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine) ? invoiceLine.PK : ZGuid.Empty;

				result.Add(doNotMergeKey);
				result.Add(invoiceLine.JI_Tariff);
				result.Add(invoiceLine.US_SupTariff);
				result.Add(invoiceLine.US_SPI);
				result.Add(invoiceLine.US_SecondarySPI);
				result.Add(invoiceLine.US_UC_NKCountryOfOrigin);
				result.Add(invoiceLine.US_TextileCategoryNo);
				result.Add(invoiceLine.US_F_PNDisclaimer);
				result.Add(invoiceLine.US_ZoneStatus);
				result.Add(invoiceLine.JI_OA_ManufacturerAddress);
			}
		}
	}
}
