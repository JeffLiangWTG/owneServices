using System;

namespace CargoWise.Billing.Collectors
{
	public static class RefStlItemGrain
	{
		public const string Transactional = "TRN";
		public const string MonthlyAllowHistoricalData = "MAH";
		public const string MonthlyCurrentDataOnly = "MCO";
		public const string Daily = "DAY";
		public const string Snapshot = "SPS";
	}

	public static class RefStlDateType
	{
		public const string DateTime = "DTE";
		public const string DateTimeOffset = "DTO";
		public const string SmallDateTime = "SDT";
	}

	public static class RefActiveOn
	{
		public const string All = "ALL";
		public const string None = "NON";
		public const string ProductionOnly = "PRD";
		public const string TestOnly = "TST";
	}

	public interface IRefStlScript
	{
		string ActiveOn { get; }
		string FeatureCode { get; }
		string FeatureName { get; }
		string DataGranularity { get; }
		string TransactionDateUtc { get; }
		string GuidReference { get; }
		string TransactionCount { get; }
		string FromClause { get; }
		string RoleName { get; }
		string ModuleName { get; }
		string FunctionName { get; }
		string CompanyCode { get; }
		string BranchCode { get; }
		string BillingReference1 { get; }
		string BillingReference2 { get; }
		string BillingReference3 { get; }
		string BillingReference4 { get; }
		string WhereClause { get; }
		string CreatingUserCode { get; }
		string AdditionalRefs { get; }
		string PreparationScript { get; }
		bool WithOptionRecompile { get; }
		bool UsedInBilling { get; }
		string MinCW1Version { get; }
		string MaxCW1Version { get; }
		string DateType { get; }
		DateTime CollectionStartDateUtc { get; }
	}
}
