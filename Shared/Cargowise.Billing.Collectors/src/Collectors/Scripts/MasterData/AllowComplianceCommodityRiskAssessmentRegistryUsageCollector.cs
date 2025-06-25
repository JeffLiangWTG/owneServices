using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class AllowComplianceCommodityRiskAssessmentRegistryUsageCollector : RefStlScriptWithDefaults
{
	public override string FeatureCode => "ROV";
	public override string FeatureName => "Get AllowComplianceCommodityRiskAssessment Registry Value Override Status";
	public override string RoleName => "Get Registry";
	public override string ModuleName => "Compliance Risk";
	public override string FunctionName => "Get Registry";
	public override string DataGranularity => RefStlItemGrain.Daily;
	public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;
	public override string TransactionDateUtc => "SD_SystemLastEditTimeUtc";
	public override string BillingReference1 => "ISNULL(CAST(SD_binaryValue AS NVARCHAR(5)), 'True')";
	public override string FromClause => "dbo.StmData";
	public override string WhereClause => "SD_Name = 'AllowComplianceCommodityRiskAssessment'";
	public override bool UsedInBilling => false;
	public override string MinCW1Version => "24.10.30.841";
}
