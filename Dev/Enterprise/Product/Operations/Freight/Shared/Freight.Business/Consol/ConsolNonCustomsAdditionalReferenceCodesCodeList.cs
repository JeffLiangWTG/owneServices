using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class ConsolNonCustomsAdditionalReferenceCodesCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string CarrierShipperReference = "CSR";
			public const string CarrierMessageReference = "CMR";
		}

		public static class Descriptions
		{
			public static MultilingualString CarrierShipperReference => ResString.GetMultilingualString("12fe45fa-5983-4a46-91b3-80974aaeaaaf", "Carrier Shipper Reference");
			public static MultilingualString CarrierMessageReference => ResString.GetMultilingualString("70CA51BC-4BD3-4DA3-AF14-3034B94759FE", "Carrier Message Reference");
		}

		public ConsolNonCustomsAdditionalReferenceCodesCodeList()
		{
			AddPair(Codes.CarrierShipperReference, Descriptions.CarrierShipperReference);
			AddPair(Codes.CarrierMessageReference, Descriptions.CarrierMessageReference);
		}
	}
}
