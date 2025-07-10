namespace Enterprise.Customs.ZA.Business.MessageBuilders.COSTCO
{
	public static class Contants
	{
		public static class ImportExportTranshipmentIndicator
		{
			public const string Import23 = "929";
			public const string Export22 = "830";
			public const string Transhipment28 = "399";
			public const string Transit24 = "950";
		}

		public static class TransportCode
		{
			public const string Sea = "1";
			public const string Rail = "2";
			public const string Road = "3";
			public const string Air = "4";
		}

		public static class ManifestType
		{
			public const string Import23 = "23";
			public const string Export22 = "22";
			public const string Transhipment28 = "28";
			public const string Transit24 = "24";
			public const string FreightRemainingOnBoard = "57";
		}

		public static class EquipmentType
		{
			public const string BreakBulk = "BB";
			public const string Container = "CN";
			public const string UnitLoadDevice = "UL";
		}

		public static class ContainerStatusLandedPurpose
		{
			public const string Import23 = "3";
			public const string Export22 = "2";
			public const string Transhipment28 = "6";
			public const string Transit24 = "1";
		}

		public static class ServiceType
		{
			public const string EmptyContainer = "4";
			public const string FullContainerLoad = "5";
			public const string LessThanFullContainerLoad = "7";
		}

		public static class ConsolidationIndicator
		{
			public const string Consol = "CO";
			public const string Straight = "ST";
		}

		public static class SealNumber
		{
			public const string NoSealNo = "NO SEAL NO";
		}

		public static class SealingParty
		{
			public const string Unknown = "AB";
			public const string Carrier = "CA";
			public const string Customs = "CU";
			public const string Shipper = "SH";
			public const string TerminalOperator = "TO";
		}

		public static class SealStatus
		{
			public const string True = "1";
			public const string False = "2";
		}

		public static class TypeOfPackages
		{
			public const string Liquid = "VL";
			public const string Bulk = "VQ";
		}

		public static class CargoTypeIndicator
		{
			public const string Containerised = "CN";
			public const string BreakBulk = "BB";
			public const string Liquid = "LB";
			public const string Bulk = "DB";
		}
	}
}
