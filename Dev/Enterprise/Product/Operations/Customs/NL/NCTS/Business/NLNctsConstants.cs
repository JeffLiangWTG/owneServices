using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.NL.NCTS.Business;

public static class NLNctsConstants
{
	public static class Messaging
	{
		public const string NCT = "NCT";
	}

	public static class RecepientTypes
	{
		public const string NTA = "NTA";
	}

	public static class NctsMessageTypes
	{
		public static class Incoming
		{
			public const string CC004C = "004";
			public const string CC006C = "006";
			public const string CC009C = "009";
			public const string CC019C = "019";
			public const string CC022C = "022";
			public const string CC025C = "025";
			public const string CC028C = "028";
			public const string CC029C = "029";
			public const string CC043C = "043";
			public const string CC045C = "045";
			public const string CC051C = "051";
			public const string CC055C = "055";
			public const string CC056C = "056";
			public const string CC057C = "057";
			public const string CC060C = "060";
			public const string CC061C = "061";
			public const string CC140C = "140";
			public const string CC182C = "182";
			public const string CC906C = "906";
			public const string CC917C = "917";
			public const string CC928C = "928";
		}
	}

	public static class ReleaseIndicator
	{
		public const int FullRelease = 1;
		public const int PartialRelease = 2;
		public const int PartialReleaseClosed = 3;
		public const int NoRelease = 4;
	}

	public static class ReleaseType
	{
		public const int PartialRelease = 1;
		public const int FullRelease = 2;
	}

	public static class Logs
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant string")]
		public static class References
		{
			public const string CC006CMessageReceived = "CC006C Message received.";
			public const string CC045CMessageReceived = "CC045C Message received.";
		}
	}

	public static class GoodsLocationParentTableCodes
	{
		public const string Incident = "BN";
	}

	public static class TaxOrFeeCodes
	{
		public const string C414 = "C414";
		public const string C480 = "C480";
		public const string C400 = "C400";
		public const string C114 = "C114";
		public const string C180 = "C180";
		public const string C100 = "C100";
		public const string C000 = "C000";
	}

	public static class DocumentWrapper
	{
		public const string OfficeOfDepartureCode = "0074";
	}
}
