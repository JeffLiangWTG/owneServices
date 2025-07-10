using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceCommodityAlertDirectionList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Export = "EXP";
			public const string Import = "IMP";
		}

		public static class Descriptions
		{
			public static MultilingualString Export => ResString.GetMultilingualString("RefComplianceCommodityAlertDirectionList|Export", "Export");
			public static MultilingualString Import => ResString.GetMultilingualString("RefComplianceCommodityAlertDirectionList|Import", "Import");
		}

		public RefComplianceCommodityAlertDirectionList()
		{
			AddPair(Codes.Export, Descriptions.Export);
			AddPair(Codes.Import, Descriptions.Import);
		}
	}
}
