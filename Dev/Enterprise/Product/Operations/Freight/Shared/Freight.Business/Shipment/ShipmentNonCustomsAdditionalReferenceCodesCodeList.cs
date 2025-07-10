using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class ShipmentNonCustomsAdditionalReferenceCodesCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string TWR = TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceive;
			public const string HIR = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			public const string CMR = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference;
			public const string SPT = "SPT";
		}

		public static class Descriptions
		{
			public static MultilingualString TWR => ResString.GetMultilingualString("F37B4FFB-0C81-4E71-879D-1AB3660F079F", TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseReceive);
			public static MultilingualString HIR => ResString.GetMultilingualString("03cdc422-39c0-47d6-9614-6e83adc26933", "eHub Interchange Reference");
			public static MultilingualString CMR => ResString.GetMultilingualString("59F1FB13-4C82-4A50-B5B6-CDC10A27DC8C", "Carrier Message Reference");
			public static MultilingualString SPT => ResString.GetMultilingualString("065879d6-9be5-4fbc-993d-e39ce711a91b", "Virtual shipment number");
		}

		public ShipmentNonCustomsAdditionalReferenceCodesCodeList()
		{
			AddPair(Codes.TWR, Descriptions.TWR);
			AddPair(Codes.HIR, Descriptions.HIR);
			AddPair(Codes.CMR, Descriptions.CMR);
			AddPair(Codes.SPT, Descriptions.SPT);
		}
	}
}
