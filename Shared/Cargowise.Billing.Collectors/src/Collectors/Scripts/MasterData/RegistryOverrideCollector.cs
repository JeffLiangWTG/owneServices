using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class RegistryOverrideCollector : RefStlScriptWithDefaults
{
	public override string FeatureCode => "ROC";
	public override string FeatureName => "Registry Override";
	public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
	public override string RoleName => "Registry Web Service Url Override";
	public override string ModuleName => "Registry Override";
	public override string FunctionName => "Registry Web Service Url Override";
	public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
	public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
	public override string FromClause => "CT";
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;
	public override string BillingReference1 => "Result";
	public override string WhereClause => string.Empty;
	public override bool UsedInBilling => false;
	public override string ActiveOn => "ALL";
	public override string PreparationScript => Constants.MonthAsDateVariableScript + @";
	WITH CT AS (
		SELECT 
			CASE 
				WHEN NOT EXISTS (SELECT 1 FROM dbo.StmData WHERE SD_Name = 'DeniedPartyScreeningWebService') THEN 0
				ELSE (SELECT TOP 1 CASE WHEN SD_IsLogged = 1 THEN 1 ELSE 0 END 
					  FROM dbo.StmData 
					  WHERE SD_Name = 'DeniedPartyScreeningWebService')
			END AS Result
	)";
}
