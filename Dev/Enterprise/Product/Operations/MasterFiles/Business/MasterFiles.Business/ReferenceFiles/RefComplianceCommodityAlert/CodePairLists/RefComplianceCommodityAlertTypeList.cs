using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceCommodityAlertTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string NomenclatureAlert = "NOM";
			public const string CommodityAlert = "COM";
			public const string LocationAlert = "LOC";
		}

		public static class Descriptions
		{
			public static MultilingualString NomenclatureAlert => ResString.GetMultilingualString("RefComplianceCommodityAlertTypeList|NomenclatureAlert", "Nomenclature Alert");
			public static MultilingualString CommodityAlert => ResString.GetMultilingualString("RefComplianceCommodityAlertTypeList|CommodityAlert", "Commodity Alert");
			public static MultilingualString LocationAlert => ResString.GetMultilingualString("RefComplianceCommodityAlertTypeList|LocationAlert", "Location Alert");
		}

		public RefComplianceCommodityAlertTypeList()
		{
			AddPair(Codes.NomenclatureAlert, Descriptions.NomenclatureAlert);
			AddPair(Codes.CommodityAlert, Descriptions.CommodityAlert);
			AddPair(Codes.LocationAlert, Descriptions.LocationAlert);
		}
	}
}
