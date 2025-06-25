namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class DddStlBillingCollector : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "DDD";
		public override string RoleName => "Delivery Due Date on Forwarding Shipment/Booking";
		public override string ModuleName => "Forwarding Shipment and Booking";
		public override string FunctionName => "Delivery Due Date";
		public override string FeatureName => "Delivery Due Date Calculator";

		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string TransactionCount => "1";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";

		public override string TransactionDateUtc => "DDELogWithBranch.SL_PostedTimeUtc";
		public override string GuidReference => "DDELogWithBranch.SL_PK";
		public override string CreatingUserCode => "DDELogWithBranch.SL_GS_NKUser";

		public override string BillingReference1 => "JS_UniqueConsignRef";
		public override string BillingReference2 => "JS_HouseBill";
		public override string BillingReference3 => "ISNULL(JS_HBLContainerPackModeOverride, '') + ' [' + JS_TransportMode + ']'";
		public override string BillingReference4 => "JS_RS_NKServiceLevel";

		public override string PreparationScript =>
		$@"WITH DDELog AS (
		SELECT
			SL_Parent,
			SL_PostedTimeUtc,
			SL_PK,
			SL_GB_NKBranch,
			SL_GS_NKUser,
			SL_Reference,
			SL_GB_NKBranch AS OriginalBranch
		FROM (
			SELECT
				SL_Parent,
				SL_PostedTimeUtc,
				SL_PK,
				SL_GB_NKBranch,
				SL_GS_NKUser,
				SL_Reference,
				MIN(SL_PostedTimeUtc) OVER (PARTITION BY SL_Parent) AS EarliestPostedTimeUtc
			FROM dbo.StmALog WITH (NOLOCK)
			WHERE
				SL_Table = 'JobShipment'
				AND SL_SE_NKEvent = 'DDE'
				AND SL_Reference NOT LIKE '%ACT=Override%'
				AND SL_Reference LIKE '%TYP=Original%'
				AND SL_PostedTimeUtc >= @StartDateTimeInclusive
				AND SL_PostedTimeUtc < @EndDateTimeExclusive
		) logs
		WHERE EarliestPostedTimeUtc = SL_PostedTimeUtc
		),
		DDELogWithBranch AS (
			SELECT
				DDELog.*,
				JS.JS_RL_NKOrigin,
				JS.JS_UniqueConsignRef,
				JS.JS_HouseBill,
				JS.JS_HBLContainerPackModeOverride,
				JS.JS_TransportMode,
				JS.JS_RS_NKServiceLevel,
				(SELECT TOP 1 GB_Code
				 FROM dbo.GlbBranch
				 WHERE GB_RL_NKHomePort = JS.JS_RL_NKOrigin
					OR GB_PK IN (
						SELECT GY_GB 
						FROM dbo.GlbBranchExtraPorts 
						WHERE GY_RL_NKAdditionalBranchRelatedPort = JS.JS_RL_NKOrigin
					)
				 ORDER BY GB_IsActive DESC) AS PreferredBranch
			FROM DDELog
			INNER JOIN dbo.JobShipment JS ON JS.JS_PK = DDELog.SL_Parent
		)";

		public override string FromClause =>
		@"DDELogWithBranch
		INNER JOIN dbo.GlbBranch Branch 
			ON Branch.GB_Code = IIF(PreferredBranch IS NULL, OriginalBranch, PreferredBranch)
		INNER JOIN dbo.GlbCompany Company 
			ON Branch.GB_GC = Company.GC_PK";

		public override string WhereClause => string.Empty;
		public override string ActiveOn => "ALL";
		public override bool UsedInBilling => false;
	}

	#endregion
}
