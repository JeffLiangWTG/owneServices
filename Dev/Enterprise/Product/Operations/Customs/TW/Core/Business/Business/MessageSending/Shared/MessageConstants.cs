namespace Enterprise.Customs.TW.Business
{
	public static class MessageConstants
	{
		public const string TWCustoms = "TWCustoms";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string FunctionalReferenceIDPlaceHolder = "<<FUNCTIONAL REFERENCE ID PLACE HOLDER>>";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string FunctionalReferenceIDPlaceHolderHtml = "&lt;&lt;FUNCTIONAL REFERENCE ID PLACE HOLDER&gt;&gt;";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string EntryNumberPlaceHolder = "<!-- placeholder:EntryNumber -->";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to be translated")]
		public const string NoBrand = "NO BRAND";
		public const string NoEnglishName = "NEN";
		public const string Company = "COMPANY";
		public const string CO = "CO";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		public const string TaiWan = "臺灣";
		public const string RoleCodeCB = "CB";

		public static class TransportEquipmentsCapacityCodes
		{
			public const string Zero = "0";
			public const string One = "1";
			public const string Two = "2";
			public const string Three = "3";
			public const string Four = "4";
			public const string Five = "5";
			public const string Six = "6";
		}

		public static class TransportContractDocumentTypeCodes
		{
			public const string _703 = "703";
			public const string _704 = "704";
			public const string _714 = "714";
			public const string _741 = "741";
			public const string _976 = "976";
			public const string NIL = "NIL";
		}

		public static class IdentificationTypeCodes
		{
			public const string HS = "HS";
			public const string ZZZ = "ZZZ";
			public const string SSO = "SSO";
			public const string BP = "BP";
			public const string SA = "SA";
		}

		public static class RoleCodes
		{
			public const string CustomsBroker = "CB";
		}

		public static class LPCOAuthorizedParty
		{
			public const string TWAEO = "TWAEO";
		}

		public static class BorderTransportMeansTypeCodes
		{
			public const string _1 = "1";
			public const string _4 = "4";
		}

		public static class CustomsValuationPartyRelationshipCode
		{
			public const string _135 = "135";
			public const string _136 = "136";
			public const string _137 = "137";
			public const string _138 = "138";
		}

		public static class TransportMeansJourneyIdTypes
		{
			public const string NIL = "NIL";
		}

		public static class CommunicationTypeIDs
		{
			public const string TE = "TE";
			public const string MA = "MA";
			public const string FX = "FX";
		}

		public static class TransportTypeCodes
		{
			public const string Import = "1";
			public const string Export = "2";
			public const string Transfer = "3";
		}

		public static class DocumentTypes
		{
			public const string PKL = "PKL";
			public const string CAT = "CAT";
			public const string CIV = "CIV";
			public const string IEP = "IEP";
			public const string TDM = "TDM";

			public static class Codes
			{
				public const string PKL = "1";
				public const string CAT = "2";
				public const string CIV = "3";
				public const string IEP = "4";
				public const string TDM = "5";
				public const string Others = "9";
			}
		}

		public static class MethodCodes
		{
			public const string _1 = "1";
			public const string _2 = "2";
		}
	}
}
