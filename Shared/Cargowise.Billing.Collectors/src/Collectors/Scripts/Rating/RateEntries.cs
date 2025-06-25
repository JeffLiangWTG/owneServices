namespace CargoWise.Billing.Collectors.Rating
{
	#region SuppressResourceStringsCheckRegion

	public class RateEntries : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "RRH";
		public override string RoleName => "Rating";
		public override string ModuleName => "Rating Headers";
		public override string FunctionName => "Rate Entries Count";
		public override string FeatureName => "Rate Entries Count";
		public override string CompanyCode => "gc.gc_code";
		public override string BranchCode => string.Empty;
		public override string TransactionDateUtc => $"DATEFROMPARTS(YEAR({Constants.EndDateTimeExclusiveParamName}), MONTH({Constants.EndDateTimeExclusiveParamName}), DAY({Constants.EndDateTimeExclusiveParamName}))";
		public override string GuidReference => "th.TH_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"RatingHeader th
				LEFT JOIN dbo.GlbCompany gc ON gc.GC_PK = TH.TH_GC
				INNER JOIN dbo.RateEntry ti ON ti.TI_TH = th.TH_PK";

		public override string WhereClause => $@"th.TH_IsCancelled = 0 AND ti.TI_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND ti.TI_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
				GROUP BY th.TH_PK, th.TH_RateType, gc.gc_code, ti.TI_RateCategory, ti.TI_Mode, ti.TI_CreationSource";

		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
				(
					SELECT
						RateType = th.TH_RateType,
						RateCategory = ti.TI_RateCategory,
						RateMode = ti.TI_Mode,
						CreationSource = ti.TI_CreationSource
					FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
				)))";
		public override string TransactionCount => "COUNT(DISTINCT ti.TI_PK)";
		public override string BillingReference1 => "th.TH_RateType";
		public override string BillingReference2 => "ti.TI_RateCategory";
		public override string BillingReference3 => "ti.TI_Mode";
		public override string BillingReference4 => "ti.TI_CreationSource";

		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTime;
	}
	#endregion
}
