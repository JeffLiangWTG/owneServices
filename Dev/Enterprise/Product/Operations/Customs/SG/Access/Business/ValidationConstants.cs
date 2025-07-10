using CargoWise.Types;

namespace Enterprise.Customs.SG.Access.Business
{
	public static class ValidationConstants
	{
		public static class Bill
		{
			public static string SGGoodsValueNotMatchTotalLinePrice => ResString.GetMultilingualString("{0EABA0D2-EA4F-471A-856B-ED38A3F5239A}", "The Goods Value does not equal the sum of the Pack Lines Price. This may cause a valuation error for Singapore.");
			public static string SGGoodsValueCurrencyNotSameWithLinePriceCurrency => ResString.GetMultilingualString("{25FDC96B-E61F-4C33-B023-794D5E9C61CD}", "Cannot verify that the sum of the Pack Lines Price matches the Goods Value as there are in different currency. This may cause a valuation error for Singapore.");
			public static string SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount => ResString.GetMultilingualString("47953F54-B498-41D1-BC26-9735E513CE43", "A TradeNet Permit is required for this client.  As the consignee has an IBG account linked to it.");
			public static string GSTRegistrationNoMustNotBeBlank => ResString.GetMultilingualString("342B70D2-F263-4D6A-8C67-B960DEBB7672", "The GST Registration No must be entered when GST Paid is entered against any Pack Line.");
			public static string GSTPaidFlagMustNotBeBlank => ResString.GetMultilingualString("2B91F86F-6AC0-4E7E-BABB-16180D729C00", "The GST Paid indicator must be entered when GST Registration No is entered against the Bill.");
		}

		public static string SGManifestValidOnlyForAirAndRoad
		{
			get { return ResString.GetMultilingualString("{A9DBCAD9-1A27-49FF-BA5A-FD1FB4C5E2D7}", "Singapore Manifests are only valid for Transport Mode Air or Road."); }
		}

		public static string SGRoadManifestValidOnlyForMalaysia
		{
			get { return ResString.GetMultilingualString("{4B4E0A8C-6D7F-490D-A28F-089B1DC71EE2}", "Singapore 'Road' Manifests are only valid for Malaysian Loading or Discharge ports."); }
		}

		public static string PortMustBeSingaporePortForExport(string fieldDescription)
		{
			return ResString.GetMultilingualString("{F477363A-1524-4C65-BDC5-22FAD64E9285}", "{0} must be a Singapore port for an Export job.", fieldDescription);
		}

		public static string PortCannotBeSingaporePortForImport(string fieldDescription)
		{
			return ResString.GetMultilingualString("{665412A0-F33D-4327-8AD9-2636E0F0F5D9}", "{0} cannot be a Singapore port for an Import job.", fieldDescription);
		}

		public static string PortCannotSingaporePortForExport(string fieldDescription)
		{
			return ResString.GetMultilingualString("{A3F9F73B-5F6D-4352-A565-C33880C1691C}", "{0} cannot be a Singapore port for an Export job.", fieldDescription);
		}

		public static string PortMustBeSingaporePortForImport(string fieldDescription)
		{
			return ResString.GetMultilingualString("{2D4B73E2-FFFA-4318-8E38-B07185CF620F}", "{0} must be a Singapore port for an Import job.", fieldDescription);
		}

		public static string AccessCredentialsNotSetUp
		{
			get { return ResString.GetMultilingualString("{507200D6-6035-4789-B3D5-498BBF3242B2}", "You cannot send the SG Manifest, your Staff record does not have SG ACCESS credentials set up."); }
		}

		public static string InvalidSGAccessCredentials(ZString status)
		{
			var statusToDisplay = status.IsEmpty ? new ZString("blank") : status;
			return ResString.GetMultilingualString("{F0BE8E12-48E0-4196-B89C-0ECBC0E1AE9E}", "You cannot send the SG Manifest, your Staff SG ACCESS credentials password status is currently {0}.", statusToDisplay);
		}
	}
}
