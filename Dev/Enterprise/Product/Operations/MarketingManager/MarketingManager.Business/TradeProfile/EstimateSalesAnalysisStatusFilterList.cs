using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class EstimateSalesAnalysisStatusFilterList : CodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public class Codes : OrgSalesActualsStatusList.Codes
		{
			public const string All = "ALL";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Descriptions
		{
			public static MultilingualString All { get { return ResString.GetMultilingualString("EstimateSalesAnalysisStatusFilterList|All", "Show All"); } }
			public static MultilingualString Lost { get { return ResString.GetMultilingualString("EstimateSalesAnalysisStatusFilterList|Lost", "Show Lost Only"); } }
			public static MultilingualString Prospective { get { return ResString.GetMultilingualString("EstimateSalesAnalysisStatusFilterList|Prospective", "Show Prospective Estimates Only"); } }
			public static MultilingualString Traded { get { return ResString.GetMultilingualString("EstimateSalesAnalysisStatusFilterList|Traded", "Show Traded Estimates Only"); } }
		}

		public EstimateSalesAnalysisStatusFilterList()
		{
			AddPair(Codes.All, Descriptions.All);
			AddPair(Codes.Lost, Descriptions.Lost);
			AddPair(Codes.Prospective, Descriptions.Prospective);
			AddPair(Codes.Traded, Descriptions.Traded);
		}
	}
}
