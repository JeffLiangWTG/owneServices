using System;

namespace CargoWise.Billing.Collectors.MasterData;

public abstract class CmdCollector: RefStlScriptWithDefaults
{
	public override bool UsedInBilling => false;
	public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
	public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
	public override string TransactionCount => "1";
	public override string PreparationScript => Constants.MonthAsDateVariableScript + $@"
				;WITH gb_org_header AS
				(
					SELECT DISTINCT OH_PK 
					FROM (
						SELECT GC_OH_OrgProxy as OH_PK
						FROM [dbo].[GlbCompany]

						UNION 

						SELECT GB_OH_OrgProxy as OH_PK
						FROM [dbo].[GlbBranch]
					) as h
					WHERE OH_PK IS NOT NULL
				),
				{QueryScript}

";
	public override string RoleName => "Logistics Services";
	public override string ModuleName => "Centralized Master Data";
	public override string FunctionName => "Org Related Records Tracker";
	public override string BillingReference1 => string.Empty;
	public override string WhereClause => string.Empty;
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;

	protected abstract string QueryScript { get; }
	}
