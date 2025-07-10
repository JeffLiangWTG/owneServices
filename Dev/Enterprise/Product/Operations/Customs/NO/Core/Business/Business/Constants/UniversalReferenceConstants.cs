namespace Enterprise.Customs.NO.Business;

// TODO: order nested classes alphabetically
public static class UniversalReferenceConstants
{
	public static class RefCusTaxOrFee
	{
		public const string MV1 = "MV1";
		public const string MV2 = "MV2";
		public const string MVF = "MVF";
		public const string MVK = "MVK";
	}

	public static class ChargeTypes
	{
		public const string VGE_ProcedureCode = "6021";
	}

	public static class OrgHeaderType
	{
		public const string NaturalPerson = "NAT";
		public const string Organization = "BUS";
	}

	public static class OrgCodeType
	{
		public const string SocialSecurityNumber = "SSN";
		public const string OrganizationNumber = "ORG";
		public const string MVARegistrationNumber = "MVA";
		public const string DefermentApprovalNumber = "DAN";
		public const string EmmaSystemIdentifier = "EMD";
	}

	public static class PackageTypes
	{
		public const string PackageInGeneral = "PK";
		public const string VehicleChassisNumber = "VN";
	}

	// TODO: fix typo
	public static class TradeGroupCodeConstans
	{
		public const string CodeTEF = "TEF";
		public const string CodeTEFT = "TEFT";
		public const string CodeTOES = "TOES";
	}

	public static class RefCusCodeListType
	{
		public const string PreviousDocumentsForManifestBill = "DC40M";
	}

	public static class RefCusCodeList
	{
		public static class CustomsUq
		{
			public static class Alcohol
			{
				public const string AlcoholStrength = "ASV";
			}
		}
	}

	public static class CusEntryStatus
	{
		public const string MEM = "MEM";
		public const string MEC = "MEC";
		public const string MED = "MED";
		public const string MEG = "MEG";
		public const string IUR = "IUR";
		public const string NA = "NA";
		public const string UAR = "UAR";
		public const string TKR = "TKR";
		public const string Rejected = "REJ";
	}

	public const string ErrorCodesPrefix = "9";
	public const string RateGivenInFractionsOfKroner = "RateGivenInFractionsOfKroner";

	public static class NatureOfTransactions
	{
		public const string Code01 = "01";
	}
}
