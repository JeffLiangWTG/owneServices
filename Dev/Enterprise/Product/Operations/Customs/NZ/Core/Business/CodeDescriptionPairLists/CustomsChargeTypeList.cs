
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class CustomsChargeTypeList : Customs.Business.CustomsChargeTypeList
	{
		public CustomsChargeTypeList()
			: base()
		{
		}

		public CustomsChargeTypeList(bool isImportTSWDeclaration)
			: base()
		{
			if (isImportTSWDeclaration)
			{
				AddPair(TSWCodes.Royalties, TSWDescriptions.Royalties);
			}
		}

		public static class TSWCodes
		{
			public const string Royalties = "RYL";
		}

		public static class TSWDescriptions
		{
			public static ResourceString Royalties = ResString.GetMultilingualString("E44F60C8-CBFB-4F8F-9DBE-6B3509E38028", "Royalties");
		}
	}
}
