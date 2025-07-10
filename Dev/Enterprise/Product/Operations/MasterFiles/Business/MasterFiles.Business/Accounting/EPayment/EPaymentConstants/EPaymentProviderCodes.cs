using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class EPaymentProviderCodes
	{
		public static class Codes
		{
			public const string OFX = "OFX";

			public static string[] PaymentReasonMandatoryProviders => new[] { OFX };
		}

		public static CodeDescriptionPairList CodesList
		{
			get
			{
				var codes = new CodeDescriptionPairList();
				codes.Add(new CodeDescriptionPair(Codes.OFX, ResString.GetMultilingualString("942DDCF0-6F73-4598-9C4F-8813CB61D05D", Codes.OFX)));
				return codes;
			}
		}
	}
}
