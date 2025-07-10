using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IGLAccount
	{
		ZPropertyInfo ControlAccountInfo { get; }
		ZPropertyInfo PercentNumInfo { get; }
		ZPropertyInfo ConsolidationNumInfo { get; }
		ZPropertyInfo AlternateNumInfo { get; }
		ZPropertyInfo TotalLevelInfo { get; }
		ZPropertyInfo HeaderDependsOnTotalInfo { get; }
		ZPropertyInfo CarriedForwardInfo { get; }
		ZPropertyInfo AccountNumInfo { get; }
		ZPropertyInfo StatisticalUnitsInfo { get; }

		ZString AccountNum { get; }
		ZString AccountNumWithPrefix { get; }
		ZString GLAccountFormat { get; }
		ZGuid ConsolidationAccount { get; }
		ZGuid TotalReferenceAccount { get; }

		ZGuid PK { get; }
	}

	public static class IGLAccountSchema
	{
		public const string ControlAccountInfo = "ControlAccountInfo";
		public const string PercentNumInfo = "PercentNumInfo";
		public const string ConsolidationNumInfo = "ConsolidationNumInfo";
		public const string AlternateNumInfo = "AlternateNumInfo";
		public const string TotalLevelInfo = "TotalLevelInfo";
		public const string HeaderDependsOnTotalInfo = "HeaderDependsOnTotalInfo";
		public const string CarriedForwardInfo = "CarriedForwardInfo";
		public const string AccountNumInfo = "AccountNumInfo";
		public const string StatisticalUnitsInfo = "StatisticalUnitsInfo";
	}
}
