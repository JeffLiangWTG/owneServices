namespace CargoWise.Billing.Collectors.MasterData
{
	public class ComplianceRiskStatusOverrideCollector : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;

		public override string FeatureCode => "COR";

		public override string ModuleName => "Compliance Risk";

		public override string FeatureName => "Compliance Risk Override Report";

		public override string FunctionName => "Compliance Risk Override Tracking";

		public override string RoleName => "Compliance Risk Status Override";

		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => "GC_Code";

		public override string BranchCode => "GB_Code";

		public override string TransactionDateUtc => "SCE_EventTimeOffset";

		public override string CreatingUserCode => "SCE_SystemCreateUser";

		public override string GuidReference => "SCE_PK";

		public override string BillingReference1 => "JobNumber";

		public override string BillingReference2 => "Origin";

		public override string BillingReference3 => "Destination";

		public override string BillingReference4 => "(SELECT COUNT(0) FROM [dbo].[StmComplianceEvent] A WHERE [SCE_EventType] = 'STU' AND [SCE_EventSubType] = 'OVL' AND [SCE_NewValue] = 'OVR' AND A.SCE_ParentID = CORLogs.SCE_ParentID AND A.SCE_EventTimeOffset <= CORLogs.SCE_EventTimeOffset)";

		public override string DateType => RefStlDateType.DateTimeOffset;

		public override string FromClause => @"
			CORLogs
			LEFT JOIN [dbo].[GlbStaff] gs ON [SCE_SystemCreateUser] = [GS_Code]
			LEFT JOIN [dbo].[GlbBranch] gb ON gb.[GB_PK] = ISNULL(gs.[GS_GB_HomeBranch], gs.[GS_GB_LastLogonBranch])
			LEFT JOIN [dbo].[GlbCompany] gc ON gc.[GC_PK] = gb.[GB_GC]";

		public override string WhereClause => "";

		public override string MinCW1Version => "24.7.12.0";

		public override string PreparationScript => $@"
			WITH CORLogs AS
			(
				SELECT [SCE_PK], [SCE_EventTimeOffset], [TH_QuoteNumber] AS JobNumber, [JS_RL_NKOrigin] AS Origin, [JS_RL_NKDestination] as Destination, [SCE_SystemCreateUser], [SCE_ParentID]
				FROM
				[dbo].[StmComplianceEvent]
				JOIN [dbo].[RatingHeader] ON [TH_PK] = [SCE_ParentID]
				AND [SCE_ParentTableCode] = 'TH'
				AND [SCE_EventType] = 'STU' AND [SCE_EventSubType] = 'OVL' AND [SCE_NewValue] = 'OVR'
				LEFT JOIN [dbo].[JobShipment] ON [TH_PK] = [JS_TH_OneTimeQuote]
				WHERE [SCE_EventTimeOffset] >= {Constants.StartDateTimeInclusiveParamName}
				AND [SCE_EventTimeOffset] < {Constants.EndDateTimeExclusiveParamName}

				UNION ALL

				SELECT [SCE_PK], [SCE_EventTimeOffset], [JS_UniqueConsignRef] as JobNumber, [JS_RL_NKOrigin] AS Origin, [JS_RL_NKDestination] as Destination, [SCE_SystemCreateUser], [SCE_ParentID]
				FROM
				[dbo].[StmComplianceEvent]
				JOIN [dbo].[JobShipment] ON [JS_PK] = [SCE_ParentID]
				AND [SCE_ParentTableCode] = 'JS'
				AND [SCE_EventType] = 'STU' AND [SCE_EventSubType] = 'OVL' AND [SCE_NewValue] = 'OVR'
				WHERE [SCE_EventTimeOffset] >= {Constants.StartDateTimeInclusiveParamName}
				AND [SCE_EventTimeOffset] < {Constants.EndDateTimeExclusiveParamName}

				UNION ALL

				SELECT [SCE_PK], [SCE_EventTimeOffset], [JK_UniqueConsignRef] as JobNumber, [JK_RL_NKLoadPort] AS Origin, [JK_RL_NKDischargePort] as Destination, [SCE_SystemCreateUser], [SCE_ParentID]
				FROM
				[dbo].[StmComplianceEvent]
				JOIN [dbo].[JobConsol] ON [JK_PK] = [SCE_ParentID]
				AND [SCE_ParentTableCode] = 'JK'
				AND [SCE_EventType] = 'STU' AND [SCE_EventSubType] = 'OVL' AND [SCE_NewValue] = 'OVR'
				WHERE [SCE_EventTimeOffset] >= {Constants.StartDateTimeInclusiveParamName}
				AND [SCE_EventTimeOffset] < {Constants.EndDateTimeExclusiveParamName}
			)";
	}
}
