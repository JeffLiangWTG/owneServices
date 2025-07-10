using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class ContainerNonCustomsAdditionalReferenceCodesCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string CarrierMessageReference = "CMR";
		}

		public static class Descriptions
		{
			public static MultilingualString CarrierMessageReference => ResString.GetMultilingualString("443694A6-4844-4FCB-AFBA-33E8F4ED430B", "Carrier Message Reference");
		}

		public ContainerNonCustomsAdditionalReferenceCodesCodeList()
		{
			AddPair(Codes.CarrierMessageReference, Descriptions.CarrierMessageReference);
		}
	}
}
