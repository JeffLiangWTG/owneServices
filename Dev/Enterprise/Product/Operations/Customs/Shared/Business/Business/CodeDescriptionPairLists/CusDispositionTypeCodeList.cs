using Enterprise.ZArchitecture.Core;
using SharedCusDispositionTypeCodeList = CargoWise.Definitions.Customs.CusDispositionTypeCodeList;

namespace Enterprise.Customs.Business
{
	public partial class CusDispositionTypeCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string USPGAEntryStatus = SharedCusDispositionTypeCodeList.USPGAEntryStatus;
			public const string USPGALineStatus = SharedCusDispositionTypeCodeList.USPGALineStatus;
			public const string USQuotaLineStatus = SharedCusDispositionTypeCodeList.USQuotaLineStatus;
			public const string USAESEntryStatus = SharedCusDispositionTypeCodeList.USAESEntryStatus;
		}

		public static class Descriptions
		{
			public static MultilingualString USPGAEntryStatus { get { return ResString.GetMultilingualString("CusDispositionTypeCodeList|USPGAEntryStatus", "US PGA Entry Status"); } }
			public static MultilingualString USPGALineStatus { get { return ResString.GetMultilingualString("CusDispositionTypeCodeList|USPGALineStatus", "US PGA Line Status"); } }
			public static MultilingualString USQuotaLineStatus { get { return ResString.GetMultilingualString("CusDispositionTypeCodeList|USQuotaLineStatus", "US Quota Line Status"); } }
			public static MultilingualString USAESEntryStatus { get { return ResString.GetMultilingualString("CusDispositionTypeCodeList|USAESEntryStatus", "US AES Entry Status"); } }
		}

		public CusDispositionTypeCodeList()
		{
			AddPair(Codes.USPGAEntryStatus, Descriptions.USPGAEntryStatus);
			AddPair(Codes.USPGALineStatus, Descriptions.USPGALineStatus);
			AddPair(Codes.USQuotaLineStatus, Descriptions.USQuotaLineStatus);
			AddPair(Codes.USAESEntryStatus, Descriptions.USAESEntryStatus);
		}
	}
}
