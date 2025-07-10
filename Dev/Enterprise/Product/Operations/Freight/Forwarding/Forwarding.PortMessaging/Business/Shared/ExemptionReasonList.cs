using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class ExemptionReasonList : CodeDescriptionPairList
	{
		public ExemptionReasonList(string entryType)
		{
			if (entryType == EntryTypeList.Codes.Message)
			{
				this.AddPair(Codes.ExemptionReason0, Descriptions.ExemptionReason0);
				this.AddPair(Codes.ExemptionReason2, Descriptions.ExemptionReason2);
				this.AddPair(Codes.ExemptionReason3, Descriptions.ExemptionReason3);
			}
			else if (entryType == EntryTypeList.Codes.OtherExemptions)
			{
				this.AddPair(Codes.ValueOver1000Euro, Descriptions.ValueOver1000Euro);
				this.AddPair(Codes.NoOtherExemptions, Descriptions.NoOtherExemptions);
				this.AddPair(Codes.OtherExemptions, Descriptions.OtherExemptions);
			}
		}

		public static class Codes
		{
			public const string ExemptionReason0 = "0";
			public const string ExemptionReason2 = "2";
			public const string ExemptionReason3 = "3";

			public const string ValueOver1000Euro = "V";
			public const string NoOtherExemptions = "L";
			public const string OtherExemptions = "O";
		}

		public static class Descriptions
		{
			public static string ExemptionReason0 { get { return Res.GetString("6933da33-717b-476b-9da4-30571e819577", "•Delivery by sea  •Hamburg transhipments completed within 14 days  •Annex 30A data submitted with a Declaration for Transit before arrival in Hamburg  •Goods will be cleared again in a non-EC port"); } }
			public static string ExemptionReason2 { get { return Res.GetString("6ef1f31a-53bf-4639-9dfb-55eccc6135bb", "•Specified goods from article 592a ZK-DVO  •Goods will be cleared again in a non-EC port"); } }
			public static string ExemptionReason3 { get { return Res.GetString("6ef59ff2-c083-44db-92e6-752351000153", "•Goods will be cleared in Norway"); } }

			public static string ValueOver1000Euro { get { return Res.GetString("dc762fcc-9875-4473-9aef-a3470dcca356", "Value > 1000 EUR, Other Exemptions Exist"); } }
			public static string NoOtherExemptions { get { return Res.GetString("49b945bc-dd15-4769-b7a7-e1d6745d35ae", "Value <= 1000 EUR, No Other Exemptions"); } }
			public static string OtherExemptions { get { return Res.GetString("12915d8d-dc81-417b-b9b1-9050ceedcbde", "Value <= 1000 EUR, Other Exemptions Exist"); } }
		}
	}
}
