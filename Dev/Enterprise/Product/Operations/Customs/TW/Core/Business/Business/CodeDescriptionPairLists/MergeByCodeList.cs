using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class MergeByCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string CondensedDeclaration = OrgConstants.MergeInvoiceLines.Tariff;
		}

		public MergeByCodeList()
		{
			AddPair(OrgConstants.MergeInvoiceLines.NotMerge, ResString.GetMultilingualString("{A7FD0C71-6D94-4E37-BCA7-A086513FB96B}", "No Merge"));
			AddPair(OrgConstants.MergeInvoiceLines.TariffAndDescription, ResString.GetMultilingualString("{88C0C9F8-409E-4B4B-8BFB-96E94BF38CC4}", "Tariff and Description"));
			AddPair(OrgConstants.MergeInvoiceLines.PartNumber, ResString.GetMultilingualString("{3DAABC35-22E6-4539-A5EF-2F647947B92A}", "Product Number"));
			AddPair(Codes.CondensedDeclaration, ResString.GetMultilingualString("{CD2E884E-6520-4723-81BC-522AAA57AA63}", "Condensed Declaration"));
		}
	}
}
