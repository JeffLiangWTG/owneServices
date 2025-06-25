namespace CargoWise.Billing.Collectors.MasterData
{
	public class ComplianceRiskUsageCollector : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CPW";

		public override string RoleName => "Compliance Risk Initiated";

		public override string ModuleName => "Compliance Risk";

		public override string FunctionName => "CPW Usage Tracking And Billing";

		public override string FeatureName => "Compliance Risk";

		public override string DataGranularity => RefStlItemGrain.Transactional;

		public override string CompanyCode => "GC_Code";

		public override string BranchCode => "GB_Code";

		public override string TransactionDateUtc => "SCE_EventTimeOffset";

		public override string CreatingUserCode => "SCE_SystemCreateUser";

		public override string GuidReference => "SCE_PK";

		public override string BillingReference1 => "Ref1";

		public override string BillingReference2 => "Ref2";

		public override string BillingReference3 => "Ref3";

		public override string BillingReference4 => "Ref4";

		public override string FromClause => @"
				CRILogs
				LEFT JOIN [dbo].[GlbStaff] gs ON [SCE_SystemCreateUser] = [GS_Code]
				LEFT JOIN [dbo].[GlbBranch] gb ON gb.[GB_PK] = ISNULL(gs.[GS_GB_HomeBranch], gs.[GS_GB_LastLogonBranch])
				LEFT JOIN [dbo].[GlbCompany] gc ON gc.[GC_PK] = gb.[GB_GC]";

		public override string WhereClause => "";

		public override string MinCW1Version => "24.7.12.0";

		public override string DateType => RefStlDateType.DateTimeOffset;

		public override string PreparationScript => @"
WITH CRILogs AS
(
	SELECT [SCE_PK], [SCE_EventTimeOffset], [TH_QuoteNumber] AS Ref1, [JS_UniqueConsignRef] as Ref2, CONVERT(VARCHAR(10), [TH_SystemCreateTimeUtc], 120) as Ref3, [SCE_ParentTableCode] as Ref4, [SCE_SystemCreateUser]
	FROM
	[dbo].[StmComplianceEvent]
	JOIN [dbo].[RatingHeader] ON [TH_PK] = [SCE_ParentID]
	AND [SCE_EventType] = 'CRI' AND [SCE_EventSubType] = 'CAI'
	LEFT JOIN [dbo].[JobShipment] ON [TH_PK] = [JS_TH_OneTimeQuote]

	UNION ALL

	SELECT [SCE_PK], [SCE_EventTimeOffset], [JS_UniqueConsignRef] as Ref1, '' AS Ref2, CONVERT(VARCHAR(10), [JS_SystemCreateTimeUtc], 120) as Ref3, [SCE_ParentTableCode] as Ref4, [SCE_SystemCreateUser]
	FROM
	[dbo].[StmComplianceEvent]
	JOIN [dbo].[JobShipment] ON [JS_PK] = [SCE_ParentID]
	AND [SCE_EventType] = 'CRI' AND [SCE_EventSubType] = 'CAI'
)";
	}
}
