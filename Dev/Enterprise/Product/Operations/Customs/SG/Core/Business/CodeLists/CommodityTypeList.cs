using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CommodityTypeList : CodeDescriptionPairList
	{
		public CommodityTypeList()
			: base()
		{
			AddPair(CommodityTypeList.Codes.Alcohol, ResString.GetMultilingualString("3909E45E-BE33-4F72-B7BE-F799D80E6CCA", "Alcohol"));
			AddPair(CommodityTypeList.Codes.Petroleum, ResString.GetMultilingualString("58F08368-0BAB-4968-A483-B372CAACCE3E", "Petroleum"));
			AddPair(CommodityTypeList.Codes.Tobacco, ResString.GetMultilingualString("80B05846-F1B5-44E9-9DE8-58F2299342EA", "Tobacco"));
			AddPair(CommodityTypeList.Codes.Vehicle, ResString.GetMultilingualString("ADF51319-95B2-4EC5-93C6-14BAE1261A36", "Vehicle"));
		}

		public static class Codes
		{
			public const string Alcohol = "ALC";
			public const string Petroleum = "PET";
			public const string Tobacco = "TOB";
			public const string Vehicle = "VEH";
		}
	}
}
