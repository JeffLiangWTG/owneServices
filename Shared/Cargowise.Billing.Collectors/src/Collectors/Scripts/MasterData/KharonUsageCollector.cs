using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class KharonUsageCollector : RefStlScriptWithDefaults
{
	public override string FeatureCode => "KUC";
	public override string FeatureName => "Compliance List";
	public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
	public override string RoleName => "Kharon Usage Tracker";
	public override string ModuleName => "Compliance List";
	public override string FunctionName => "Kharon Usage Tracking";
	public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
	public override string GuidReference => "RCL_PK";
	public override string FromClause => "dbo.RefComplianceList";
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;
	public override string BillingReference1 => "CASE WHEN RCL_IsExcluded = 1 THEN 'Excluded' ELSE 'Included' END";
	public override string WhereClause => "RCL_ListCode = 'KN-SANOWN'";
	public override bool UsedInBilling => false;
	public override string PreparationScript => Constants.MonthAsDateVariableScript;
}
