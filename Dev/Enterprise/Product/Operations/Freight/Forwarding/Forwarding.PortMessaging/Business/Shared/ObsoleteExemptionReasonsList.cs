using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class ObsoleteExemptionReasonsList : CodeDescriptionPairList
	{
		public ObsoleteExemptionReasonsList(string entryType)
		{
			if (entryType == EntryTypeList.Codes.Message)
			{
				this.AddPair(Codes.ExemptionReason4, Descriptions.ExemptionReason4);
				this.AddPair(Codes.ExemptionReason6, Descriptions.ExemptionReason6);
			}
		}

		public static class Codes
		{
			public const string ExemptionReason4 = "4";
			public const string ExemptionReason6 = "6";
		}

		public static class Descriptions
		{
			public static string ExemptionReason4 { get { return Res.GetString("4f3a6e22-3f00-4927-80b0-7b6b02efd92e", "•Delivery by road  •Annex 30A was submitted with a customs declaration before arrival in Hamburg.The word “Export” or the MRN (for a different Declaration for Transit) is stated on the Declaration for Transit as proof.  •Place of destination and Consignee have not been changed (from Declaration for Transit issued prior to arrival in Hamburg with Annex 30A data)  •Goods will be cleared again in a non-EC port"); } }
			public static string ExemptionReason6 { get { return Res.GetString("bea51b82-3968-4cf8-a21b-c7a89cd71fc4", "•Declaration for Transit is not available in digital format  •The word “EXPORT” is not included in the Declaration for Transit  •Exit summary declaration data is included in the Declaration for Transit (inc. no change to details for Consignee and Destination Country/Region)  •Waltershof is declared as the customs office at destination in the Declaration for Transit  •Goods will be cleared again in a non-EC port"); } }
		}
	}
}
